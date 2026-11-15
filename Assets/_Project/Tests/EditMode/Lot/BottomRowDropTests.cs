using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Lot;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>The bottom bay row sits lower by LotConfig.BottomRowDrop (issue #131).</summary>
    public class BottomRowDropTests
    {
        private LotGeometry _geo;
        private LotConfig _config;
        private readonly List<LotConfig> _extra = new List<LotConfig>();

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<LotConfig>();
            _geo = new LotGeometry(_config);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
            foreach (var config in _extra) Object.DestroyImmediate(config);
            _extra.Clear();
        }

        [Test]
        public void TheBottomRowDropDefaultsToAboutOneAndAHalfUnits()
        {
            Assert.AreEqual(1.5f, _config.BottomRowDrop, 0.001f);
        }

        [Test]
        public void TheBottomRowSitsLowerByTheDrop_AndTheTopRowStaysPut()
        {
            var tuned = new LotGeometry(ConfigWithDrop(0f));
            Assert.AreEqual(tuned.Spots[0], _geo.Spots[0], "the top row does not move");
            Assert.AreEqual(tuned.Spots[4].y - _config.BottomRowDrop, _geo.Spots[4].y, 1e-4f, "bottom regular bay");
            Assert.AreEqual(tuned.ChargingSpots[0].y - _config.BottomRowDrop, _geo.ChargingSpots[0].y, 1e-4f, "EV bay");
            Assert.AreEqual(tuned.Wash.y - _config.BottomRowDrop, _geo.Wash.y, 1e-4f, "wash slot");
        }

        [Test]
        public void TheCrosswalkStaysCentredInTheWiderAisle_ClearOfBothRows()
        {
            var top = _geo.Spots[0].yMin;
            var bottom = _geo.ChargingSpots[0].yMax;
            Assert.AreEqual((top + bottom) * 0.5f, _geo.AisleY, 1e-4f);
            const float half = 0.7f;
            Assert.Greater(_geo.AisleY - half, bottom, "crosswalk band clears the bottom row");
            Assert.Less(_geo.AisleY + half, top, "crosswalk band clears the top row");
        }

        [Test]
        public void TheFrameStillHoldsTheLoweredRow_AndTheDropGrowsTheFrameWhenTheRowSetsItsFloor()
        {
            var low = _geo.ChargingSpots[0].yMin;
            Assert.LessOrEqual(_geo.Bounds.yMin, low - _config.Margin + 1e-4f, "bounds hold the row plus margin");
            var plain = new LotGeometry(ConfigWithDrop(0f));
            Assert.LessOrEqual(_geo.Bounds.yMin, plain.Bounds.yMin + 1e-4f, "the frame never shrinks");
            Assert.AreEqual(plain.Bounds.yMax, _geo.Bounds.yMax, 1e-4f, "the top does not move");
        }

        private LotConfig ConfigWithDrop(float drop)
        {
            var config = ScriptableObject.CreateInstance<LotConfig>();
            var so = new UnityEditor.SerializedObject(config);
            so.FindProperty("_bottomRowDrop").floatValue = drop;
            so.ApplyModifiedPropertiesWithoutUndo();
            _extra.Add(config);
            return config;
        }
    }
}
