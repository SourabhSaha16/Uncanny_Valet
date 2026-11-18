using NUnit.Framework;
using UncannyValet.Lot.Handmade;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>Validator rules 10 to 14 and 21.</summary>
    public class HandmadeRoadRulesTests
    {
        private HandmadeLotData _d;

        [SetUp]
        public void SetUp() => _d = HandmadeLotFixture.Valid();

        private bool Fails(LotRule rule) => HandmadeLotFixture.Has(HandmadeLotFixture.Check(_d), rule, LotSeverity.Error);

        [Test]
        public void Rule10_NoEntryRoad_Fails()
        {
            _d.EntryRoads.Clear();
            Assert.IsTrue(Fails(LotRule.EntryRoad));
        }

        [Test]
        public void Rule10_EntryLaneWithOnePoint_Fails()
        {
            var e = _d.EntryRoads[0];
            _d.EntryRoads[0] = new EntryRoadData(e.Name, new[] { e.Lane[0] }, e.QueueTurn, e.Bend);
            Assert.IsTrue(Fails(LotRule.EntryRoad));
        }

        [Test]
        public void Rule10_EntryStartsOnScreen_Fails()
        {
            var e = _d.EntryRoads[0];
            _d.EntryRoads[0] = new EntryRoadData(e.Name, new[] { new Vector2(-5f, -10f), e.Lane[1] }, e.QueueTurn, e.Bend);
            Assert.IsTrue(Fails(LotRule.EntryRoad));
        }

        [Test]
        public void Rule11_QueueTurnOffTheLane_Fails()
        {
            var e = _d.EntryRoads[0];
            _d.EntryRoads[0] = new EntryRoadData(e.Name, e.Lane, new Vector2(-2.4f, -12f), e.Bend);
            Assert.IsTrue(Fails(LotRule.QueueTurn));
        }

        [Test]
        public void Rule11_BendMissesSlotZero_Fails()
        {
            var e = _d.EntryRoads[0];
            _d.EntryRoads[0] = new EntryRoadData(e.Name, e.Lane, e.QueueTurn, new[] { new Vector2(5f, -11.6f) });
            Assert.IsTrue(Fails(LotRule.QueueTurn));
        }

        [Test]
        public void Rule12_NoExitRoad_Fails()
        {
            _d.ExitRoads.Clear();
            Assert.IsTrue(Fails(LotRule.ExitRoad));
        }

        [Test]
        public void Rule12_CurbExitStartsOutsideCurb_Fails()
        {
            var x = _d.ExitRoads[0];
            _d.ExitRoads[0] = new ExitRoadData(x.Name, x.Lane, new[] { new Vector2(0f, 6f), new Vector2(2f, 12f) });
            Assert.IsTrue(Fails(LotRule.ExitRoad));
        }

        [Test]
        public void Rule12_CurbExitMissesTheLane_Fails()
        {
            var x = _d.ExitRoads[0];
            _d.ExitRoads[0] = new ExitRoadData(x.Name, x.Lane, new[] { Vector2.zero, new Vector2(0f, 9f) });
            Assert.IsTrue(Fails(LotRule.ExitRoad));
        }

        [Test]
        public void Rule13_EntryRouteThroughASolid_Fails()
        {
            _d.Solids.Add(new LotPiece("Rock", new OrientedRect(new Vector2(0f, -12.5f), new Vector2(3f, 1f), 0f)));
            Assert.IsTrue(Fails(LotRule.RouteCrossesSolid));
        }

        [Test]
        public void Rule13_ExitRouteThroughASolid_Fails()
        {
            _d.Solids.Add(new LotPiece("Rock", new OrientedRect(new Vector2(0f, 6f), new Vector2(3f, 1f), 0f)));
            Assert.IsTrue(Fails(LotRule.RouteCrossesSolid));
        }

        [Test]
        public void Rule14_AmbientTrafficWithoutBackLane_Fails()
        {
            _d.Requirements.AmbientTraffic = true;
            _d.ExitBackLanes.Clear();
            Assert.IsTrue(Fails(LotRule.AmbientLanes));
        }

        [Test]
        public void Rule14_AmbientTrafficOffNeedsNoLane()
        {
            _d.ExitBackLanes.Clear();
            Assert.IsFalse(Fails(LotRule.AmbientLanes));
        }

        [Test]
        public void Rule21_PropOnARouteWithoutCollider_Warns()
        {
            _d.SceneryWithoutCollider.Add(new LotPiece("Barrel", new OrientedRect(new Vector2(0f, 6f), Vector2.one, 0f)));
            var issues = HandmadeLotFixture.Check(_d);
            Assert.IsTrue(HandmadeLotFixture.Has(issues, LotRule.SceneryOnRoute, LotSeverity.Warning));
            Assert.IsFalse(HandmadeLotRules.HasErrors(issues));
        }
    }
}
