using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Tests.Orders
{
    public class OrderIssueTests : OrderFlowFixture
    {
        private static readonly CarId CarA = new CarId(1);
        private static readonly CarId CarB = new CarId(2);

        [Test]
        public void Night1_HasNoOrders()
        {
            StartNight(1);
            Show(CarA, 'A'); Park(CarA, 1);
            for (var m = 1; m <= 60; m++) Tick(m);
            Assert.AreEqual(0, Issued.Count);
        }

        [Test]
        public void Night2_IssuesAfterGap_ToFreeRegularSpot()
        {
            Rng.Picks(0, 1);
            StartNight(2);
            Show(CarA, 'A'); Park(CarA, 1);
            Tick(9);
            Assert.AreEqual(0, Issued.Count, "gap is 10 minutes");
            Tick(10);
            Assert.AreEqual(1, Issued.Count);
            Assert.AreEqual(CarA, Issued[0].Car);
            Assert.AreEqual('A', Issued[0].Letter);
            Assert.AreEqual(3, Issued[0].TargetSpot, "spots 2 and 3 are free; charging spot 9 is never offered");
            Assert.AreEqual(1, Issued[0].Order.Value);
        }

        [Test]
        public void ChargingSpotIsNeverTheTarget()
        {
            Lot.Spots = new[] { 1, 9 };
            StartNight(2);
            Show(CarA, 'A'); Park(CarA, 1);
            Tick(10);
            Assert.AreEqual(0, Issued.Count, "only a charging spot is free: skip");
        }

        [Test]
        public void NoFreeSpot_SkipsAndRetriesNextGap()
        {
            StartNight(2);
            Show(CarA, 'A'); Park(CarA, 1);
            Park(CarB, 2); Park(CarB, 3);
            Tick(10);
            Assert.AreEqual(0, Issued.Count);
            Lot.Taken.Remove(3);
            Tick(19);
            Assert.AreEqual(0, Issued.Count, "next attempt is 10 minutes after the skipped one");
            Tick(20);
            Assert.AreEqual(1, Issued.Count);
            Assert.AreEqual(3, Issued[0].TargetSpot);
        }

        [Test]
        public void CarNotParked_OrWithPickupRequest_IsNotChosen()
        {
            StartNight(2);
            Show(CarA, 'A');
            Show(CarB, 'B'); Park(CarB, 1);
            Bus.Publish(new PickupRequested(new TicketId(1), CarB));
            Tick(10);
            Assert.AreEqual(0, Issued.Count);
        }

        [Test]
        public void MaxOpenOrders_StopsFurtherOrders_UntilOneEnds()
        {
            StartNight(2);
            Show(CarA, 'A'); Park(CarA, 1);
            Show(CarB, 'B'); Park(CarB, 2);
            Show(new CarId(3), 'C'); Park(new CarId(3), 3);
            Lot.Spots = new[] { 1, 2, 3, 4, 5, 6, 9 };
            Tick(10); Tick(20); Tick(30);
            Assert.AreEqual(2, Issued.Count, "MaxOpenOrders is 2");
            CloseWithoutDelivery(Issued[0].Car);
            Tick(40);
            Assert.AreEqual(3, Issued.Count);
        }

        [Test]
        public void TwoOrders_NeverShareATargetOrACar()
        {
            Lot.Spots = new[] { 1, 2, 3, 4 };
            StartNight(2);
            Show(CarA, 'A'); Park(CarA, 1);
            Show(CarB, 'B'); Park(CarB, 2);
            Tick(10); Tick(20); Tick(30);
            Assert.AreEqual(2, Issued.Count);
            Assert.AreNotEqual(Issued[0].Car, Issued[1].Car);
            Assert.AreNotEqual(Issued[0].TargetSpot, Issued[1].TargetSpot);
        }
    }
}
