using System.IO;
using UncannyValet.Lot.Handmade;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>
    /// Generates <c>Lot_TestDiner.prefab</c>, the fixture lot of the hand-made lot tests and the sandbox: a modest lot on the square tiles with a roof on the
    /// left, a curb, 2 queue slots, 4 bays, an entry road below and an exit road above (each with a back lane), a main and a side door, wait and spawn
    /// points, a walk graph along a sidewalk, a wall and a hedge column. It passes the validator without a finding. Rebuilt in full each time.
    /// </summary>
    public static class HandmadeTestLot
    {
        public const string LotName = "TestDiner";
        public const string PrefabPath = HandmadeLotPrefabs.Folder + "/" + HandmadeLotPrefabs.NamePrefix + LotName + ".prefab";
        private const string RootPrefab = "HandmadeLotRoot";
        private static readonly Vector2[] BayCentres = { new Vector2(6f, 2f), new Vector2(10f, 2f), new Vector2(14f, 2f), new Vector2(18f, 2f) };
        private static readonly Vector2[] WalkPoints =
        {
            new Vector2(-6f, 0f), new Vector2(-3f, 0f), new Vector2(-3f, 6f), new Vector2(6f, 6f), new Vector2(14f, 6f), new Vector2(21f, 6f),
        };

        /// <summary>Builds the prefab (the marker prefabs and tiles must exist: run the lot kit first).</summary>
        public static void Build()
        {
            Directory.CreateDirectory(HandmadeLotPrefabs.Folder);
            var root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(HandmadeMarkerPrefabs.PathOf(RootPrefab)));
            root.name = LotName;
            HandmadeTestTiles.Paint(HandmadeTemplateScene.BuildGrid(root.transform));
            Bays(root.transform);
            Curb(root.transform);
            Roads(root.transform);
            People(root.transform);
            Solids(root.transform);
            Scenery(root.transform);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void Bays(Transform parent)
        {
            for (var i = 0; i < BayCentres.Length; i++)
                MarkerKit.SetInt(MarkerKit.Place(parent, "LotBay_Regular", "Bay " + (i + 1), BayCentres[i]), "_number", i + 1);
        }

        private static void Curb(Transform parent)
        {
            MarkerKit.Place(parent, "CurbZone", "Curb", Vector2.zero);
            MarkerKit.SetInt(MarkerKit.Place(parent, "QueueSlot", "Slot 0", new Vector2(0f, -5.55f)), "_index", 0);
            MarkerKit.SetInt(MarkerKit.Place(parent, "QueueSlot", "Slot 1", new Vector2(0f, -10.2f)), "_index", 1);
        }

        private static void Roads(Transform parent)
        {
            var entry = MarkerKit.Place(parent, "CarEntryRoad", "Car entry", Vector2.zero);
            MarkerKit.SetPoints(entry, "_lane", new Vector2(-30f, -14f), new Vector2(30f, -14f));
            MarkerKit.SetPoint(entry, "_queueTurn", new Vector2(-2.4f, -14f));
            MarkerKit.SetPoints(entry, "_bend", new Vector2(0f, -11.6f));
            var exit = MarkerKit.Place(parent, "CarExitRoad", "Car exit", Vector2.zero);
            MarkerKit.SetPoints(exit, "_lane", new Vector2(-30f, 12f), new Vector2(30f, 12f));
            MarkerKit.SetPoints(exit, "_curbExit", Vector2.zero, new Vector2(0f, 10f), new Vector2(2f, 12f));
            var entryBack = MarkerKit.Place(parent, "AmbientLane_EntryBack", "Entry back lane", Vector2.zero);
            MarkerKit.SetPoints(entryBack, "_points", new Vector2(30f, -17f), new Vector2(-30f, -17f));
            var exitBack = MarkerKit.Place(parent, "AmbientLane_ExitBack", "Exit back lane", Vector2.zero);
            MarkerKit.SetPoints(exitBack, "_points", new Vector2(30f, 15f), new Vector2(-30f, 15f));
        }

        private static void People(Transform parent)
        {
            MarkerKit.Place(parent, "LotDoor_Main", "Main door", new Vector2(-7f, 0f));
            MarkerKit.Place(parent, "LotDoor_Side", "Side door", new Vector2(-7f, 4f));
            MarkerKit.SetInt(MarkerKit.Place(parent, "PatronWaitPoint", "Wait 1", new Vector2(-3f, 0.5f)), "_order", 1);
            MarkerKit.SetInt(MarkerKit.Place(parent, "PatronWaitPoint", "Wait 2", new Vector2(-3f, -0.5f)), "_order", 2);
            MarkerKit.Place(parent, "AmbientSpawnPoint", "Ambient spawn", new Vector2(22f, 6f));
            MarkerKit.Place(parent, "ValetStart", "Valet start", new Vector2(3f, -1f));
            var nodes = new GameObject[WalkPoints.Length];
            for (var i = 0; i < nodes.Length; i++) nodes[i] = MarkerKit.Place(parent, "WalkNode", "Node " + i, WalkPoints[i]);
            for (var i = 0; i + 1 < nodes.Length; i++)
            {
                var link = MarkerKit.Place(parent, "WalkLink", "Link " + i, Vector2.zero);
                MarkerKit.SetReference(link, "_a", nodes[i].GetComponent<WalkNode>());
                MarkerKit.SetReference(link, "_b", nodes[i + 1].GetComponent<WalkNode>());
            }
        }

        private static void Solids(Transform parent)
        {
            MarkerKit.SetBox(MarkerKit.Place(parent, "SolidBlock", "Diner wall", new Vector2(-12f, 0f)), new Vector2(8f, 16f));
            MarkerKit.SetBox(MarkerKit.Place(parent, "SolidBlock", "Hedge wall", new Vector2(26f, -2f)), new Vector2(4f, 20f));
        }

        private static void Scenery(Transform parent)
        {
            var group = new GameObject("Scenery").transform;
            group.SetParent(parent, false);
            Prop(group, "TreeLarge", new Vector2(22f, -8f));
            Prop(group, "TreeSmall", new Vector2(-14f, -12f));
            Prop(group, "Bench", new Vector2(-5f, -6f));
            Prop(group, "LampPost", new Vector2(-5f, 3f));
        }

        private static void Prop(Transform parent, string name, Vector2 at)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HandmadeSceneryPrefabs.PathOf(name));
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.localPosition = at;
        }
    }
}
