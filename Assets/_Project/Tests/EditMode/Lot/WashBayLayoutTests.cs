using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>The parametric builder's wash bays and lot extension (Vanaheim): where the wash bays stand, how far from the bays, and how the lot widens.</summary>
    public sealed class WashBayLayoutTests
    {
        private const float Extension = 13.5f;
        private const float Gap = 0.5f;
        private static readonly LotLayoutOptions Beside = new LotLayoutOptions(true, 0f, 1, 0f, Gap);
        private static readonly LotLayoutOptions Far = new LotLayoutOptions(true, 0f, 3, Extension, Gap);
        private static readonly LotLayoutOptions Next = new LotLayoutOptions(true, 0f, 3, 0f, Gap);

        private LotConfig _config;

        [SetUp] public void SetUp() => _config = ScriptableObject.CreateInstance<LotConfig>();

        [TearDown] public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void TheDefaultOptions_HaveNoWashBays_AndTheWashSlotIsTheOnlyStation()
        {
            var g = new LotGeometry(_config);
            Assert.IsEmpty(g.WashBays);
            Assert.AreEqual(1, g.WashStations.Length);
            Assert.AreEqual(g.Wash, g.WashStations[0]);
        }

        [Test]
        public void ClosedServices_WithoutWashBays_HaveNoStations()
        {
            var g = new LotGeometry(_config, 3, new LotLayoutOptions(true, 0f));
            Assert.IsEmpty(g.WashStations);
        }

        [Test]
        public void OneWashBay_StandsBesideTheBayBlock_BetweenTheBaysAndTheStrip_InTheTopRow()
        {
            var g = new LotGeometry(_config, 3, Beside);
            Assert.AreEqual(1, g.WashBays.Length);
            Assert.AreEqual(8, g.Spots.Length, "the wash bay is not one of the eight bays");
            var washBay = g.WashBays[0];
            Assert.AreEqual(_config.SpotSize, washBay.size, "as big as a bay");
            Assert.Greater(washBay.xMin, g.Spots[3].xMax, "east of the bay block");
            Assert.Less(washBay.xMax, g.Strip.xMin, "west of the right strip");
            Assert.AreEqual(g.Spots[3].y, washBay.y, 0.001f, "in the top row");
            foreach (var spot in g.Spots) Assert.IsFalse(washBay.Overlaps(spot));
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
        public void ThreeWashBays_StandRightNextToTheBayBlock_ExactlyTheGapAway()
        {
            var g = new LotGeometry(_config, 4, Far);
            Assert.AreEqual(3, g.WashBays.Length);
            foreach (var washBay in g.WashBays)
                Assert.AreEqual(Gap, washBay.xMin - g.Spots[3].xMax, 0.001f, "the gap to the bay block");
        }

        [Test]
        public void TheWashBayGap_IsOneKnob_AndMovesEveryWashBay()
        {
            var near = new LotGeometry(_config, 4, Next);
            var wider = new LotGeometry(_config, 4, new LotLayoutOptions(true, 0f, 3, 0f, Gap + 2f));
            for (var i = 0; i < 3; i++) Assert.AreEqual(near.WashBays[i].x + 2f, wider.WashBays[i].x, 0.001f);
        }

        [Test]
        public void TheTopWashBay_SharesARowWithTheTopBays_AndTheSecondWithTheBottomBays()
        {
            var g = new LotGeometry(_config, 4, Next);
            Assert.AreEqual(g.Spots[3].y, g.WashBays[0].y, 0.001f);
            Assert.AreEqual(g.Spots[7].y, g.WashBays[1].y, 0.001f);
        }

        [Test]
        public void TheThreeWashBays_AreInsideTheLot_ClearOfEachOther_TheStrip_TheAisleAndTheQueue()
        {
            var g = new LotGeometry(_config, 4, Next);
            var aisle = Rect.MinMaxRect(g.Spots[0].xMin, g.AisleY - _config.AisleWidth * 0.5f, g.Strip.xMin, g.AisleY + _config.AisleWidth * 0.5f);
            for (var i = 0; i < g.WashBays.Length; i++)
            {
                var t = g.WashBays[i];
                Assert.IsTrue(g.Bounds.Contains(t.min) && g.Bounds.Contains(t.max), "wash bay " + i + " inside the frame");
                Assert.Less(t.xMax, g.Strip.xMin, "wash bay " + i + " off the strip");
                Assert.IsFalse(t.Overlaps(aisle), "wash bay " + i + " is on the drive aisle");
                foreach (var car in g.QueueCars) Assert.IsFalse(t.Overlaps(car));
                for (var j = 0; j < i; j++) Assert.IsFalse(t.Overlaps(g.WashBays[j]), "wash bays " + i + " and " + j);
            }
        }

        [Test]
        public void TheWashBays_RunTopToBottom_AllInOneColumn()
        {
            var g = new LotGeometry(_config, 4, Far);
            Assert.Greater(g.WashBays[0].y, g.WashBays[1].y);
            Assert.Greater(g.WashBays[1].y, g.WashBays[2].y);
            Assert.AreEqual(g.WashBays[0].x, g.WashBays[2].x, 0.001f);
        }

        [Test]
        public void AWashBayBelowTheQueue_GrowsTheLotDownwards()
        {
            var shallow = new LotGeometry(_config, 1, Far);
            Assert.LessOrEqual(shallow.Bounds.yMin, shallow.WashBays[2].yMin - _config.Margin + 0.001f);
        }

        [Test]
        public void TheOptions_ClampTheirNumbers()
        {
            var o = new LotLayoutOptions(true, -1f, 9, -4f, -2f);
            Assert.AreEqual(LotLayoutOptions.MaxWashTroughs, o.WashTroughs);
            Assert.AreEqual(0f, o.ExtensionWidth);
            Assert.AreEqual(0f, o.WashBayGap);
            Assert.AreEqual(0f, o.CornerClearance);
        }
    }
}
