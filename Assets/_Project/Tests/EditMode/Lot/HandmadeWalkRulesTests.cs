using NUnit.Framework;
using UncannyValet.Lot.Handmade;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>Validator rules 15 to 20, 22 and 23.</summary>
    public class HandmadeWalkRulesTests
    {
        private HandmadeLotData _d;

        [SetUp]
        public void SetUp() => _d = HandmadeLotFixture.Valid();

        private bool Fails(LotRule rule) => HandmadeLotFixture.Has(HandmadeLotFixture.Check(_d), rule, LotSeverity.Error);

        [Test]
        public void Rule15_NoMainDoor_Fails()
        {
            _d.Doors.RemoveAt(0);
            Assert.IsTrue(Fails(LotRule.Doors));
        }

        [Test]
        public void Rule15_NoWaitPoints_Fails()
        {
            _d.WaitPoints.Clear();
            Assert.IsTrue(Fails(LotRule.Doors));
        }

        [Test]
        public void Rule16_AmbientPeopleWithoutSpawnPoints_Fails()
        {
            _d.Requirements.AmbientPedestrians = 3;
            _d.SpawnPoints.Clear();
            Assert.IsTrue(Fails(LotRule.SpawnPoints));
        }

        [Test]
        public void Rule17_BrokenGraph_Fails()
        {
            _d.WalkLinks.RemoveAt(3);
            _d.WalkLinks.RemoveAt(3);
            Assert.IsTrue(Fails(LotRule.WalkConnected));
        }

        [Test]
        public void Rule17_NoWalkNodes_Fails()
        {
            _d.WalkNodes.Clear();
            _d.WalkLinks.Clear();
            Assert.IsTrue(Fails(LotRule.WalkConnected));
        }

        [Test]
        public void Rule18_LinkThroughASolid_Fails()
        {
            _d.WalkNodes.Add(new NamedPoint("Far", new Vector2(-12f, 0f)));
            _d.WalkLinks.Add(new WalkLinkData("Across", 0, 5));
            Assert.IsTrue(Fails(LotRule.WalkCrossesSolid));
        }

        [Test]
        public void Rule18_LinkToMissingNode_Fails()
        {
            _d.WalkLinks.Add(new WalkLinkData("Loose", 0, 40));
            Assert.IsTrue(Fails(LotRule.WalkCrossesSolid));
        }

        [Test]
        public void Rule19_NoValetStart_Fails()
        {
            _d.ValetStarts.Clear();
            Assert.IsTrue(Fails(LotRule.ValetStart));
        }

        [Test]
        public void Rule19_ValetInsideASolid_Fails()
        {
            _d.ValetStarts[0] = new NamedPoint("Valet", new Vector2(-8f, 1f));
            Assert.IsTrue(Fails(LotRule.ValetStart));
        }

        [Test]
        public void Rule20_HazardWithoutSize_Fails()
        {
            _d.Hazards.Add(new LotPiece("Mud", new OrientedRect(new Vector2(5f, 5f), Vector2.zero, 0f)));
            Assert.IsTrue(Fails(LotRule.HazardSize));
        }

        [Test]
        public void Rule22_ViewSmallerThanGameplay_Warns()
        {
            _d.HasView = true;
            _d.View = new Rect(0f, 0f, 4f, 4f);
            var issues = HandmadeLotFixture.Check(_d);
            Assert.IsTrue(HandmadeLotFixture.Has(issues, LotRule.ViewTooSmall, LotSeverity.Warning));
        }

        [Test]
        public void Rule22_ViewHoldingEverything_Passes()
        {
            _d.HasView = true;
            _d.View = new Rect(-12f, -14f, 30f, 30f);
            Assert.IsFalse(HandmadeLotFixture.Has(HandmadeLotFixture.Check(_d), LotRule.ViewTooSmall, LotSeverity.Warning));
        }

        [Test]
        public void Rule23_BuildingTilemapWithoutCollider_Warns()
        {
            _d.TileLayers.Add(new TileLayerData("Building", TileLayerKind.Building, false));
            _d.TileLayers.Add(new TileLayerData("Ground", TileLayerKind.Ground, false));
            var issues = HandmadeLotFixture.Check(_d);
            Assert.AreEqual(1, issues.FindAll(i => i.Rule == LotRule.TileCollider).Count);
            Assert.IsTrue(HandmadeLotFixture.Has(issues, LotRule.TileCollider, LotSeverity.Warning));
        }
    }
}
