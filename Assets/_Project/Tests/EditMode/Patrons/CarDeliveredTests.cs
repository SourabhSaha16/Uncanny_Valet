using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;
using UncannyValet.Tests.Integration;

namespace UncannyValet.Tests.Patrons
{
    public class CarDeliveredTests
    {

        private sealed class FakeClock : IGameClock
        {
            public float SecondsRemaining => 0f;
            public float ShiftLengthSeconds => 0f;
            public float ElapsedMinutes { get; set; }
            public bool IsRunning => true;
        }

        private sealed class FakeBlessings : IBlessingCalculator
        {
            public int Calculate(in BlessingInput input) => 25;
            public BlessingBreakdown Explain(in BlessingInput input) => new BlessingBreakdown(25, 0, 0, 25, BlessingZeroReason.None);
        }

        private EventBus _bus;
        private FakeClock _clock;
        private TicketRegistry _registry;
        private List<IGameEvent> _log;
        private readonly CarId _car = new CarId(1);

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus(); _clock = new FakeClock();
            var cars = new FakeCarFactory(_bus);
            _registry = new TicketRegistry(_bus, _clock, new FakeBlessings());
            new PatronDeparture(_bus, cars);
            _log = new List<IGameEvent>();
            _bus.Subscribe<CarDelivered>(e => _log.Add(e));
            _bus.Subscribe<TicketClosed>(e => _log.Add(e));
            cars.Spawn(_car, UnityEngine.Vector2.zero, 0f);
            _registry.Open(_car, new PatronRequest(20f, 20, false, 60f));
        }

        private void Tick(float minute)
        {
            _clock.ElapsedMinutes = minute;
            _bus.Publish(new ShiftTick(minute));
        }

        private void AssertClosedWithoutDelivery()
        {
            Assert.AreEqual(1, _log.Count);
            Assert.IsInstanceOf<TicketClosed>(_log[0]);
        }

        [Test]
        public void DeliveredClose_PublishesCarDeliveredThenTicketClosed()
        {
            _bus.Publish(new CarParked(_car, 1));
            Tick(20f);
            _bus.Publish(new CarAtCurb(_car));

            Assert.AreEqual(2, _log.Count);
            Assert.IsInstanceOf<CarDelivered>(_log[0]);
            Assert.AreEqual(_car, ((CarDelivered)_log[0]).Car);
            Assert.IsInstanceOf<TicketClosed>(_log[1]);
        }

        [Test]
        public void LatePickupClose_PublishesCarDeliveredThenTicketClosed()
        {
            _bus.Publish(new CarParked(_car, 1));
            Tick(20f);
            Tick(90f);
            _bus.Publish(new CarAtCurb(_car));

            Assert.AreEqual(2, _log.Count);
            Assert.IsInstanceOf<CarDelivered>(_log[0]);
            Assert.IsInstanceOf<TicketClosed>(_log[1]);
        }

        [Test]
        public void NightEndBeforeParking_ClosesWithoutCarDelivered()
        {
            _bus.Publish(new ShiftEnded());
            AssertClosedWithoutDelivery();
        }

        [Test]
        public void NightEndWhileParkedOrRequested_ClosesWithoutCarDelivered()
        {
            _bus.Publish(new CarParked(_car, 1));
            Tick(20f);
            _bus.Publish(new ShiftEnded());
            AssertClosedWithoutDelivery();
        }
    }
}
