using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.EditorTools.HandmadeLots;
using UncannyValet.Lot.Handmade;
using UncannyValet.Tests.Lot;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UncannyValet.Tests.Integration
{
    /// <summary>The editor tooling of the hand-made lots (#194): walk auto-link plan, prefab paths, the marker prefab generator, template layer markers, and the shipped lot prefabs.</summary>
    public class HandmadeEditorToolsTests
    {
        [Test]
        public void AutoLinker_LinksUnlinkedNeighbours_AndSkipsLinkedOnes()
        {
            var data = new HandmadeLotData();
            data.WalkNodes.Add(new NamedPoint("A", Vector2.zero));
            data.WalkNodes.Add(new NamedPoint("B", new Vector2(4f, 0f)));
            data.WalkNodes.Add(new NamedPoint("C", new Vector2(4f, 4f)));
            data.WalkLinks.Add(new WalkLinkData("AB", 0, 1));
            var plan = WalkAutoLinker.Plan(data, 10f);
            CollectionAssert.AreEquivalent(new[] { (0, 2), (1, 2) }, plan);
        }

        [Test]
        public void AutoLinker_SkipsLinksThroughASolid_AndPastTheRadius()
        {
            var data = new HandmadeLotData();
            data.WalkNodes.Add(new NamedPoint("A", Vector2.zero));
            data.WalkNodes.Add(new NamedPoint("B", new Vector2(6f, 0f)));
            data.WalkNodes.Add(new NamedPoint("Far", new Vector2(40f, 0f)));
            data.Solids.Add(new LotPiece("Wall", new OrientedRect(new Vector2(3f, 0f), new Vector2(1f, 6f), 0f)));
            Assert.IsEmpty(WalkAutoLinker.Plan(data, 10f));
        }

        [Test]
        public void AutoLinker_LinksThroughTheMiddleNode_NotAroundIt()
        {
            var data = new HandmadeLotData();
            data.WalkNodes.Add(new NamedPoint("A", Vector2.zero));
            data.WalkNodes.Add(new NamedPoint("Mid", new Vector2(4f, 0f)));
            data.WalkNodes.Add(new NamedPoint("B", new Vector2(8f, 0f)));
            CollectionAssert.AreEquivalent(new[] { (0, 1), (1, 2) }, WalkAutoLinker.Plan(data, 10f));
        }

        [TestCase("Diner", "Assets/_Project/Prefabs/HandmadeLots/Lot_Diner.prefab")]
        [TestCase("Lot_Diner", "Assets/_Project/Prefabs/HandmadeLots/Lot_Diner.prefab")]
        [TestCase(" Back Yard ", "Assets/_Project/Prefabs/HandmadeLots/Lot_Back Yard.prefab")]
        public void LotPrefabPath_AddsThePrefixOnce(string name, string expected) => Assert.AreEqual(expected, HandmadeLotPrefabs.PathFor(name));

        [Test]
        public void MarkerPrefabs_AreNineteen_AndRegeneratingKeepsGuidsAndValues()
        {
            Assert.AreEqual(19, HandmadeMarkerPrefabs.Count);
            var before = Snapshot();
            HandmadeMarkerPrefabs.Build();
            AssetDatabase.Refresh();
            var after = Snapshot();
            Assert.AreEqual(before.Count, after.Count);
            foreach (var pair in before) Assert.AreEqual(pair.Value, after[pair.Key], pair.Key + " changed on regeneration");
        }

        [Test]
        public void TemplateScene_EveryTilemapHasItsLotTileLayerMarker()
        {
            var scene = EditorSceneManager.OpenScene(HandmadeTileSetup.TemplateScenePath, OpenSceneMode.Additive);
            try
            {
                var seen = 0;
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var map in root.GetComponentsInChildren<Tilemap>(true))
                    {
                        var marker = map.GetComponent<LotTileLayer>();
                        Assert.IsNotNull(marker, map.name + " has no LotTileLayer");
                        Assert.AreEqual(map.name, marker.Kind.ToString(), map.name + " kind");
                        seen++;
                    }
                Assert.AreEqual(6, seen);
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }

        [Test]
        public void EveryHandmadeLotPrefab_PassesTheValidatorWithoutErrors()
        {
            var paths = HandmadeLotPrefabs.AllLotPaths();
            Assert.IsNotEmpty(paths, "no hand-made lot prefab under " + HandmadeLotPrefabs.Folder);
            foreach (var path in paths)
            {
                var contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var lot = contents.GetComponent<HandmadeLot>();
                    Assert.IsNotNull(lot, path + " has no HandmadeLot on its root");
                    var issues = LotValidation.Check(lot);
                    Assert.IsFalse(HandmadeLotRules.HasErrors(issues), path + ": " + LotValidation.ErrorSummary(issues));
                }
                finally { PrefabUtility.UnloadPrefabContents(contents); }
            }
        }

        [Test]
        public void Validation_ReportsTheErrorsOfASceneLot_AndFindsTheMarker()
        {
            var lot = HandmadeSceneKit.Build(Vector2.zero, null);
            try
            {
                Object.DestroyImmediate(lot.GetComponentInChildren<CurbZoneMarker>().gameObject);
                var issues = LotValidation.Check(lot);
                Assert.IsTrue(HandmadeLotRules.HasErrors(issues));
                StringAssert.Contains("curb", LotValidation.ErrorSummary(issues).ToLowerInvariant());
                Assert.AreSame(lot.gameObject, LotValidation.Find(lot, new LotIssue(LotRule.OneCurb, LotSeverity.Error, "x")));
            }
            finally { Object.DestroyImmediate(lot.gameObject); }
        }

        private static Dictionary<string, string> Snapshot()
        {
            var map = new Dictionary<string, string>();
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { HandmadeMarkerPrefabs.Folder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                map[path] = guid + "|" + System.IO.File.ReadAllText(path);
            }
            return map;
        }
    }
}
