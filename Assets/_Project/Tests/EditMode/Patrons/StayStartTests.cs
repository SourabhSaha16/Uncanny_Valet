using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    /// <summary>The stay clock starts only when the patron walks into the diner (issue 126; replaces the queue-distance and fallback rules of issue 78).</summary>
    public class StayStartTests
    {
        private const float Stay = 30f;

        private sealed class FakeClock : IGameClock
        {
            public float SecondsRemaining => 0f;
            public float ShiftLengthSeconds => 0f;
            public float ElapsedMinutes { get; set; }
            public bool IsRunning => true;
        }

        private sealed class FakeBlessings : IBlessingCalculator
        {
            public int Calculate(in BlessingInput input) => 10;
            public BlessingBreakdown Explain(in BlessingInput input) => new BlessingBreakdown(10, 0, 0, 10, BlessingZeroReason.None);
        }

        private EventBus _bus;
        private FakeClock _clock;
        private TicketRegistry _registry;
        private readonly System.Collections.Generic.List<StayStarted> _started = new System.Collections.Generic.List<StayStarted>();

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus(); _clock = new FakeClock();
            _registry = new TicketRegistry(_bus, _clock, new FakeBlessings());
            _started.Clear();
            _bus.Subscribe<StayStarted>(e => _started.Add(e));
        }

        private Ticket Arrive(int car, float minute)
        {
            _clock.ElapsedMinutes = minute;
            var t = _registry.Open(new CarId(car), new PatronRequest(Stay, 20, false));
            _bus.Publish(new CarArrived(t.CarId));
            return t;
        }

        private void Tick(float minute)
        {
            _clock.ElapsedMinutes = minute;
            _bus.Publish(new ShiftTick(minute));
        }

        [Test]
        public void StayStartsWhenThePatronEntersTheDiner_NotBefore()
        {
            var t = Arrive(1, 1f);
            Tick(10f);
            Assert.IsFalse(t.StayStarted);
            _clock.ElapsedMinutes = 12f;
            _bus.Publish(new PatronEnteredDiner(t.CarId));
            Assert.IsTrue(t.StayStarted);
            Assert.AreEqual(12f + Stay, t.ReturnMinute);
            Assert.AreEqual(1, _started.Count);
            Assert.AreEqual(12f, _started[0].Minute);
        }

        [Test]
        public void ACarThatStaysInTheQueue_NeverStartsItsStay_AndNeverFallsBackToATimer()
        {
            var requested = 0;
            _bus.Subscribe<PickupRequested>(e => requested++);
            var t = Arrive(1, 0f);
            for (var m = 0.5f; m < 500f; m += 0.5f) Tick(m);
            Assert.IsFalse(t.StayStarted);
            Assert.AreEqual(0, _started.Count);
            Assert.AreEqual(0, requested);
        }

        [Test]
        public void ParkingTheCar_DoesNotStartTheStay_BeforeThePatronIsInside()
        {
            var t = Arrive(1, 0f);
            _clock.ElapsedMinutes = 1f;
            _bus.Publish(new CarParked(t.CarId, 3));
            Assert.IsFalse(t.StayStarted, "the car was handed over but the patron has not reached the diner");
            _clock.ElapsedMinutes = 4f;
            _bus.Publish(new PatronEnteredDiner(t.CarId));
            Assert.AreEqual(4f + Stay, t.ReturnMinute);
        }

        [Test]
        public void EnteringTwice_StartsTheStayOnlyOnce()
        {
            var t = Arrive(1, 1f);
            _clock.ElapsedMinutes = 2f;
            _bus.Publish(new PatronEnteredDiner(t.CarId));
            _clock.ElapsedMinutes = 4f;
            _bus.Publish(new PatronEnteredDiner(t.CarId));
            Assert.AreEqual(2f + Stay, t.ReturnMinute);
            Assert.AreEqual(1, _started.Count);
        }

        [Test]
        public void ADinerEntryForAnUnknownCar_IsIgnored()
        {
            _bus.Publish(new PatronEnteredDiner(new CarId(99)));
            Assert.AreEqual(0, _started.Count);
        }

        [Test]
        public void ReturnShiftIsRefusedUntilTheStayHasStarted()
        {
            var t = Arrive(1, 0f);
            var shifted = 0;
            _bus.Subscribe<PatronReturnShifted>(e => shifted++);
            _bus.Publish(new ReturnShiftRequested(t.CarId, -5f));
            Assert.AreEqual(0, shifted);
            _bus.Publish(new PatronEnteredDiner(t.CarId));
            _bus.Publish(new ReturnShiftRequested(t.CarId, -5f));
            Assert.AreEqual(1, shifted);
        }

        [Test]
        public void ATicketWhoseStayHasNotStarted_NeverEnds()
        {
            var t = new Ticket(new TicketId(1), new CarId(1), new PatronRequest(Stay, 20, false), 0, "KT 1");
            Assert.IsFalse(t.StayEnded(1000f));
            Assert.IsTrue(float.IsPositiveInfinity(t.ReturnMinute));
        }
    }
}
