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
            for (var i = 0; i < _geo.Stations.Length; i++) all["station" + i] = _geo.Stations[i];
            return all;
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
        public void StationCount_IsChargingBaysPlusOneWash()
        {
            Assert.AreEqual(_config.ChargingBays + 1, _geo.Stations.Length);
        }

        [Test]
        public void StationsLeaveAWalkwayToTheSpots()
        {
            foreach (var station in _geo.Stations)
                Assert.GreaterOrEqual(_geo.Spots[0].xMin - station.xMax, 2f);
        }

        [Test]
        public void SlotZero_IsTheFirstQueueCar_BelowTheCurb()
        {
            Assert.AreEqual(_geo.QueueSlots[0], _geo.QueueCars[0].center);
            Assert.Less(_geo.QueueCars[0].yMax, _geo.Curb.yMin);
        }
    }
}
