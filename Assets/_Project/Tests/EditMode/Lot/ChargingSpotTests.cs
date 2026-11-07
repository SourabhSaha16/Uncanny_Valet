using NUnit.Framework;
using UncannyValet.Lot;

namespace UncannyValet.Tests.Lot
{
    public class ChargingSpotTests
    {
        private static SpotOccupancy Make() => new SpotOccupancy(new[] { 1, 2, 3, 4, 5 }, 4);

        [Test]
        public void RegularIds_AreNotCharging()
        {
            var occ = Make();
            Assert.IsFalse(occ.IsCharging(1));
            Assert.IsFalse(occ.IsCharging(3));
        }

        [Test]
        public void ChargingIds_AreCharging()
        {
            var occ = Make();
            Assert.IsTrue(occ.IsCharging(4));
            Assert.IsTrue(occ.IsCharging(5));
        }

        [Test]
        public void UnknownIds_AreNotCharging()
        {
            var occ = Make();
            Assert.IsFalse(occ.IsCharging(99));
            Assert.IsFalse(occ.IsCharging(0));
        }
    }
}
