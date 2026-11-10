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
        public void EastLaneRunsLeftToRightAndWestLaneRightToLeft()
        {
            Assert.Less(_road.EastLane[0].x, _road.EastLane[1].x);
            Assert.Greater(_road.WestLane[0].x, _road.WestLane[1].x);
            Assert.AreEqual(_road.EastLane[0].y, _road.EastLane[1].y, 1e-4f);
            Assert.AreEqual(_road.WestLane[0].y, _road.WestLane[1].y, 1e-4f);
        }

        [Test]
        public void EastLaneIsNextToTheLotAndWestLaneBelowIt()
        {
            Assert.Greater(_road.EastLane[0].y, _road.WestLane[0].y);
            Assert.Less(_road.EastLane[0].y, _geo.Bounds.yMin + Origin.y);
        }

        [Test]
        public void LanesStartAndEndWellOutsideTheLotWalls()
        {
            Assert.Less(_road.EastLane[0].x, _geo.Bounds.xMin + Origin.x);
            Assert.Greater(_road.EastLane[1].x, _geo.Bounds.xMax + Origin.x);
            Assert.Greater(_road.WestLane[0].x, _geo.Bounds.xMax + Origin.x);
            Assert.Less(_road.WestLane[1].x, _geo.Bounds.xMin + Origin.x);
        }

        [Test]
        public void TheQueueTurnComesBeforeTheQueueColumnAndTheCurbMergeAfterTheExit()
        {
            var start = _road.EastLane[0].x;
            Assert.Less(start + _road.QueueTurnDistance, _geo.QueueSlots[0].x + Origin.x);
            Assert.Greater(start + _road.CurbMergeDistance, _geo.ExitX + Origin.x);
            Assert.Less(_road.QueueTurnDistance, _road.CurbMergeDistance, "cars turn into the queue before the point where leavers merge");
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
        public void CurbExitStartsAtTheCurbAndMergesIntoTheEastLaneMovingRightAndDown()
        {
            _road.CurbExitRoute(_route);
            Assert.AreEqual(_geo.Curb.center + Origin, _route[0]);
            var last = _route[_route.Count - 1];
            Assert.AreEqual(_road.EastLane[0].y, last.y, 1e-4f);
            Assert.AreEqual(_road.EastLane[0].x + _road.CurbMergeDistance, last.x, 1e-3f);
            for (var i = 1; i < _route.Count; i++)
            {
                Assert.GreaterOrEqual(_route[i].x, _route[i - 1].x - 1e-4f);
                Assert.LessOrEqual(_route[i].y, _route[i - 1].y + 1e-4f);
            }
        }

        [Test]
        public void CurbExitPassesBesideTheQueueColumnNotThroughIt()
        {
            _road.CurbExitRoute(_route);
            var queueRight = _geo.QueueCars[0].xMax + Origin.x;
            for (var i = 2; i < _route.Count; i++)
                Assert.Greater(_route[i].x, queueRight, "point " + i + " clears the queue cars");
        }
    }
}
