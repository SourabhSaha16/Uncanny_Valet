using NUnit.Framework;
using UncannyValet.Core.Ids;
using UncannyValet.Lot;

namespace UncannyValet.Tests.Lot
{
    public class SpotOccupancyTests
    {
        private static SpotOccupancy Make() => new SpotOccupancy(new[] { 1, 2, 3 });

        [Test]
        public void NewSpots_AreFree_UnknownSpotsAreNot()
        {
            var occ = Make();
            Assert.IsTrue(occ.IsFree(1));
            Assert.IsFalse(occ.IsFree(99));
        }

        [Test]
        public void Park_MarksSpotTaken_AndRecordsCar()
        {
            var occ = Make();
            Assert.IsTrue(occ.Park(2, new CarId(5)));
            Assert.IsFalse(occ.IsFree(2));
            Assert.IsTrue(occ.TryGetCar(2, out var car));
            Assert.AreEqual(new CarId(5), car);
        }

        [Test]
        public void Park_RejectsSecondCarAndUnknownSpot()
        {
            var occ = Make();
            occ.Park(1, new CarId(5));
            Assert.IsFalse(occ.Park(1, new CarId(6)));
            Assert.IsFalse(occ.Park(99, new CarId(6)));
        }

        [Test]
        public void Leave_OnlyFreesForTheHoldingCar()
        {
            var occ = Make();
            occ.Park(1, new CarId(5));
            Assert.IsFalse(occ.Leave(1, new CarId(6)));
            Assert.IsFalse(occ.IsFree(1));
            Assert.IsTrue(occ.Leave(1, new CarId(5)));
            Assert.IsTrue(occ.IsFree(1));
        }
    }
}
