using NUnit.Framework;
using UncannyValet.Core.Ids;
using UncannyValet.Lot;

namespace UncannyValet.Tests.Lot
{
    public class SpotClosingTests
    {
        private static SpotOccupancy Make() => new SpotOccupancy(new[] { 1, 2, 3, 4 });

        [Test]
        public void ZeroActiveSpots_OpensEverything()
        {
            Assert.AreEqual(int.MaxValue, OpenSpotRule.Limit(0));
            var occ = Make();
            occ.SetActiveSpots(0);
            Assert.IsTrue(occ.IsFree(4));
        }

        [Test]
        public void SpotsBeyondActiveSpots_AreClosedAndRefuseCars()
        {
            var occ = Make();
            occ.SetActiveSpots(2);
            Assert.IsTrue(occ.IsFree(2));
            Assert.IsFalse(occ.IsFree(3));
            Assert.IsFalse(occ.Park(3, new CarId(1)));
            Assert.IsFalse(occ.TryGetCar(3, out _));
        }

        [Test]
        public void LaterPlanWithMoreSpots_ReopensThem()
        {
            var occ = Make();
            occ.SetActiveSpots(2);
            occ.SetActiveSpots(4);
            Assert.IsTrue(occ.Park(4, new CarId(1)));
            occ.SetActiveSpots(0);
            Assert.IsTrue(occ.IsOpen(3));
        }

        [Test]
        public void CarParkedBeforeClosing_CanStillLeave()
        {
            var occ = Make();
            occ.Park(4, new CarId(1));
            occ.SetActiveSpots(2);
            Assert.IsTrue(occ.Leave(4, new CarId(1)));
            Assert.IsFalse(occ.IsFree(4));
        }
    }
}
