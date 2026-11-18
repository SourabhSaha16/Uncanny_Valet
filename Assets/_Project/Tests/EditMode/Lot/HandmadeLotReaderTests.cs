using NUnit.Framework;
using UncannyValet.Lot;
using UncannyValet.Lot.Handmade;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>The reader turns marker objects into data in world space; the scene kit mirrors the valid fixture.</summary>
    public class HandmadeLotReaderTests
    {
        private static readonly Vector2 Offset = new Vector2(100f, -40f);

        private LotConfig _config;
        private HandmadeLot _lot;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<LotConfig>();
            _lot = HandmadeSceneKit.Build(Offset, _config);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_lot.gameObject);
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void ReadsEveryMarkerInWorldSpace()
        {
            var d = HandmadeLotReader.Read(_lot);
            Assert.AreEqual(1, d.RootCount);
            Assert.AreEqual(2, d.Bays.Count);
            Assert.AreEqual(new Vector2(6f, 0f) + Offset, d.Bays[0].Rect.Center);
            Assert.AreEqual(new Vector2(2.6f, 4.4f), d.Bays[0].Rect.Size);
            Assert.AreEqual(1, d.Curbs.Count);
            Assert.AreEqual(2, d.Slots.Count);
            Assert.AreEqual(1, d.Slots[1].Index);
            Assert.AreEqual(new Vector2(-30f, -14f) + Offset, d.EntryRoads[0].Lane[0]);
            Assert.AreEqual(new Vector2(-2.4f, -14f) + Offset, d.EntryRoads[0].QueueTurn);
            Assert.AreEqual(1, d.Solids.Count);
            Assert.AreEqual(new Vector2(1f, 10f), d.Solids[0].Rect.Size);
            Assert.AreEqual(5, d.WalkNodes.Count);
            Assert.AreEqual(5, d.WalkLinks.Count);
        }

        [Test]
        public void WaitPoints_AreOrderedByTheirOrderNumber()
        {
            var d = HandmadeLotReader.Read(_lot);
            Assert.AreEqual("Wait A", d.WaitPoints[0].Name);
            Assert.AreEqual("Wait B", d.WaitPoints[1].Name);
        }

        [Test]
        public void RotatedMarker_KeepsItsRotation()
        {
            _lot.transform.Find("Bay 1").rotation = Quaternion.Euler(0f, 0f, 30f);
            var d = HandmadeLotReader.Read(_lot);
            Assert.AreEqual(30f, d.Bays[0].Rect.RotationDegrees, 1e-3f);
        }

        [Test]
        public void TheKitLot_HasNoErrors()
        {
            var issues = HandmadeLotFixture.Check(HandmadeLotReader.Read(_lot));
            Assert.IsFalse(HandmadeLotRules.HasErrors(issues), string.Join("; ", issues));
        }

        [Test]
        public void MissingMarkers_AreReportedByTheRules()
        {
            Object.DestroyImmediate(_lot.transform.Find("Curb").gameObject);
            var issues = HandmadeLotFixture.Check(HandmadeLotReader.Read(_lot));
            Assert.IsTrue(HandmadeLotFixture.Has(issues, LotRule.OneCurb, LotSeverity.Error));
        }

        [Test]
        public void SolidScenery_WithoutCollider_IsListed_WithColliderIsNot()
        {
            var bare = HandmadeSceneKit.Add<ScenerySolid>(_lot.gameObject, "Barrel", new Vector2(20f, 20f));
            var solid = HandmadeSceneKit.Add<ScenerySolid>(_lot.gameObject, "Crate", new Vector2(25f, 20f));
            solid.gameObject.AddComponent<BoxCollider2D>();
            var d = HandmadeLotReader.Read(_lot);
            Assert.AreEqual(1, d.SceneryWithoutCollider.Count);
            Assert.AreEqual(bare.name, d.SceneryWithoutCollider[0].Name);
        }

        [Test]
        public void TileLayers_ReportTheirCollider()
        {
            var layer = HandmadeSceneKit.Add<LotTileLayer>(_lot.gameObject, "Building", Vector2.zero);
            HandmadeSceneKit.Set(layer, "_kind", TileLayerKind.Building);
            var d = HandmadeLotReader.Read(_lot);
            Assert.IsFalse(d.TileLayers[0].HasCollider);
            layer.gameObject.AddComponent<BoxCollider2D>();
            Assert.IsTrue(HandmadeLotReader.Read(_lot).TileLayers[0].HasCollider);
        }

        [Test]
        public void HazardPatch_BecomesAPieceWithItsSize()
        {
            var patch = HandmadeSceneKit.Add<HazardPatch>(_lot.gameObject, "Mud", new Vector2(5f, 8f));
            HandmadeSceneKit.Set(patch, "_height", 2f);
            var d = HandmadeLotReader.Read(_lot);
            Assert.AreEqual(new Vector2(3f, 2f), d.Hazards[0].Rect.Size);
        }
    }
}
