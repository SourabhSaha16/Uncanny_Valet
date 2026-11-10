using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class DecorLayoutTests
    {
        private static readonly Vector2 BusStopSize = new Vector2(1.2f, 3f);
        private LotConfig _config;
        private LotGeometry _geo;
        private DecorLayout _layout;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<LotConfig>();
            _geo = new LotGeometry(_config);
            _layout = new DecorLayout(_geo, _config.WallThickness, 1.5f, 40f, BusStopSize, 2f);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void TheRoadIsTwoLanesBelowTheSouthWallThenAFarSidewalk()
        {
            Assert.AreEqual(_geo.Bounds.yMin - _config.WallThickness, _layout.EastLane.yMax, 1e-4f);
            Assert.AreEqual(_layout.EastLane.yMin, _layout.WestLane.yMax, 1e-4f);
            Assert.AreEqual(_layout.WestLane.yMin, _layout.FarSidewalk.yMax, 1e-4f);
            Assert.AreEqual(_geo.LaneWidth, _layout.EastLane.height, 1e-4f);
            Assert.AreEqual(2f * _geo.LaneWidth, _layout.Road.height, 1e-4f);
        }

        [Test]
        public void TheRoadReachesPastTheLotOnBothSides()
        {
            Assert.Less(_layout.Road.xMin, _geo.Bounds.xMin);
            Assert.Greater(_layout.Road.xMax, _geo.Bounds.xMax);
        }

        [Test]
        public void TheLaneCentresMatchTheGeometryLaneHeights()
        {
            Assert.AreEqual(_geo.EastLaneY, _layout.EastLane.center.y, 1e-4f);
            Assert.AreEqual(_geo.WestLaneY, _layout.WestLane.center.y, 1e-4f);
        }

        [Test]
        public void TheBusStopStandsOnTheRightStrip()
        {
            Assert.IsTrue(_geo.Strip.Contains(_layout.BusStop.min));
            Assert.IsTrue(_geo.Strip.Contains(_layout.BusStop.max));
        }

        [Test]
        public void TreesAndTuftsNeverStandInsideTheLotOrOnTheRoad()
        {
            foreach (var tree in _layout.TreeBases())
            {
                Assert.IsFalse(_geo.Bounds.Contains(tree), "tree " + tree);
                Assert.IsFalse(_layout.Road.Contains(tree), "tree on road " + tree);
            }
            foreach (var tuft in _layout.TuftSpots(60))
            {
                Assert.IsFalse(_geo.Bounds.Contains(tuft), "tuft " + tuft);
                Assert.IsFalse(_layout.Road.Contains(tuft), "tuft on road " + tuft);
            }
        }

        [Test]
        public void TuftsAreDeterministicAndCounted()
        {
            var a = _layout.TuftSpots(10);
            var b = _layout.TuftSpots(10);
            Assert.AreEqual(10, a.Length);
            Assert.AreEqual(a, b);
            Assert.AreEqual(0, _layout.TuftSpots(0).Length);
        }
    }
}
