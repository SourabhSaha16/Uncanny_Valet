using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Tests.Orders
{
    public class OrderResolveTests : OrderFlowFixture
    {
        private static readonly CarId CarA = new CarId(1);
        private int _target;

        // Night 2, car A parked in spot 1, order issued at minute 10 to the first free spot.
        private void IssueOrderForCarA()
        {
            StartNight(2);
            Show(CarA, 'A'); Park(CarA, 1);
            Tick(10);
            Assert.AreEqual(1, Issued.Count, "setup: one order issued");
            _target = Issued[0].TargetSpot;
        }

        [Test]
        public void DeliveredAfterVisitingTarget_Passes_NoStrike()
        {
            IssueOrderForCarA();
            Leave(CarA, 1);
            Tick(12);
            Park(CarA, _target);
            Tick(15);
            Leave(CarA, _target);
            Park(CarA, 1);
            Deliver(CarA);
            Assert.AreEqual(1, Resolved.Count);
            Assert.AreEqual(OrderOutcome.Passed, Resolved[0].Outcome);
            Assert.AreEqual(Issued[0].Order, Resolved[0].Order);
            Assert.AreEqual(0, Struck.Count);
        }

        [Test]
        public void DeliveredWithoutVisit_Fails_AndGivesStrikeAfterResolved()
        {
            IssueOrderForCarA();
            Deliver(CarA);
            Assert.AreEqual(OrderOutcome.Failed, Resolved[0].Outcome);
            Assert.AreEqual(1, Struck.Count);
            Assert.AreEqual(1, Struck[0].Total);
            Assert.AreEqual(3, Struck[0].Max);
            Assert.IsInstanceOf<OrderResolved>(Sequence[1]);
            Assert.IsInstanceOf<StrikeGiven>(Sequence[2]);
        }

        [Test]
        public void VisitBeforeTheOrderDoesNotCount()
        {
            StartNight(2);
            Show(CarA, 'A');
            Park(CarA, 2);
            Tick(5);
            Leave(CarA, 2);
            Park(CarA, 1);
            Tick(10);
            Assert.AreEqual(2, Issued[0].TargetSpot, "spot 2 is now free again");
            Deliver(CarA);
            Assert.AreEqual(OrderOutcome.Failed, Resolved[0].Outcome);
        }

        [Test]
        public void TicketClosedWithoutDelivery_Cancels_NoStrike()
        {
            IssueOrderForCarA();
            CloseWithoutDelivery(CarA);
            Assert.AreEqual(OrderOutcome.Cancelled, Resolved[0].Outcome);
            Assert.AreEqual(0, Struck.Count);
            Deliver(CarA);
            Assert.AreEqual(1, Resolved.Count, "an order is resolved once");
        }

        [Test]
        public void NightEnd_CancelsEveryOpenOrder()
        {
            IssueOrderForCarA();
            EndNight();
            Assert.AreEqual(1, Resolved.Count);
            Assert.AreEqual(OrderOutcome.Cancelled, Resolved[0].Outcome);
            Assert.AreEqual(0, Struck.Count);
        }

        [Test]
        public void CarWithoutOrder_DeliveredQuietly()
        {
            StartNight(2);
            Show(CarA, 'A'); Park(CarA, 1);
            Deliver(CarA);
            Assert.AreEqual(0, Resolved.Count);
        }

        [Test]
        public void HistoryIsClearedWhenTicketCloses_SoALaterCarWithSameIdStartsFresh()
        {
            IssueOrderForCarA();
            Leave(CarA, 1);
            Park(CarA, _target);
            Deliver(CarA);
            Show(CarA, 'A'); Park(CarA, 1);
            Tick(20);
            Assert.AreEqual(2, Issued.Count);
            Deliver(CarA);
            Assert.AreEqual(OrderOutcome.Failed, Resolved[1].Outcome);
        }

        [Test]
        public void NewOrderAfterNightEnd_UsesNextId()
        {
            IssueOrderForCarA();
            EndNight();
            StartNight(3);
            Show(CarA, 'A'); Park(CarA, 1);
            Tick(10);
            Assert.AreEqual(2, Issued[1].Order.Value);
        }

        [Test]
        public void StrikesCarryAcrossNights_ThroughTheFullFlow()
        {
            IssueOrderForCarA();
            Deliver(CarA);
            EndNight();
            StartNight(3);
            Assert.AreEqual(1, Strikes.Total);
            StartNight(1);
            Assert.AreEqual(0, Strikes.Total);
        }
    }
}
