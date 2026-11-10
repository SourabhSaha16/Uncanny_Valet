using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class PedestrianPathMapTests
    {
        private static readonly Vector2 Origin = new Vector2(100f, 50f);

        private LotConfig _config;
        private LotGeometry _geo;
        private WalkwayStyle _style;
        private PedestrianPathMap _map;
        private readonly List<Vector2> _route = new List<Vector2>();

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<LotConfig>();
            _geo = new LotGeometry(_config);
            _style = new WalkwayStyle();
            _map = new PedestrianPathMap(_geo, _config.Site, _style, Origin);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void EveryRoadRouteStartsOnTheRightStripAndEndsAtTheSideDoor()
        {
            Assert.Greater(_map.RoadRouteCount, 0);
            for (var i = 0; i < _map.RoadRouteCount; i++)
            {
                _map.RoadRoute(i, false, _route);
                Assert.IsTrue(_geo.Strip.Contains(_route[0] - Origin), "route " + i + " starts on the strip");
                Assert.AreEqual(_map.SideDoor.x, _route[_route.Count - 1].x, 1e-4f);
                Assert.AreEqual(_map.SideDoor.y, _route[_route.Count - 1].y, 1e-4f);
            }
        }

        [Test]
        public void AmbientRoutesNeverUseTheMainDoor()
        {
            _map.RoadRoute(0, false, _route);
            Assert.AreNotEqual(_map.DinerDoor.y, _route[_route.Count - 1].y);
            Assert.Greater(_map.SideDoor.y, _map.DinerDoor.y, "the side door is the upper door");
        }

        [Test]
        public void RoadRoutesGoAlongTheAisleCrossingThenUpTheNorthCrosswalk()
        {
            _map.RoadRoute(0, false, _route);
            var crossX = _geo.CrossingX + Origin.x;
            var aisleY = _geo.AisleY + Origin.y;
            Assert.IsTrue(_route.Exists(p => Mathf.Approximately(p.x, crossX) && Mathf.Approximately(p.y, aisleY)));
            Assert.IsTrue(_route.Exists(p => Mathf.Approximately(p.x, crossX) && Mathf.Approximately(p.y, _map.SideDoor.y)));
        }

        [Test]
        public void LeavingIsTheReverseWalk()
        {
            var forward = new List<Vector2>();
            _map.RoadRoute(0, false, forward);
            _map.RoadRoute(0, true, _route);
            Assert.AreEqual(forward.Count, _route.Count);
            for (var i = 0; i < forward.Count; i++) Assert.AreEqual(forward[i], _route[forward.Count - 1 - i]);
        }

        [Test]
        public void PatronRouteFromAQueueSlotUsesTheCorridorCrosswalkAndEndsAtTheMainDoor()
        {
            var from = _geo.QueueSlots[1] + Origin;
            _map.RouteToDoor(from, _route);
            Assert.AreEqual(from, _route[0]);
            Assert.AreEqual(_map.DinerDoor, _route[_route.Count - 1]);
            Assert.AreEqual(_geo.CrossingX + Origin.x, _route[1].x, 1e-4f);
        }

        [Test]
        public void PatronRouteFromABayLeavesOntoTheAisleFirst()
        {
            var from = _geo.Spots[2].center + Origin;
            _map.RouteToDoor(from, _route);
            Assert.AreEqual(from.x, _route[1].x, 1e-4f);
            Assert.AreEqual(_geo.AisleY + Origin.y, _route[1].y, 1e-4f);
            Assert.AreEqual(_map.DinerDoor, _route[_route.Count - 1]);
        }

        [Test]
        public void TheMainDoorIsBesideTheCurb()
        {
            Assert.AreEqual(_geo.Curb.center.y + Origin.y, _map.DinerDoor.y, 1e-4f);
            Assert.Less(_map.DinerDoor.x, _geo.Curb.xMin + Origin.x);
        }

        [Test]
        public void CurbRouteStartsAtTheMainDoorAndEndsAtTheSlot()
        {
            for (var slot = 0; slot < _map.CurbSlotCount; slot++)
            {
                _map.RouteToCurb(slot, _route);
                Assert.AreEqual(_map.DinerDoor, _route[0]);
                Assert.AreEqual(_map.CurbSlot(slot), _route[_route.Count - 1]);
            }
        }

        [Test]
        public void CurbSlotsAreSpreadAlongTheCurbInTheGapWestOfTheCurbZone()
        {
            Assert.AreEqual(_style.WaitSlots, _map.CurbSlotCount);
            for (var slot = 0; slot < _map.CurbSlotCount; slot++)
            {
                var at = _map.CurbSlot(slot);
                Assert.Greater(at.x, _geo.Diner.xMax + Origin.x, "east of the diner wall");
                Assert.Less(at.x, _geo.Curb.xMin + Origin.x, "west of the curb zone");
                Assert.Less(at.x, _map.CurbCenter.x, "the curb zone is to the patron's east");
            }
            Assert.IsTrue(_geo.CurbGap.Contains(_map.CurbSlot(0) - Origin));
            Assert.AreNotEqual(_map.CurbSlot(0).y, _map.CurbSlot(1).y);
            Assert.AreEqual(_map.CurbSlot(_map.CurbSlotCount - 1), _map.CurbSlot(99));
        }
    }
}
