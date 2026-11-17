using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>The parametric builder's closed-services layout (Midgard): eight regular bays, no wash slot, no EV bay, and the corner clearance that grows the lot.</summary>
    public sealed class ServicesClosedLayoutTests
    {
        private static readonly LotLayoutOptions Closed = new LotLayoutOptions(true, 0f);

        private LotConfig _config;

        [SetUp] public void SetUp() => _config = ScriptableObject.CreateInstance<LotConfig>();

        [TearDown] public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void TheDefaultOptions_KeepTodaysDiner()
        {
            var g = new LotGeometry(_config);
            Assert.AreEqual(6, g.Spots.Length);
            Assert.AreEqual(1, g.ChargingSpots.Length);
            Assert.IsTrue(g.HasWash);
        }

        [Test]
        public void ClosedServices_GiveEightRegularBays_AndNoStations()
        {
            var g = new LotGeometry(_config, 3, Closed);
            Assert.AreEqual(8, g.Spots.Length);
            Assert.AreEqual(0, g.ChargingSpots.Length);
            Assert.IsFalse(g.HasWash);
            Assert.AreEqual(9, g.FirstChargingId, "no charging id exists below 9");
        }

        [Test]
        public void TheEightBays_RunInReadingOrder_TwoRowsOfFour_WithoutOverlap()
        {
            var g = new LotGeometry(_config, 3, Closed);
            for (var i = 1; i < 4; i++) Assert.Greater(g.Spots[i].x, g.Spots[i - 1].x, "top row runs left to right");
            for (var i = 0; i < 4; i++)
            {
                Assert.AreEqual(g.Spots[i].x, g.Spots[i + 4].x, 1e-4f, "the bottom row sits under the top row");
                Assert.Less(g.Spots[i + 4].y, g.Spots[i].y);
            }
            for (var i = 0; i < 8; i++)
            {
                Assert.IsTrue(Contains(g.Bounds, g.Spots[i]), "bay " + (i + 1) + " is inside the frame");
                for (var j = i + 1; j < 8; j++) Assert.IsFalse(g.Spots[i].Overlaps(g.Spots[j]), "bays " + (i + 1) + " and " + (j + 1) + " overlap");
            }
        }

        [Test]
        public void TheBays_StayClearOfTheAisleAndTheQueue()
        {
            var g = new LotGeometry(_config, 3, Closed);
            foreach (var spot in g.Spots)
            {
                Assert.IsFalse(spot.Contains(new Vector2(spot.x, g.AisleY)), "the aisle line crosses no bay");
                foreach (var car in g.QueueCars) Assert.IsFalse(spot.Overlaps(car));
            }
        }

        [Test]
        public void CornerClearance_GrowsTheLot_OnlyWhenTheQueueLeavesNoRoom()
        {
            var plain = new LotGeometry(_config, 3, Closed);
            var roomy = new LotGeometry(_config, 3, new LotLayoutOptions(true, 6f));
            var tall = new LotGeometry(_config, 4, Closed);
            var tallRoomy = new LotGeometry(_config, 4, new LotLayoutOptions(true, 6f));
            Assert.Less(roomy.Bounds.yMin, plain.Bounds.yMin, "three slots leave too little corner room: the lot grows");
            Assert.AreEqual(tall.Bounds.yMin, tallRoomy.Bounds.yMin, 1e-4f, "four slots already leave 6 units");
            Assert.GreaterOrEqual(roomy.Spots[7].yMin - roomy.Bounds.yMin, 6f, "the clearance is free under the bottom row");
        }

        private static bool Contains(Rect outer, Rect inner)
            => inner.xMin >= outer.xMin && inner.xMax <= outer.xMax && inner.yMin >= outer.yMin && inner.yMax <= outer.yMax;
    }
}
