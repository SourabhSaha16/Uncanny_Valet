using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;
using UncannyValet.Tests.Integration;

namespace UncannyValet.Tests.Patrons
{
    public class TicketRegistryTimerTests
    {
        private const float PickupWindow = 60f;

        private sealed class FakeClock : IGameClock
        {
            public float SecondsRemaining => 0f;
            public float ShiftLengthSeconds => 0f;
            public float ElapsedMinutes { get; set; }
            public bool IsRunning => true;
        }

        private sealed class FakeTips : ITipCalculator
        {
            public int Calls;
            public TipInput Last;
            public int Calculate(in TipInput input) { Calls++; Last = input; return input.PickupExpired ? 0 : 25; }
            public TipBreakdown Explain(in TipInput input) { var t = Calculate(input); return new TipBreakdown(t, 0, 0, t, TipZeroReason.None); }
        }

        private EventBus _bus;
        private FakeClock _clock;
        private FakeTips _tips;
        private FakeCarFactory _cars;
        private PatronDeparture _departure;
        private TicketRegistry _registry;
        private List<IGameEvent> _log;
        private readonly CarId _car = new CarId(1);

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus(); _clock = new FakeClock(); _tips = new FakeTips();
            _cars = new FakeCarFactory(_bus);
            _registry = new TicketRegistry(_bus, _clock, _tips);
            _departure = new PatronDeparture(_bus, _cars);
            _log = new List<IGameEvent>();
            _bus.Subscribe<TicketTimerChanged>(e => _log.Add(e));
            _bus.Subscribe<TicketTimerExpired>(e => _log.Add(e));
            _bus.Subscribe<TicketTimerStopped>(e => _log.Add(e));
            _bus.Subscribe<TicketClosed>(e => _log.Add(e));
            _bus.Subscribe<CarDeparted>(e => _log.Add(e));
        }

        private PatronRequest Request(float stay = 20f) =>
            new PatronRequest(stay, 20, false, PickupWindow);

        private Ticket OpenTicket(CarId car, float stay = 20f)
        {
            _cars.Spawn(car, UnityEngine.Vector2.zero, 0f);
            var ticket = _registry.Open(car, Request(stay));
            _bus.Publish(new CarClearedQueue(car));
            return ticket;
        }

        private void Tick(float minute)
        {
            _clock.ElapsedMinutes = minute;
            _bus.Publish(new ShiftTick(minute));
        }

        private int Count<T>() where T : IGameEvent
        {
            var n = 0;
            foreach (var e in _log) if (e is T) n++;
            return n;
        }

        [Test]
        public void UnparkedCar_HasNoTimerBeforeItsStayEnds()
        {
            OpenTicket(_car);
            for (var m = 1; m < 20; m++) Tick(m);
            Assert.AreEqual(0, Count<TicketTimerChanged>());
            Assert.AreEqual(0, Count<TicketTimerExpired>());
        }

        [Test]
        public void UnparkedCar_IsStillOwed_AndNeverLeavesOnItsOwn()
        {
            var ticket = OpenTicket(_car);
            for (var m = 1; m <= 200; m++) Tick(m);

            Assert.AreEqual(TicketState.PickupRequested, ticket.State);
            Assert.AreEqual(1, _registry.OpenCount);
            Assert.AreEqual(0, Count<TicketClosed>());
            Assert.AreEqual(0, Count<CarDeparted>());
            Assert.AreEqual(0, _cars.DespawnCount);
        }

        [Test]
        public void CarParked_PublishesNoTimerEvents()
        {
            OpenTicket(_car);
            Tick(5f);
            _bus.Publish(new CarParked(_car, 2));
            Assert.AreEqual(0, Count<TicketTimerStopped>());
            Assert.AreEqual(0, Count<TicketTimerChanged>());
        }

        [Test]
        public void PickupTimerExpiry_CarStillOwed_TipIsZero()
        {
            var ticket = OpenTicket(_car);
            _bus.Publish(new CarParked(_car, 1));
            _log.Clear();
            Tick(20f);
            Assert.AreEqual(TicketState.PickupRequested, ticket.State);
            Tick(20f + PickupWindow);

            Assert.AreEqual(1, Count<TicketTimerExpired>());
            Assert.IsTrue(ticket.PickupExpired);
            Assert.AreEqual(TicketState.PickupRequested, ticket.State);

            _bus.Publish(new CarAtCurb(_car));
            Assert.IsTrue(_tips.Last.PickupExpired);
            Assert.AreEqual(0, ((TicketClosed)_log.Find(e => e is TicketClosed)).Tip);
            Assert.AreEqual(1, Count<TicketTimerStopped>());
        }

        [Test]
        public void PickupDeliveredInTime_StopsTimerAndIsNotExpired()
        {
            OpenTicket(_car);
            _bus.Publish(new CarParked(_car, 1));
            _log.Clear();
            Tick(20f); Tick(30f);
            _bus.Publish(new CarAtCurb(_car));

            Assert.IsFalse(_tips.Last.PickupExpired);
            Assert.AreEqual(1, Count<TicketTimerStopped>());
            Tick(500f);
            Assert.AreEqual(0, Count<TicketTimerExpired>());
        }

        [Test]
        public void TagsAreLowestFree_AndFreedWhenTicketCloses()
        {
            var a = OpenTicket(new CarId(1));
            var b = OpenTicket(new CarId(2));
            Assert.AreEqual(0, a.Tag);
            Assert.AreEqual(1, b.Tag);

            _bus.Publish(new CarParked(a.CarId, 1));
            Tick(20f);
            _bus.Publish(new CarAtCurb(a.CarId));

            Assert.AreEqual(0, OpenTicket(new CarId(3)).Tag);
        }

        [Test]
        public void ShiftEnd_ClosesAnUnparkedTicket_AndFreesItsTag()
        {
            var first = OpenTicket(new CarId(1));
            _bus.Publish(new ShiftEnded());

            Assert.AreEqual(0, first.Tag);
            Assert.AreEqual(0, _registry.OpenCount);
            Assert.AreEqual(0, _registry.AwaitingParkCount);
            Assert.IsFalse(_registry.TryGet(first.TicketId, out _));
            Assert.AreEqual(0, OpenTicket(new CarId(2)).Tag);
        }
    }
}
