using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class LotGeometryTests
    {
        private LotGeometry _geo;
        private LotConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<LotConfig>();
            _geo = new LotGeometry(_config);
        }

        private Dictionary<string, Rect> AllRects()
        {
            var all = new Dictionary<string, Rect> { ["curb"] = _geo.Curb };
            for (var i = 0; i < _geo.Spots.Length; i++) all["spot" + i] = _geo.Spots[i];
            for (var i = 0; i < _geo.QueueCars.Length; i++) all["queue" + i] = _geo.QueueCars[i];
            for (var i = 0; i < _geo.ChargingSpots.Length; i++) all["charging" + i] = _geo.ChargingSpots[i];
            all["wash"] = _geo.Wash;
            return all;
        }

        [Test]
        public void SpotIds_RunColumnByColumnFromTheCurb()
        {
            var rows = _config.Rows;
            for (var i = 1; i < _geo.Spots.Length; i++)
            {
                var prev = _geo.Spots[i - 1];
                var cur = _geo.Spots[i];
                Assert.GreaterOrEqual(cur.x, prev.x - 0.001f, "x never decreases from one id to the next");
                if (i % rows != 0) Assert.AreEqual(prev.x, cur.x, 0.001f, "same column within a group of rows");
            }
        }

        [Test]
        public void NoTwoLotRectanglesOverlap()
        {
            var all = AllRects();
            var keys = new List<string>(all.Keys);
            for (var i = 0; i < keys.Count; i++)
            for (var j = i + 1; j < keys.Count; j++)
                Assert.IsFalse(all[keys[i]].Overlaps(all[keys[j]]), keys[i] + " overlaps " + keys[j]);
        }

        [Test]
        public void EveryRectangleIsInsideTheBounds()
        {
            foreach (var pair in AllRects())
            {
                Assert.GreaterOrEqual(pair.Value.xMin, _geo.Bounds.xMin, pair.Key);
                Assert.LessOrEqual(pair.Value.xMax, _geo.Bounds.xMax, pair.Key);
                Assert.GreaterOrEqual(pair.Value.yMin, _geo.Bounds.yMin, pair.Key);
                Assert.LessOrEqual(pair.Value.yMax, _geo.Bounds.yMax, pair.Key);
            }
        }

        [Test]
        public void ChargingSpots_FollowTheRegularSpotIds()
        {
            Assert.AreEqual(_config.ChargingBays, _geo.ChargingSpots.Length);
            Assert.AreEqual(_geo.Spots.Length + 1, _geo.FirstChargingId);
        }

        [Test]
        public void ChargingSpotsAndWash_LeaveAWalkwayToTheSpots()
        {
            foreach (var spot in _geo.ChargingSpots)
                Assert.GreaterOrEqual(_geo.Spots[0].xMin - spot.xMax, 2f);
            Assert.GreaterOrEqual(_geo.Spots[0].xMin - _geo.Wash.xMax, 2f);
        }

        [Test]
        public void ColumnsFor_RoundsUpAndClamps()
        {
            var rows = _config.Rows;
            Assert.AreEqual(1, _geo.ColumnsFor(0));
            Assert.AreEqual(1, _geo.ColumnsFor(rows));
            Assert.AreEqual(2, _geo.ColumnsFor(rows + 1));
            Assert.AreEqual(_config.SpotsPerRow, _geo.ColumnsFor(int.MaxValue / 2));
        }

        [Test]
        public void Frame_GrowsToTheRightOnlyWithMoreColumns()
        {
            var one = _geo.FrameFor(1, false, false);
            var four = _geo.FrameFor(4, false, false);
            Assert.AreEqual(one.xMin, four.xMin, 0.001f);
            Assert.AreEqual(one.yMin, four.yMin, 0.001f);
            Assert.AreEqual(one.yMax, four.yMax, 0.001f);
            Assert.AreEqual(3 * (_config.SpotSize.x + _config.SpotGap), four.xMax - one.xMax, 0.001f);
        }

        [Test]
        public void Frame_ContainsEveryOpenSpot_AndTheMaxFrameIsTheBounds()
        {
            var cols = _config.SpotsPerRow;
            Assert.AreEqual(_geo.Bounds, _geo.FrameFor(cols, true, true));
            var frame = _geo.FrameFor(2, true, false);
            var lastOpen = _geo.Spots[2 * _config.Rows - 1];
            Assert.LessOrEqual(lastOpen.xMax, frame.xMax);
            foreach (var spot in _geo.ChargingSpots) Assert.IsTrue(Contains(frame, spot));
        }

        [Test]
        public void EightSpotsAndChargingFrame_StaysInsideTheWiderLot()
        {
            var frame = _geo.FrameFor(_geo.ColumnsFor(8), true, true);
            Assert.LessOrEqual(frame.width, _geo.Bounds.width);
            Assert.Less(frame.width, 26f, "8 spots + 2 charging fit about 26 units wide");
        }

        private static bool Contains(Rect outer, Rect inner) =>
            inner.xMin >= outer.xMin && inner.xMax <= outer.xMax && inner.yMin >= outer.yMin && inner.yMax <= outer.yMax;

        [Test]
        public void QueueHasFourSideBySideSlotsOnOneLine()
        {
            Assert.AreEqual(4, _geo.QueueSlots.Length);
            for (var i = 1; i < _geo.QueueSlots.Length; i++)
                Assert.AreEqual(_geo.QueueSlots[0].y, _geo.QueueSlots[i].y, 0.001f);
        }

        [Test]
        public void SlotZero_IsTheFirstQueueCar_BelowTheCurb()
        {
            Assert.AreEqual(_geo.QueueSlots[0], _geo.QueueCars[0].center);
            Assert.Less(_geo.QueueCars[0].yMax, _geo.Curb.yMin);
        }
    }
}
