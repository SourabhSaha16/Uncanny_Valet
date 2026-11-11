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
        public void TheEntryRoadIsTwoLanesBelowTheSouthWallThenASidewalk()
        {
            Assert.AreEqual(_geo.Bounds.yMin - _config.WallThickness, _layout.EastLane.yMax, 1e-4f);
            Assert.AreEqual(_layout.EastLane.yMin, _layout.EastBackLane.yMax, 1e-4f, "the opposite lane lies next to the patron lane, away from the lot");
            Assert.AreEqual(_layout.EastBackLane.yMin, _layout.FarSidewalk.yMax, 1e-4f);
            Assert.AreEqual(_geo.EastBackLaneY, _layout.EastBackLane.center.y, 1e-4f);
            Assert.AreEqual(_geo.LaneWidth, _layout.EastLane.height, 1e-4f);
        }

        [Test]
        public void TheExitRoadIsTwoLanesAboveTheTopWallThenASidewalk()
        {
            Assert.AreEqual(_geo.Bounds.yMax + _config.WallThickness, _layout.TopLane.yMin, 1e-4f);
            Assert.AreEqual(_layout.TopLane.yMax, _layout.TopBackLane.yMin, 1e-4f);
            Assert.AreEqual(_layout.TopBackLane.yMax, _layout.TopSidewalk.yMin, 1e-4f);
            Assert.AreEqual(_geo.TopBackLaneY, _layout.TopBackLane.center.y, 1e-4f);
            Assert.AreEqual(_layout.EastLane.height, _layout.TopLane.height, 1e-4f, "same style as the entry road");
            Assert.AreEqual(_layout.EastLane.width, _layout.TopLane.width, 1e-4f);
        }

        [Test]
        public void BothRoadsReachPastTheLotOnBothSides()
        {
            Assert.Less(_layout.EastLane.xMin, _geo.Bounds.xMin);
            Assert.Less(_layout.TopLane.xMin, _geo.Bounds.xMin);
            Assert.Greater(_layout.EastLane.xMax, _geo.Bounds.xMax);
            Assert.Greater(_layout.TopLane.xMax, _geo.Bounds.xMax);
        }

        [Test]
        public void TheLaneCentresMatchTheGeometryLaneHeights()
        {
            Assert.AreEqual(_geo.EastLaneY, _layout.EastLane.center.y, 1e-4f);
            Assert.AreEqual(_geo.TopLaneY, _layout.TopLane.center.y, 1e-4f);
        }

        [Test]
        public void TheCameraFramingStaysWhereItWasWithOneEntryLane()
        {
            Assert.AreEqual(_layout.EastLane.yMin - 1.5f, _layout.ViewBottom, 1e-4f);
        }

        [Test]
        public void TheBusStopStandsOnTheRightStrip()
        {
            Assert.IsTrue(_geo.Strip.Contains(_layout.BusStop.min));
            Assert.IsTrue(_geo.Strip.Contains(_layout.BusStop.max));
        }

        [Test]
        public void TheOutsideGroundCoversTheLotTheRoadAndTheFarSidewalk()
        {
            Assert.IsTrue(_layout.Outside.Contains(_geo.Bounds.min));
            Assert.IsTrue(_layout.Outside.Contains(_geo.Bounds.max));
            Assert.IsTrue(_layout.Outside.Contains(_layout.EastLane.min));
            Assert.IsTrue(_layout.Outside.Contains(_layout.TopLane.max));
            Assert.IsTrue(_layout.Outside.Contains(_layout.FarSidewalk.min));
            Assert.IsTrue(_layout.Outside.Contains(_layout.TopSidewalk.max));
        }

        [Test]
        public void TheOutsideGroundReachesPastTheWallsOnEverySide()
        {
            Assert.Less(_layout.Outside.xMin, _geo.Bounds.xMin - _config.WallThickness);
            Assert.Greater(_layout.Outside.xMax, _geo.Bounds.xMax + _config.WallThickness);
            Assert.Greater(_layout.Outside.yMax, _geo.Bounds.yMax + _config.WallThickness);
        }
    }
}
