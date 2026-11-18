using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Lot;
using UncannyValet.Lot.Handmade;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>The authored layers, fed the Diner's own numbers as hand-made data, answer like the parametric Diner.</summary>
    public class HandmadeDinerParityTests
    {
        private static readonly Vector2 Origin = new Vector2(7f, -3f);

        private LotConfig _config;
        private LotGeometry _geo;
        private RoadLayout _road;
        private PedestrianPathMap _paths;
        private HandmadeLotData _data;
        private AuthoredRoadLayout _roads;
        private AuthoredPedestrianPaths _walks;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<LotConfig>();
            _geo = new LotGeometry(_config);
            _road = new RoadLayout(_geo, _config.Site, Origin);
            _paths = new PedestrianPathMap(_geo, _config.Site, new WalkwayStyle(), Origin);
            _data = DinerLotData.From(_config, _geo, _road, _paths, Origin);
            _data.Requirements.TurnRadius = 0f;
            _roads = new AuthoredRoadLayout(_data);
            _walks = new AuthoredPedestrianPaths(_data);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void QueueSlots_MatchTheDinerColumn()
        {
            Assert.AreEqual(_geo.QueueSlots.Length, _roads.ArrivalSlots.Length);
            for (var i = 0; i < _roads.ArrivalSlots.Length; i++)
                Assert.AreEqual(0f, Vector2.Distance(_geo.QueueSlots[i] + Origin, _roads.ArrivalSlots[i]), 1e-4f);
        }

        [Test]
        public void Lanes_MatchTheDinerRoads()
        {
            AssertSame(_road.EastLane, _roads.EastLane);
            AssertSame(_road.TopLane, _roads.TopLane);
            AssertSame(_road.EastBackLane, _roads.EastBackLane);
            AssertSame(_road.TopBackLane, _roads.TopBackLane);
        }

        [Test]
        public void TurnAndMergeDistances_Match()
        {
            Assert.AreEqual(_road.QueueTurnDistance, _roads.QueueTurnDistance, 1e-3f);
            Assert.AreEqual(_road.CurbMergeDistance, _roads.CurbMergeDistance, 1e-3f);
        }

        [Test]
        public void QueueEntryRoutes_MatchForEverySlot()
        {
            var expected = new List<Vector2>();
            var actual = new List<Vector2>();
            for (var slot = 0; slot < _geo.QueueSlots.Length; slot++)
            {
                _road.QueueEntryRoute(slot, expected);
                _roads.QueueEntryRoute(slot, actual);
                AssertSame(expected, actual);
            }
        }

        [Test]
        public void CurbExitRoute_Matches()
        {
            var expected = new List<Vector2>();
            var actual = new List<Vector2>();
            _road.CurbExitRoute(expected);
            _roads.CurbExitRoute(actual);
            AssertSame(expected, actual);
        }

        [Test]
        public void DoorsAndCurbSpots_MatchTheDinerWalks()
        {
            Assert.AreEqual(_paths.DinerDoor, _walks.DinerDoor);
            Assert.AreEqual(_paths.CurbCenter.x, _walks.CurbCenter.x, 1e-4f);
            Assert.AreEqual(_paths.CurbCenter.y, _walks.CurbCenter.y, 1e-4f);
            Assert.AreEqual(_paths.CurbSlotCount, _walks.CurbSlotCount);
            for (var i = 0; i < _paths.CurbSlotCount; i++) Assert.AreEqual(_paths.CurbSlot(i), _walks.CurbSlot(i));
            Assert.AreEqual(_paths.RoadRouteCount, _walks.RoadRouteCount);
        }

        [Test]
        public void RoutesToTheDoor_StartAtThePointAndEndAtTheDoor()
        {
            var route = new List<Vector2>();
            var from = _geo.Spots[5].center + Origin;
            _walks.RouteToDoor(from, route);
            Assert.AreEqual(from, route[0]);
            Assert.AreEqual(_paths.DinerDoor, route[route.Count - 1]);
            _walks.RoadRoute(0, true, route);
            Assert.AreEqual(_paths.SideDoor, route[0]);
        }

        private static void AssertSame(IReadOnlyList<Vector2> expected, IReadOnlyList<Vector2> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);
            for (var i = 0; i < expected.Count; i++) Assert.AreEqual(0f, Vector2.Distance(expected[i], actual[i]), 1e-3f, "point " + i);
        }
    }
}
