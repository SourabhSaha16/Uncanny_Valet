using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    /// <summary>The stay clock starts at the first rest clear of the queue, a bay park, or the queue wait limit (issue 78).</summary>
    public class StayStartTests
    {
        private const float Stay = 30f;
        private static readonly float Wait = ServiceTuning.DefaultQueueWaitMinutes;

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
        public void ACarLeftInTheQueue_IsNotAskedForBeforeTheLimitPlusTheStay()
        {
            var requested = 0;
            _bus.Subscribe<PickupRequested>(e => requested++);
            var t = Arrive(1, 0f);
            for (var m = 0.5f; m < Wait + Stay; m += 0.5f) Tick(m);
            Assert.AreEqual(0, requested, "handing it back fast is no longer possible");
            Tick(Wait + Stay);
            Assert.AreEqual(TicketState.PickupRequested, t.State);
        }

        [Test]
        public void StayStartsAtTheFirstRestClearOfTheQueue_AndMovingBackDoesNotRestartIt()
        {
            var t = Arrive(1, 1f);
            _clock.ElapsedMinutes = 2f;
            _bus.Publish(new CarClearedQueue(t.CarId));
            _clock.ElapsedMinutes = 4f;
            _bus.Publish(new CarClearedQueue(t.CarId));
            Assert.AreEqual(2f + Stay, t.ReturnMinute);
            Assert.AreEqual(1, _started.Count);
            Assert.AreEqual(2f, _started[0].Minute);
        }

        [Test]
        public void QueueWaitLimit_StartsTheStayAtArrivalPlusTheLimit_AndOnlyOnce()
        {
            var t = Arrive(1, 5f);
            Tick(5f + Wait - 0.1f);
            Assert.IsFalse(t.StayStarted);
            Tick(5f + Wait);
            Assert.IsTrue(t.StayStarted);
            Assert.AreEqual(5f + Wait + Stay, t.ReturnMinute);
            Tick(5f + Wait + 1f);
            Assert.AreEqual(1, _started.Count);
        }

        [Test]
        public void ParkingInABay_StartsTheStayEvenWithoutAClearReport()
        {
            var t = Arrive(1, 0f);
            _clock.ElapsedMinutes = 1f;
            _bus.Publish(new CarParked(t.CarId, 3));
            Assert.AreEqual(1f + Stay, t.ReturnMinute);
        }

        [Test]
        public void ReturnShiftIsRefusedUntilTheStayHasStarted()
        {
            var t = Arrive(1, 0f);
            var shifted = 0;
            _bus.Subscribe<PatronReturnShifted>(e => shifted++);
            _bus.Publish(new ReturnShiftRequested(t.CarId, -5f));
            Assert.AreEqual(0, shifted);
            _bus.Publish(new CarClearedQueue(t.CarId));
            _bus.Publish(new ReturnShiftRequested(t.CarId, -5f));
            Assert.AreEqual(1, shifted);
        }

        [Test]
        public void ATicketWhoseStayHasNotStarted_NeverEnds()
        {
            var t = new Ticket(new TicketId(1), new CarId(1), new PatronRequest(Stay, 20, false), 0, "KT 1");
            Assert.IsFalse(t.StayEnded(1000f));
            Assert.IsFalse(t.QueueWaitOver(1000f, Wait), "never arrived, so no queue wait");
        }
    }
}
