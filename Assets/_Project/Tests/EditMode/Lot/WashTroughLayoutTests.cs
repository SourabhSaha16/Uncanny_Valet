using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>The parametric builder's wash troughs and lot extension (Vanaheim): where the troughs stand, how far from the bays, and how the lot widens.</summary>
    public sealed class WashTroughLayoutTests
    {
        private const float Extension = 13.5f;
        private const float Distance = 14f;
        private static readonly LotLayoutOptions Beside = new LotLayoutOptions(true, 0f, 1);
        private static readonly LotLayoutOptions Far = new LotLayoutOptions(true, 0f, 3, Extension, Distance);

        private LotConfig _config;

        [SetUp] public void SetUp() => _config = ScriptableObject.CreateInstance<LotConfig>();

        [TearDown] public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void TheDefaultOptions_HaveNoTroughs_AndTheWashSlotIsTheOnlyStation()
        {
            var g = new LotGeometry(_config);
            Assert.IsEmpty(g.Troughs);
            Assert.AreEqual(1, g.WashStations.Length);
            Assert.AreEqual(g.Wash, g.WashStations[0]);
        }

        [Test]
        public void ClosedServices_WithoutTroughs_HaveNoStations()
        {
            var g = new LotGeometry(_config, 3, new LotLayoutOptions(true, 0f));
            Assert.IsEmpty(g.WashStations);
        }

        [Test]
        public void OneTrough_StandsBesideTheBayBlock_BetweenTheBaysAndTheStrip_InTheTopRow()
        {
            var g = new LotGeometry(_config, 3, Beside);
            Assert.AreEqual(1, g.Troughs.Length);
            Assert.AreEqual(8, g.Spots.Length, "the trough is not one of the eight bays");
            var trough = g.Troughs[0];
            Assert.AreEqual(_config.SpotSize, trough.size, "as big as a bay");
            Assert.Greater(trough.xMin, g.Spots[3].xMax, "east of the bay block");
            Assert.Less(trough.xMax, g.Strip.xMin, "west of the right strip");
            Assert.AreEqual(g.Spots[3].y, trough.y, 0.001f, "in the top row");
            foreach (var spot in g.Spots) Assert.IsFalse(trough.Overlaps(spot));
        }

        [Test]
        public void WithoutAnExtension_TheLotKeepsItsWidth()
        {
            var plain = new LotGeometry(_config, 3, new LotLayoutOptions(true, 0f));
            var beside = new LotGeometry(_config, 3, Beside);
            Assert.AreEqual(plain.Bounds, beside.Bounds);
            Assert.AreEqual(plain.Strip, beside.Strip);
        }

        [Test]
        public void TheExtension_WidensTheLotOnTheFarSide_AndMovesTheStripOut()
        {
            var plain = new LotGeometry(_config, 4, new LotLayoutOptions(true, 0f));
            var wide = new LotGeometry(_config, 4, Far);
            Assert.AreEqual(plain.Bounds.xMax + Extension, wide.Bounds.xMax, 0.001f);
            Assert.AreEqual(plain.Strip.xMin + Extension, wide.Strip.xMin, 0.001f);
            Assert.AreEqual(plain.Bounds.xMin, wide.Bounds.xMin, 0.001f, "the near side does not move");
            Assert.AreEqual(plain.Spots[3], wide.Spots[3], "the bays stay where they are");
            Assert.AreEqual(plain.Curb, wide.Curb);
            Assert.AreEqual(plain.AisleY, wide.AisleY, 0.001f);
            Assert.AreEqual(plain.RoadSpan.y + Extension, wide.RoadSpan.y, 0.001f, "the road reaches the new edge");
        }

        [Test]
        public void ThreeTroughs_StandAtTheFarEnd_AtLeastFourteenUnitsFromEveryBay()
        {
            var g = new LotGeometry(_config, 4, Far);
            Assert.AreEqual(3, g.Troughs.Length);
            foreach (var trough in g.Troughs)
                foreach (var spot in g.Spots) Assert.GreaterOrEqual(trough.xMin - spot.xMax, Distance - 0.001f, "gap to bay " + spot);
            Assert.AreEqual(Distance, g.Troughs[0].xMin - g.Spots[3].xMax, 0.001f, "exactly the asked distance from the nearest bay");
        }

        [Test]
        public void TheThreeTroughs_AreInsideTheLot_ClearOfEachOther_TheStrip_TheAisleAndTheQueue()
        {
            var g = new LotGeometry(_config, 4, Far);
            var aisle = Rect.MinMaxRect(g.Spots[0].xMin, g.AisleY - _config.AisleWidth * 0.5f, g.Strip.xMin, g.AisleY + _config.AisleWidth * 0.5f);
            for (var i = 0; i < g.Troughs.Length; i++)
            {
                var t = g.Troughs[i];
                Assert.IsTrue(g.Bounds.Contains(t.min) && g.Bounds.Contains(t.max), "trough " + i + " inside the frame");
                Assert.Less(t.xMax, g.Strip.xMin, "trough " + i + " off the strip");
                Assert.IsFalse(t.Overlaps(aisle), "trough " + i + " is on the drive aisle");
                foreach (var car in g.QueueCars) Assert.IsFalse(t.Overlaps(car));
                for (var j = 0; j < i; j++) Assert.IsFalse(t.Overlaps(g.Troughs[j]), "troughs " + i + " and " + j);
            }
        }

        [Test]
        public void TheTroughs_RunTopToBottom_AllInOneColumn()
        {
            var g = new LotGeometry(_config, 4, Far);
            Assert.Greater(g.Troughs[0].y, g.Troughs[1].y);
            Assert.Greater(g.Troughs[1].y, g.Troughs[2].y);
            Assert.AreEqual(g.Troughs[0].x, g.Troughs[2].x, 0.001f);
        }

        [Test]
        public void ATroughBelowTheQueue_GrowsTheLotDownwards()
        {
            var shallow = new LotGeometry(_config, 1, Far);
            Assert.LessOrEqual(shallow.Bounds.yMin, shallow.Troughs[2].yMin - _config.Margin + 0.001f);
        }

        [Test]
        public void TheOptions_ClampTheirNumbers()
        {
            var o = new LotLayoutOptions(true, -1f, 9, -4f, -2f);
            Assert.AreEqual(LotLayoutOptions.MaxWashTroughs, o.WashTroughs);
            Assert.AreEqual(0f, o.ExtensionWidth);
            Assert.AreEqual(0f, o.TroughDistance);
            Assert.AreEqual(0f, o.CornerClearance);
        }
    }
}
