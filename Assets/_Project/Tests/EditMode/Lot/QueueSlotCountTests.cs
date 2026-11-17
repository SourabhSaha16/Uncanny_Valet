using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>The queue slot count is a layout parameter: a fourth slot makes the lot taller below the curb.</summary>
    public sealed class QueueSlotCountTests
    {
        private LotConfig _config;

        [SetUp]
        public void SetUp() => _config = ScriptableObject.CreateInstance<LotConfig>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void ZeroSlots_UsesTheConfigsCount()
        {
            Assert.AreEqual(_config.QueueSlotCount, new LotGeometry(_config).QueueSlots.Length);
            Assert.AreEqual(_config.QueueSlotCount, new LotGeometry(_config, 0).QueueSlots.Length);
        }

        [Test]
        public void FourSlots_AreAllInsideTheWalls_AndTheLotGrowsDown()
        {
            var three = new LotGeometry(_config, 3);
            var four = new LotGeometry(_config, 4);
            Assert.AreEqual(4, four.QueueSlots.Length);
            Assert.Less(four.Bounds.yMin, three.Bounds.yMin);
            Assert.AreEqual(three.Bounds.yMax, four.Bounds.yMax, 0.0001f);
            foreach (var box in four.QueueCars)
            {
                Assert.GreaterOrEqual(box.yMin, four.Bounds.yMin);
                Assert.GreaterOrEqual(box.xMin, four.Bounds.xMin);
                Assert.LessOrEqual(box.xMax, four.Bounds.xMax);
            }
        }

        [Test]
        public void FourSlots_TheEntryOpeningStillCoversTheColumn_AndEveryRouteEndsAtItsSlot()
        {
            var geo = new LotGeometry(_config, 4);
            var road = new RoadLayout(geo, _config.Site, Vector2.zero);
            var route = new List<Vector2>();
            for (var slot = 0; slot < 4; slot++)
            {
                Assert.GreaterOrEqual(geo.QueueSlots[slot].x - _config.QueueCarSize.x * 0.5f, geo.EntryGap.x);
                Assert.LessOrEqual(geo.QueueSlots[slot].x + _config.QueueCarSize.x * 0.5f, geo.EntryGap.y);
                road.QueueEntryRoute(slot, route);
                Assert.AreEqual(geo.QueueSlots[slot], route[route.Count - 1]);
                Assert.Less(road.EastLane[0].y, geo.Bounds.yMin);
            }
        }
    }
}
