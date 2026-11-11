using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class RoadLayoutTests
    {
        private static readonly Vector2 Origin = new Vector2(7f, -3f);

        private LotConfig _config;
        private LotGeometry _geo;
        private RoadLayout _road;
        private readonly List<Vector2> _route = new List<Vector2>();

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<LotConfig>();
            _geo = new LotGeometry(_config);
            _road = new RoadLayout(_geo, _config.Site, Origin);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void BothRoadsAreOneWayLeftToRight()
        {
            Assert.Less(_road.EastLane[0].x, _road.EastLane[1].x);
            Assert.Less(_road.TopLane[0].x, _road.TopLane[1].x);
            Assert.AreEqual(_road.EastLane[0].y, _road.EastLane[1].y, 1e-4f);
            Assert.AreEqual(_road.TopLane[0].y, _road.TopLane[1].y, 1e-4f);
        }

        [Test]
        public void TheEntryLaneIsBelowTheLotAndTheTopLaneAboveIt()
        {
            Assert.Less(_road.EastLane[0].y, _geo.Bounds.yMin + Origin.y);
            Assert.Greater(_road.TopLane[0].y, _geo.Bounds.yMax + Origin.y);
        }

        [Test]
        public void LanesStartAndEndWellOutsideTheLotWalls()
        {
            foreach (var lane in new[] { _road.EastLane, _road.TopLane })
            {
                Assert.Less(lane[0].x, _geo.Bounds.xMin + Origin.x);
                Assert.Greater(lane[1].x, _geo.Bounds.xMax + Origin.x);
            }
        }

        [Test]
        public void TheQueueTurnComesBeforeTheQueueColumnAndTheCurbMergeAfterTheCurb()
        {
            var start = _road.EastLane[0].x;
            Assert.Less(start + _road.QueueTurnDistance, _geo.QueueSlots[0].x + Origin.x);
            Assert.Greater(_road.TopLane[0].x + _road.CurbMergeDistance, _geo.Curb.center.x + Origin.x);
        }

        [Test]
        public void QueueRouteTurnsUpFromTheEastLaneAndEndsAtTheSlot()
        {
            for (var slot = 0; slot < _geo.QueueSlots.Length; slot++)
            {
                _road.QueueEntryRoute(slot, _route);
                Assert.AreEqual(_road.EastLane[0].y, _route[0].y, 1e-4f, "starts on the lane");
                Assert.AreEqual(_geo.QueueSlots[slot] + Origin, _route[_route.Count - 1]);
                Assert.AreEqual(_road.EastLane[0].x + _road.QueueTurnDistance, _route[0].x, 1e-3f);
            }
        }

        [Test]
        public void QueueRouteOnlyMovesRightAndUp()
        {
            _road.QueueEntryRoute(2, _route);
            for (var i = 1; i < _route.Count; i++)
            {
                Assert.GreaterOrEqual(_route[i].x, _route[i - 1].x - 1e-4f);
                Assert.GreaterOrEqual(_route[i].y, _route[i - 1].y - 1e-4f);
            }
        }

        [Test]
        public void CurbExitStartsAtTheCurbAndMergesIntoTheTopLaneMovingRightAndUp()
        {
            _road.CurbExitRoute(_route);
            Assert.AreEqual(_geo.Curb.center + Origin, _route[0]);
            var last = _route[_route.Count - 1];
            Assert.AreEqual(_road.TopLane[0].y, last.y, 1e-4f);
            Assert.AreEqual(_road.TopLane[0].x + _road.CurbMergeDistance, last.x, 1e-3f);
            for (var i = 1; i < _route.Count; i++)
            {
                Assert.GreaterOrEqual(_route[i].x, _route[i - 1].x - 1e-4f);
                Assert.GreaterOrEqual(_route[i].y, _route[i - 1].y - 1e-4f);
            }
        }

        [Test]
        public void CurbExitGoesStraightUpThroughTheTopWallGapThenHeadsRightOntoTheTopRoad()
        {
            _road.CurbExitRoute(_route);
            var wallTop = _geo.Bounds.yMax + Origin.y;
            var curbX = _geo.Curb.center.x + Origin.x;
            foreach (var p in _route)
            {
                if (p.y > _geo.Curb.yMax + Origin.y && p.y <= wallTop + _config.WallThickness)
                {
                    Assert.AreEqual(curbX, p.x, 1e-3f, "straight up while crossing the wall line");
                    Assert.GreaterOrEqual(p.x, _geo.ExitGap.x + Origin.x);
                    Assert.LessOrEqual(p.x, _geo.ExitGap.y + Origin.x);
                }
            }
            var end = _route[_route.Count - 1] - _route[_route.Count - 2];
            Assert.Greater(end.x, 0f, "the last leg heads right");
            Assert.Greater(end.x, end.y, "mostly right by the time it reaches the top road");
            Assert.AreEqual(_road.TopLane[0].y, _route[_route.Count - 1].y, 1e-3f, "and ends on the top lane");
        }

        [Test]
        public void CurbExitNeverGoesDownIntoTheEntryRoad()
        {
            _road.CurbExitRoute(_route);
            foreach (var p in _route) Assert.Greater(p.y, _geo.Bounds.yMin + Origin.y, "no exit point below the lot");
        }
    }
}
