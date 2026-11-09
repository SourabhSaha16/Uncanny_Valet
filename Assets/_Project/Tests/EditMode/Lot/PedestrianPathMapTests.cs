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
        private WalkwayStyle _style;
        private DecorLayout _decor;
        private PedestrianPathMap _map;
        private readonly List<Vector2> _route = new List<Vector2>();

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<LotConfig>();
            _style = new WalkwayStyle();
            _decor = new DecorLayout(new LotGeometry(_config).Bounds, _config.WallThickness, 1.5f, 3f, 8f, 40f);
            _map = new PedestrianPathMap(_config, _decor, _style, 5f, Origin);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void EveryRoadRouteEndsAtTheDoorAndStartsOffTheLotSidewalk()
        {
            Assert.Greater(_map.RoadRouteCount, 0);
            for (var i = 0; i < _map.RoadRouteCount; i++)
            {
                _map.RoadRoute(i, false, _route);
                Assert.AreEqual(_map.DinerDoor.x, _route[_route.Count - 1].x, 1e-4f);
                Assert.AreEqual(_map.DinerDoor.y, _route[_route.Count - 1].y, 1e-4f);
                var startX = _route[0].x - Origin.x;
                Assert.IsTrue(startX < _decor.SouthSidewalk.xMin || startX > _decor.SouthSidewalk.xMax, "route " + i);
            }
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
        public void FarSideRoutesCrossTheRoadAtTheCrosswalk()
        {
            _map.RoadRoute(0, false, _route);
            var crossX = _config.CurbPosition.x + 5f + Origin.x;
            var onCrosswalk = 0;
            foreach (var p in _route) if (Mathf.Approximately(p.x, crossX)) onCrosswalk++;
            Assert.GreaterOrEqual(onCrosswalk, 2);
        }

        [Test]
        public void RouteFromACarGoesViaTheStripToTheDoor()
        {
            var from = new Vector2(_config.CurbPosition.x, -8f) + Origin;
            _map.RouteToDoor(from, _route);
            Assert.AreEqual(from.x, _route[0].x, 1e-3f);
            Assert.AreEqual(from.y, _route[0].y, 1e-3f);
            Assert.AreEqual(_map.DinerDoor.x, _route[_route.Count - 1].x, 1e-4f);
            Assert.AreEqual(WalkwayLayout.StripX(_config) + Origin.x, _route[1].x, 1e-4f);
        }

        [Test]
        public void CurbRouteStartsAtTheDoorAndEndsAtTheSlot()
        {
            for (var slot = 0; slot < _map.CurbSlotCount; slot++)
            {
                _map.RouteToCurb(slot, _route);
                Assert.AreEqual(_map.DinerDoor.x, _route[0].x, 1e-4f);
                Assert.AreEqual(_map.CurbSlot(slot).x, _route[_route.Count - 1].x, 1e-3f);
                Assert.AreEqual(_map.CurbSlot(slot).y, _route[_route.Count - 1].y, 1e-3f);
            }
        }

        [Test]
        public void CurbSlotsAreSpreadAlongTheCurbBesideTheZone()
        {
            Assert.AreEqual(_style.WaitSlots, _map.CurbSlotCount);
            var edge = _config.CurbPosition.x + _config.CurbSize.x * 0.5f + Origin.x;
            Assert.Greater(_map.CurbSlot(0).x, edge);
            Assert.AreNotEqual(_map.CurbSlot(0).y, _map.CurbSlot(1).y);
            Assert.AreEqual(_map.CurbSlot(_map.CurbSlotCount - 1), _map.CurbSlot(99));
        }
    }
}
