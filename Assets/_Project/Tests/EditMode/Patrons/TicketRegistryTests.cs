using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class TicketRegistryTests
    {
        private class FakeClock : IGameClock
        {
            public int Night => 1;
            public float MinuteOfNight { get; set; }
            public bool IsRunning => true;
        }

        private class FakeTips : ITipCalculator
        {
            public TipInput Last;
            public int Calculate(in TipInput input) { Last = input; return input.BaseTip + 5; }
        }

        private EventBus _bus;
        private FakeClock _clock;
        private FakeTips _tips;
        private TicketRegistry _registry;
        private List<IGameEvent> _log;
        private readonly CarId _car = new CarId(1);

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus(); _clock = new FakeClock(); _tips = new FakeTips();
            _registry = new TicketRegistry(_bus, _clock, _tips);
            _log = new List<IGameEvent>();
            _bus.Subscribe<PickupRequested>(e => _log.Add(e));
            _bus.Subscribe<TicketClosed>(e => _log.Add(e));
            _bus.Subscribe<TipEarned>(e => _log.Add(e));
        }

        private void Tick(float minute)
        {
            _clock.MinuteOfNight = minute;
            _bus.Publish(new NightTick(1, minute));
        }

        [Test]
        public void WholeTicket_OpenParkWaitPickupClose()
        {
            var ticket = _registry.Open(_car, new PatronRequest(30f, 20, true));
            Tick(10f);
            _bus.Publish(new CarParked(_car, 3));
            Assert.AreEqual(TicketState.Parked, ticket.State);

            Tick(39f);
            Assert.AreEqual(TicketState.Parked, ticket.State);
            Tick(40f);
            Assert.AreEqual(TicketState.PickupRequested, ticket.State);

            _clock.MinuteOfNight = 46f;
            _bus.Publish(new CarAtCurb(_car));

            Assert.AreEqual(TicketState.Closed, ticket.State);
            Assert.AreEqual(0, _registry.OpenCount);
            Assert.AreEqual(6f, _tips.Last.WaitMinutes);
            Assert.IsTrue(_tips.Last.Impatient);
            Assert.IsInstanceOf<PickupRequested>(_log[0]);
            Assert.IsInstanceOf<TicketClosed>(_log[1]);
            Assert.IsInstanceOf<TipEarned>(_log[2]);
            Assert.AreEqual(25, ((TipEarned)_log[2]).Amount);
        }

        [Test]
        public void CarAtCurb_BeforePickupRequested_IsIgnored()
        {
            var ticket = _registry.Open(_car, new PatronRequest(30f, 20, false));
            _bus.Publish(new CarAtCurb(_car));
            _bus.Publish(new CarParked(_car, 1));
            _bus.Publish(new CarAtCurb(_car));

            Assert.AreEqual(TicketState.Parked, ticket.State);
            Assert.AreEqual(0, _log.Count);
        }

        [Test]
        public void CarAlreadyAtCurb_WhenPickupRequested_ClosesImmediately()
        {
            var ticket = _registry.Open(_car, new PatronRequest(10f, 20, false));
            _bus.Publish(new CarParked(_car, 1));
            _bus.Publish(new CarAtCurb(_car));
            Tick(10f);

            Assert.AreEqual(TicketState.Closed, ticket.State);
            Assert.AreEqual(0f, _tips.Last.WaitMinutes);
            Assert.IsInstanceOf<PickupRequested>(_log[0]);
            Assert.IsInstanceOf<TicketClosed>(_log[1]);
        }

        [Test]
        public void CarEnteredAfterCurb_ClearsCurbFlag()
        {
            var ticket = _registry.Open(_car, new PatronRequest(10f, 20, false));
            _bus.Publish(new CarParked(_car, 1));
            _bus.Publish(new CarAtCurb(_car));
            _bus.Publish(new CarEntered(_car));
            Tick(10f);

            Assert.AreEqual(TicketState.PickupRequested, ticket.State);
        }

        [Test]
        public void OpeningTicketInsideHandler_DoesNotThrow()
        {
            _registry.Open(_car, new PatronRequest(5f, 20, false));
            _bus.Publish(new CarParked(_car, 1));
            _bus.Subscribe<PickupRequested>(e => _registry.Open(new CarId(2), new PatronRequest(5f, 20, false)));

            Assert.DoesNotThrow(() => Tick(5f));
            Assert.AreEqual(2, _registry.OpenCount);
        }

        [Test]
        public void NightTick_BeforePark_DoesNotRequestPickup()
        {
            var ticket = _registry.Open(_car, new PatronRequest(5f, 20, false));
            Tick(10f);
            Assert.AreEqual(TicketState.AwaitingPark, ticket.State);
            Assert.AreEqual(0, _log.Count);
        }

        [Test]
        public void UnknownCar_IsIgnored()
        {
            Assert.DoesNotThrow(() => _bus.Publish(new CarAtCurb(new CarId(99))));
        }
    }
}
