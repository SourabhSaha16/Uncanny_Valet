using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    /// <summary>A patron's stay runs from the drop-off and ends wherever the car is (issue 77).</summary>
    public class TicketReturnTests
    {
        private const float Stay = 30f;
        private const float WalkTimeout = 5f;

        private sealed class FakeClock : IGameClock
        {
            public float SecondsRemaining => 0f;
            public float ShiftLengthSeconds => 0f;
            public float ElapsedMinutes { get; set; }
            public bool IsRunning => true;
        }

        private sealed class FakeTips : ITipCalculator
        {
            public TipInput Last;
            public int Calculate(in TipInput input) { Last = input; return 10; }
            public TipBreakdown Explain(in TipInput input) { Last = input; return new TipBreakdown(10, 0, 0, 0, 10, TipZeroReason.None); }
        }

        private EventBus _bus;
        private FakeClock _clock;
        private FakeTips _tips;
        private TicketRegistry _registry;
        private int _summoned, _requested, _closed;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus(); _clock = new FakeClock(); _tips = new FakeTips();
            _registry = new TicketRegistry(_bus, _clock, _tips);
            _summoned = _requested = _closed = 0;
            _bus.Subscribe<PatronSummoned>(e => _summoned++);
            _bus.Subscribe<PickupRequested>(e => _requested++);
            _bus.Subscribe<TicketClosed>(e => _closed++);
        }

        private static Ticket NewTicket() =>
            new Ticket(new TicketId(1), new CarId(1), new PatronRequest(Stay, 20, false), 0, "KT 1", ServiceTuning.Default);

        private void Tick(float minute)
        {
            _clock.ElapsedMinutes = minute;
            _bus.Publish(new ShiftTick(minute));
        }

        [Test]
        public void StayEnds_ForQueuedAndParkedCars_Alike()
        {
            var queued = NewTicket();
            var parked = NewTicket();
            parked.MarkParked();
            Assert.IsFalse(queued.StayEnded(Stay - 1f));
            Assert.IsTrue(queued.StayEnded(Stay), "queued, never parked");
            Assert.IsTrue(parked.StayEnded(Stay), "parked, washing or charging (no state of its own)");
        }

        [Test]
        public void StayCountsFromTheDropOff_AndOnlyTheFirstOne()
        {
            var t = NewTicket();
            t.StartStay(0f);
            t.MarkDroppedOff(4f);
            t.MarkDroppedOff(9f);
            Assert.AreEqual(4f + Stay, t.ReturnMinute);
        }

        [Test]
        public void PickupCanBeRequestedFromQueueOrBay_ButNotTwice()
        {
            var queued = NewTicket();
            queued.MarkPickupRequested(31f);
            Assert.AreEqual(TicketState.PickupRequested, queued.State);
            Assert.IsFalse(queued.StayEnded(99f), "no second request");
            var parked = NewTicket();
            parked.MarkParked();
            parked.MarkPickupRequested(31f);
            Assert.AreEqual(TicketState.PickupRequested, parked.State);
        }

        [Test]
        public void CurbCall_AcceptsSummonedPatronsBeforePickup_InAnyOpenState()
        {
            var t = NewTicket();
            Assert.IsFalse(CurbCall.AcceptsArrival(t), "not summoned yet");
            t.MarkSummoned(30f);
            Assert.IsTrue(CurbCall.AcceptsArrival(t));
            t.MarkParked();
            Assert.IsTrue(CurbCall.AcceptsArrival(t));
            t.MarkPickupRequested(31f);
            Assert.IsFalse(CurbCall.AcceptsArrival(t));
        }

        [Test]
        public void CarThatNeverParked_IsSummonedAndAskedForAtArrivalPlusStay()
        {
            _registry.CurbWalkTimeoutMinutes = WalkTimeout;
            Tick(2f);
            var t = _registry.Open(new CarId(1), new PatronRequest(Stay, 20, false));
            Tick(3f);
            _bus.Publish(new CarArrived(t.CarId));
            Tick(3f + Stay - 1f);
            Assert.AreEqual(0, _summoned);
            Tick(3f + Stay);
            Assert.AreEqual(1, _summoned);
            Assert.AreEqual(0, _requested);
            _bus.Publish(new PatronReachedCurb(t.CarId));
            Assert.AreEqual(1, _requested);
            Assert.AreEqual(TicketState.PickupRequested, t.State);
        }

        [Test]
        public void SummonedPatronAsksAfterTheWalkTimeout_EvenIfNobodyWalks()
        {
            _registry.CurbWalkTimeoutMinutes = WalkTimeout;
            var t = _registry.Open(new CarId(1), new PatronRequest(Stay, 20, false));
            Tick(Stay);
            Tick(Stay + WalkTimeout);
            Assert.AreEqual(TicketState.PickupRequested, t.State);
        }

        [Test]
        public void DeliveryFromTheQueueAndFromAStation_ClosesTheTicket()
        {
            var queued = _registry.Open(new CarId(1), new PatronRequest(Stay, 20, false, 60f, true, false));
            var washing = _registry.Open(new CarId(2), new PatronRequest(Stay, 20, false, 60f, false, true));
            _bus.Publish(new CarEnteredStation(washing.CarId, StationKind.Wash));
            Tick(Stay);
            _bus.Publish(new CarAtCurb(queued.CarId));
            _bus.Publish(new CarAtCurb(washing.CarId));
            Assert.AreEqual(2, _closed);
            Assert.AreEqual(0, _registry.OpenCount);
            Assert.IsTrue(_tips.Last.UncleanedDirty, "an unfinished wash is reported to the tip rules, not hidden");
        }

        [Test]
        public void CarAtCurbBeforeTheStayEnds_IsIgnored_ThenClosesOnceAsked()
        {
            var t = _registry.Open(new CarId(1), new PatronRequest(Stay, 20, false));
            _bus.Publish(new CarAtCurb(t.CarId));
            Assert.AreEqual(0, _closed);
            Tick(Stay);
            Assert.AreEqual(1, _closed, "already at the curb when asked: delivered at once");
        }

        [Test]
        public void NoTicketIsStuck_EveryEndedStayGetsAPickupRequestWithinTheWalkTimeout()
        {
            _registry.CurbWalkTimeoutMinutes = WalkTimeout;
            var tickets = new Ticket[12];
            for (var i = 0; i < tickets.Length; i++)
            {
                Tick(i);
                tickets[i] = _registry.Open(new CarId(i + 1), new PatronRequest(20f + i, 20, false));
                if (i % 3 == 1) _bus.Publish(new CarParked(tickets[i].CarId, i));
                if (i % 3 == 2) _bus.Publish(new CarEnteredStation(tickets[i].CarId, StationKind.Charging));
            }
            for (var m = 12; m <= 120; m++)
            {
                Tick(m);
                foreach (var t in tickets)
                {
                    if (m < t.ReturnMinute + WalkTimeout + 1f) continue;
                    Assert.AreEqual(TicketState.PickupRequested, t.State, "ticket " + t.TicketId.Value + " stuck at minute " + m);
                }
            }
        }

        [Test]
        public void PickupAcceptance_IsGreenOnlyOnceThePatronAsked_WhereverTheCarIs()
        {
            var acceptance = new PickupAcceptance(_registry);
            var t = _registry.Open(new CarId(1), new PatronRequest(Stay, 20, false));
            Assert.IsFalse(acceptance.WillAccept(t.CarId));
            Tick(Stay);
            Assert.IsTrue(acceptance.WillAccept(t.CarId));
        }
    }
}
