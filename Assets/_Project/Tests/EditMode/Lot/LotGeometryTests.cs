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
            var all = new Dictionary<string, Rect> { ["curb"] = _geo.Curb, ["wash"] = _geo.Wash, ["diner"] = _geo.Diner, ["strip"] = _geo.Strip };
            for (var i = 0; i < _geo.Spots.Length; i++) all["spot" + i] = _geo.Spots[i];
            for (var i = 0; i < _geo.QueueCars.Length; i++) all["queue" + i] = _geo.QueueCars[i];
            for (var i = 0; i < _geo.ChargingSpots.Length; i++) all["charging" + i] = _geo.ChargingSpots[i];
            return all;
        }

        [Test]
        public void TheGridHasSixRegularSpotsOneChargingBayAndTheWash()
        {
            Assert.AreEqual(6, _geo.Spots.Length);
            Assert.AreEqual(1, _geo.ChargingSpots.Length);
            Assert.AreEqual(7, _geo.FirstChargingId, "the EV bay is id 7");
            Assert.AreEqual(BayPlan.Columns * BayPlan.Rows, _geo.Spots.Length + _geo.ChargingSpots.Length + 1);
        }

        [Test]
        public void TopRowIsFourRegularSpotsInReadingOrder()
        {
            for (var i = 1; i < 4; i++)
            {
                Assert.AreEqual(_geo.Spots[0].y, _geo.Spots[i].y, 0.001f, "spots 1..4 share the top row");
                Assert.Greater(_geo.Spots[i].x, _geo.Spots[i - 1].x);
            }
        }

        [Test]
        public void BottomRowIsEvRegularWashRegular()
        {
            var row = _geo.ChargingSpots[0].y;
            Assert.Less(row, _geo.Spots[0].y, "the bottom row is below the top row");
            Assert.AreEqual(row, _geo.Spots[4].y, 0.001f);
            Assert.AreEqual(row, _geo.Wash.y, 0.001f);
            Assert.AreEqual(row, _geo.Spots[5].y, 0.001f);
            Assert.Less(_geo.ChargingSpots[0].x, _geo.Spots[4].x);
            Assert.Less(_geo.Spots[4].x, _geo.Wash.x);
            Assert.Less(_geo.Wash.x, _geo.Spots[5].x);
            Assert.AreEqual(_geo.Spots[0].x, _geo.ChargingSpots[0].x, 0.001f, "the EV bay is under the first top bay");
        }

        [Test]
        public void EveryBayHasTheConfiguredSize()
        {
            foreach (var spot in _geo.Spots) Assert.AreEqual(_config.SpotSize, spot.size);
            Assert.AreEqual(_config.SpotSize, _geo.ChargingSpots[0].size);
            Assert.AreEqual(_config.SpotSize, _geo.Wash.size);
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
                Assert.GreaterOrEqual(pair.Value.xMin, _geo.Bounds.xMin - 1e-4f, pair.Key);
                Assert.LessOrEqual(pair.Value.xMax, _geo.Bounds.xMax + 1e-4f, pair.Key);
                Assert.GreaterOrEqual(pair.Value.yMin, _geo.Bounds.yMin - 1e-4f, pair.Key);
                Assert.LessOrEqual(pair.Value.yMax, _geo.Bounds.yMax + 1e-4f, pair.Key);
            }
        }

        [Test]
        public void CurbSitsRightOfTheMainDoorAndTheSideDoorIsAboveIt()
        {
            Assert.AreEqual(_geo.Diner.xMax, _geo.Curb.xMin, 1e-4f, "the curb touches the diner");
            Assert.AreEqual(_geo.Curb.center.y, _geo.MainDoor.center.y, 1e-4f);
            Assert.Greater(_geo.SideDoor.yMin, _geo.Curb.yMax, "the side door is above the curb");
            Assert.Greater(_geo.SideDoor.center.y, _geo.MainDoor.center.y);
            Assert.AreEqual(_geo.MainDoor.xMax, _geo.Diner.xMax, 1e-4f, "both doors sit on the diner's right face");
        }

        [Test]
        public void QueueIsAColumnOfThreeUnderTheCurb()
        {
            Assert.AreEqual(3, _geo.QueueSlots.Length);
            for (var i = 1; i < 3; i++)
            {
                Assert.AreEqual(_geo.QueueSlots[0].x, _geo.QueueSlots[i].x, 0.001f);
                Assert.Less(_geo.QueueSlots[i].y, _geo.QueueSlots[i - 1].y, "later slots are lower");
            }
            Assert.AreEqual(_geo.Curb.center.x, _geo.QueueSlots[0].x, 0.001f);
            Assert.Less(_geo.QueueCars[0].yMax, _geo.Curb.yMin, "slot 0 is just below the curb");
        }

        [Test]
        public void TheCorridorBesideTheQueueAndCurbIsClearOfBays()
        {
            foreach (var spot in _geo.Spots) Assert.GreaterOrEqual(spot.xMin, _geo.Corridor.xMax - 1e-4f);
            Assert.GreaterOrEqual(_geo.Wash.xMin, _geo.Corridor.xMax - 1e-4f);
            Assert.Greater(_geo.CrossingX, _geo.Curb.xMax);
            Assert.Greater(_geo.ExitX, _geo.CrossingX);
            Assert.Less(_geo.ExitX, _geo.Corridor.xMax);
        }

        [Test]
        public void TheSouthWallOpeningCoversTheQueueColumnAndTheExit()
        {
            Assert.LessOrEqual(_geo.EntryGap.x, _geo.QueueCars[0].xMin);
            Assert.GreaterOrEqual(_geo.EntryGap.y, _geo.ExitX + _config.QueueCarSize.x * 0.5f);
        }

        [Test]
        public void AisleIsBetweenTheTwoBayRows()
        {
            Assert.Less(_geo.AisleY, _geo.Spots[0].yMin);
            Assert.Greater(_geo.AisleY, _geo.ChargingSpots[0].yMax);
        }

        [Test]
        public void StripIsRightOfEveryBayAndTouchesTheRightWall()
        {
            foreach (var spot in _geo.Spots) Assert.GreaterOrEqual(_geo.Strip.xMin, spot.xMax);
            Assert.AreEqual(_geo.Bounds.xMax, _geo.Strip.xMax, 1e-4f);
        }

        [Test]
        public void TheRoadLiesBelowTheLotWithTheEastLaneNextToIt()
        {
            Assert.Less(_geo.EastLaneY + _geo.LaneWidth * 0.5f, _geo.Bounds.yMin - _config.WallThickness + 1e-4f);
            Assert.AreEqual(_geo.LaneWidth, _geo.EastLaneY - _geo.WestLaneY, 1e-4f);
            Assert.Less(_geo.RoadSpan.x, _geo.Bounds.xMin);
            Assert.Greater(_geo.RoadSpan.y, _geo.Bounds.xMax);
        }
    }
}
