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

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

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
        public void TheGridHasNineRegularSpotsTwoChargingBaysAndTheWash()
        {
            Assert.AreEqual(9, _geo.Spots.Length);
            Assert.AreEqual(2, _geo.ChargingSpots.Length);
            Assert.AreEqual(_config.Rows * _config.SpotsPerRow, _geo.Spots.Length + _geo.ChargingSpots.Length + 1);
        }

        [Test]
        public void StationsFillTheFirstGridCells_ChargingBaysInTheCurbColumn_WashTopOfTheNext()
        {
            Assert.AreEqual(_geo.ChargingSpots[0].x, _geo.ChargingSpots[1].x, 0.001f, "both bays share the column nearest the curb");
            Assert.Greater(_geo.ChargingSpots[0].y, _geo.ChargingSpots[1].y, "top then bottom");
            Assert.Greater(_geo.Wash.x, _geo.ChargingSpots[0].x, "the wash is in the second column");
            Assert.AreEqual(_geo.ChargingSpots[0].y, _geo.Wash.y, 0.001f, "the top row");
            foreach (var spot in _geo.ChargingSpots) Assert.AreEqual(_config.SpotSize, spot.size);
            Assert.AreEqual(_config.SpotSize, _geo.Wash.size);
        }

        [Test]
        public void RegularSpotIds_RunColumnByColumnFromTheCurb_AfterTheStations()
        {
            var rows = _config.Rows;
            Assert.Greater(_geo.Spots[0].x, _geo.ChargingSpots[0].x, "spot 1 is not in the station column");
            Assert.AreEqual(_geo.Wash.x, _geo.Spots[0].x, 0.001f, "spot 1 is the free cell under the wash");
            Assert.Less(_geo.Spots[0].y, _geo.Wash.y);
            for (var i = 1; i < _geo.Spots.Length; i++)
                Assert.GreaterOrEqual(_geo.Spots[i].x, _geo.Spots[i - 1].x - 0.001f, "x never decreases from one id to the next");
            Assert.AreEqual(_geo.Spots[1].x, _geo.Spots[2].x, 0.001f);
            Assert.AreNotEqual(_geo.Spots[1].x, _geo.Spots[0].x);
            Assert.AreEqual(rows, 2, "this test assumes the default two rows");
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
            Assert.AreEqual(10, _geo.FirstChargingId);
        }

        [Test]
        public void Stations_LeaveTheWalkwayBesideTheCurbClear()
        {
            foreach (var spot in _geo.ChargingSpots)
                Assert.GreaterOrEqual(spot.xMin - _geo.Curb.xMax, 2f);
            Assert.GreaterOrEqual(_geo.Wash.xMin - _geo.Curb.xMax, 2f);
        }

        [Test]
        public void ColumnsFor_CountsTheStationCellsFirst_RoundsUpAndClamps()
        {
            Assert.AreEqual(2, _geo.ColumnsFor(0), "the stations alone fill the first two columns");
            Assert.AreEqual(2, _geo.ColumnsFor(1));
            Assert.AreEqual(3, _geo.ColumnsFor(2));
            Assert.AreEqual(3, _geo.ColumnsFor(3));
            Assert.AreEqual(4, _geo.ColumnsFor(4));
            Assert.AreEqual(_config.SpotsPerRow, _geo.ColumnsFor(_geo.Spots.Length));
            Assert.AreEqual(_config.SpotsPerRow, _geo.ColumnsFor(int.MaxValue / 2));
        }

        [Test]
        public void Frame_GrowsToTheRightOnlyWithMoreColumns()
        {
            var one = _geo.FrameFor(1);
            var four = _geo.FrameFor(4);
            Assert.AreEqual(one.xMin, four.xMin, 0.001f);
            Assert.AreEqual(one.yMin, four.yMin, 0.001f);
            Assert.AreEqual(one.yMax, four.yMax, 0.001f);
            Assert.AreEqual(3 * (_config.SpotSize.x + _config.SpotGap), four.xMax - one.xMax, 0.001f);
        }

        [Test]
        public void Frame_ContainsEveryRegularSpotItShows_AndTheMaxFrameIsTheBounds()
        {
            Assert.AreEqual(_geo.Bounds, _geo.FrameFor(_config.SpotsPerRow));
            var frame = _geo.FrameFor(_geo.ColumnsFor(3));
            Assert.IsTrue(Contains(frame, _geo.Spots[2]));
            foreach (var spot in _geo.ChargingSpots) Assert.IsTrue(Contains(frame, spot));
            Assert.IsTrue(Contains(frame, _geo.Wash));
        }

        [Test]
        public void StationsNoLongerStretchTheFrameUpward()
        {
            var spotsHalf = (_config.Rows * _config.SpotSize.y + (_config.Rows - 1) * _config.AisleWidth) * 0.5f;
            Assert.AreEqual(spotsHalf + _config.Margin, _geo.Bounds.yMax, 0.001f, "the top edge is the top spot row plus the margin");
        }

        [Test]
        public void NineSpotsFrame_StaysInsideTheWiderLot()
        {
            var frame = _geo.FrameFor(_geo.ColumnsFor(9));
            Assert.LessOrEqual(frame.width, _geo.Bounds.width);
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
