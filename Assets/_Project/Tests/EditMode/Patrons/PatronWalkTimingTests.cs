using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;
using UncannyValet.Tests.Integration;

namespace UncannyValet.Tests.Patrons
{
    public class PatronWalkTimingTests
    {
        private const float Stay = 20f;
        private const float WalkTimeout = 30f;

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
        private FakeCarFactory _cars;
        private TicketRegistry _registry;
        private PatronDeparture _departure;
        private List<IGameEvent> _log;
        private readonly CarId _car = new CarId(1);

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _clock = new FakeClock();
            _cars = new FakeCarFactory(_bus);
            _registry = new TicketRegistry(_bus, _clock, new FakeBlessings()) { CurbWalkTimeoutMinutes = WalkTimeout };
            _departure = new PatronDeparture(_bus, _cars, _clock, 5f);
            _log = new List<IGameEvent>();
            _bus.Subscribe<PatronSummoned>(e => _log.Add(e));
            _bus.Subscribe<PickupRequested>(e => _log.Add(e));
            _bus.Subscribe<CarDeparted>(e => _log.Add(e));
            _cars.Spawn(_car, UnityEngine.Vector2.zero, 0f);
            _registry.Open(_car, new PatronRequest(Stay, 20, false, 60f));
            _bus.Publish(new CarParked(_car, 1));
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
        public void StayEndSummonsThePatronInsteadOfRequestingThePickup()
        {
            Tick(Stay + 1f);
            Assert.AreEqual(1, Count<PatronSummoned>());
            Assert.AreEqual(0, Count<PickupRequested>());
            Tick(Stay + 2f);
            Assert.AreEqual(1, Count<PatronSummoned>());
        }

        [Test]
        public void PickupIsRequestedWhenThePatronReachesTheCurb()
        {
            Tick(Stay + 1f);
            _clock.ElapsedMinutes = Stay + 9f;
            _bus.Publish(new PatronReachedCurb(_car));
            Assert.AreEqual(1, Count<PickupRequested>());
        }

        [Test]
        public void ReachingTheCurbBeforeTheSummonDoesNothing()
        {
            _bus.Publish(new PatronReachedCurb(_car));
            Tick(Stay - 5f);
            Assert.AreEqual(0, Count<PickupRequested>());
        }

        [Test]
        public void PickupFallsBackToATimeoutWhenNobodyWalks()
        {
            Tick(Stay + 1f);
            Tick(Stay + WalkTimeout - 1f);
            Assert.AreEqual(0, Count<PickupRequested>());
            Tick(Stay + 1f + WalkTimeout);
            Assert.AreEqual(1, Count<PickupRequested>());
        }

        [Test]
        public void ZeroTimeoutKeepsTheOldInstantPickup()
        {
            _registry.CurbWalkTimeoutMinutes = 0f;
            Tick(Stay + 1f);
            Assert.AreEqual(1, Count<PickupRequested>());
            Assert.AreEqual(0, Count<PatronSummoned>());
        }

        [Test]
        public void DeliveredCarWaitsForThePatronToBoard()
        {
            Tick(Stay + 1f);
            _bus.Publish(new PatronReachedCurb(_car));
            _bus.Publish(new CarAtCurb(_car));
            Assert.AreEqual(0, Count<CarDeparted>());
            Assert.AreEqual(1, _cars.Alive);
            _bus.Publish(new PatronBoarded(_car));
            Assert.AreEqual(1, Count<CarDeparted>());
            Assert.AreEqual(0, _cars.Alive);
        }

        [Test]
        public void DeliveredCarLeavesAfterTheBoardingTimeoutWithoutAPatron()
        {
            Tick(Stay + 1f);
            _bus.Publish(new PatronReachedCurb(_car));
            _bus.Publish(new CarAtCurb(_car));
            Tick(Stay + 4f);
            Assert.AreEqual(0, Count<CarDeparted>());
            Tick(Stay + 7f);
            Assert.AreEqual(1, Count<CarDeparted>());
        }

        [Test]
        public void ShiftEndReleasesACarThatIsStillBoarding()
        {
            Tick(Stay + 1f);
            _bus.Publish(new PatronReachedCurb(_car));
            _bus.Publish(new CarAtCurb(_car));
            _bus.Publish(new ShiftEnded());
            Assert.AreEqual(1, Count<CarDeparted>());
        }

        [Test]
        public void DisabledBoardingSendsTheCarOffAtOnce()
        {
            _departure.Dispose();
            new PatronDeparture(_bus, _cars, _clock, 0f);
            Tick(Stay + 1f);
            _bus.Publish(new PatronReachedCurb(_car));
            _bus.Publish(new CarAtCurb(_car));
            Assert.AreEqual(1, Count<CarDeparted>());
        }
    }
}
