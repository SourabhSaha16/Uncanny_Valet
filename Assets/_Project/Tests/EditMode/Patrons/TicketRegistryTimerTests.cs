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
        private const float ParkWindow = 45f;
        private const float PickupWindow = 60f;

        private sealed class FakeClock : IGameClock
        {
            public int Night => 1;
            public float MinuteOfNight { get; set; }
            public bool IsRunning => true;
        }

        private sealed class FakeTips : ITipCalculator
        {
            public int Calls;
            public TipInput Last;
            public int Calculate(in TipInput input) { Calls++; Last = input; return input.PickupExpired ? 0 : 25; }
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
            new PatronRequest(stay, 20, false, ParkWindow, PickupWindow);

        private Ticket OpenTicket(CarId car, float stay = 20f)
        {
            _cars.Spawn(car, UnityEngine.Vector2.zero, 0f);
            return _registry.Open(car, Request(stay));
        }

        private void Tick(float minute)
        {
            _clock.MinuteOfNight = minute;
            _bus.Publish(new NightTick(1, minute));
        }

        private int Count<T>() where T : IGameEvent
        {
            var n = 0;
            foreach (var e in _log) if (e is T) n++;
            return n;
        }

        [Test]
        public void ParkTimer_PublishesProgressEachMinute()
        {
            OpenTicket(_car);
            Tick(1f); Tick(2f);
            var changed = (TicketTimerChanged)_log[1];
            Assert.AreEqual(TimerKind.Park, changed.Kind);
            Assert.AreEqual(2f / ParkWindow, changed.Elapsed01, 0.0001f);
            Assert.AreEqual(2, Count<TicketTimerChanged>());
        }

        [Test]
        public void ParkTimerExpiry_PatronLeavesWithNoTip()
        {
            var ticket = OpenTicket(_car);
            for (var m = 1; m <= (int)ParkWindow; m++) Tick(m);

            Assert.AreEqual(1, Count<TicketTimerExpired>());
            var closed = (TicketClosed)_log.Find(e => e is TicketClosed);
            Assert.AreEqual(0, closed.Tip);
            Assert.AreEqual(1, Count<CarDeparted>());
            Assert.AreEqual(1, _cars.DespawnCount);
            Assert.AreEqual(TicketState.Closed, ticket.State);
            Assert.AreEqual(0, _registry.OpenCount);
            Assert.AreEqual(0, _tips.Calls);
        }

        [Test]
        public void ParkTimer_PausesWhileValetDrives()
        {
            OpenTicket(_car);
            Tick(10f);
            _bus.Publish(new CarEntered(_car));
            Tick(100f);
            Assert.AreEqual(0, Count<TicketTimerExpired>());
            _clock.MinuteOfNight = 100f;
            _bus.Publish(new CarExited(_car));
            Tick(134f);
            Assert.AreEqual(0, Count<TicketTimerExpired>());
            Tick(135f);
            Assert.AreEqual(1, Count<TicketTimerExpired>());
        }

        [Test]
        public void CarParked_StopsParkTimer()
        {
            OpenTicket(_car);
            Tick(5f);
            _bus.Publish(new CarParked(_car, 2));
            Assert.AreEqual(1, Count<TicketTimerStopped>());
            Tick(200f);
            Assert.AreEqual(0, Count<TicketTimerExpired>());
            Assert.AreEqual(1, Count<TicketTimerChanged>());
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
        public void LettersAreLowestFree_AndFreedWhenTicketCloses()
        {
            var a = OpenTicket(new CarId(1));
            var b = OpenTicket(new CarId(2));
            Assert.AreEqual('A', a.Letter);
            Assert.AreEqual('B', b.Letter);

            _bus.Publish(new CarParked(a.CarId, 1));
            Tick(20f);
            _bus.Publish(new CarAtCurb(a.CarId));

            Assert.AreEqual('A', OpenTicket(new CarId(3)).Letter);
        }

        [Test]
        public void VanishedCar_FreesItsLetterAndTicket()
        {
            var first = OpenTicket(new CarId(1));
            for (var m = 1; m <= (int)ParkWindow; m++) Tick(m);

            Assert.AreEqual('A', first.Letter);
            Assert.AreEqual(0, _registry.OpenCount);
            Assert.AreEqual(0, _registry.AwaitingParkCount);
            Assert.IsFalse(_registry.TryGet(first.TicketId, out _));
            Assert.AreEqual('A', OpenTicket(new CarId(2)).Letter);
        }
    }
}
