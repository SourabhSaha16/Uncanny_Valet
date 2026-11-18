using System.IO;
using UncannyValet.Lot.Handmade;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>
    /// The lot authoring scene template: a Grid of 4x4 unit cells with the Tilemap layers of a hand-made lot. Building and Hedge are solid (a tilemap
    /// collider merged by a composite collider on a static body). Never overwritten once it exists; duplicate it to start a new lot.
    /// </summary>
    internal static class HandmadeTemplateScene
    {
        private const string GridName = "Grid";

        /// <summary>The layers, back to front: name, sorting order, solid, the kind the validator reads.</summary>
        public static readonly (string Name, int Order, bool Solid, TileLayerKind Kind)[] Layers =
        {
            ("Ground", -30, false, TileLayerKind.Ground), ("Sidewalk", -28, false, TileLayerKind.Sidewalk),
            ("Markings", -27, false, TileLayerKind.Markings), ("Building", -5, true, TileLayerKind.Building),
            ("Hedge", 0, true, TileLayerKind.Hedge), ("Overhead", 5, false, TileLayerKind.Overhead),
        };

        /// <summary>Creates the scene when the file does not exist; otherwise makes sure every layer carries its <see cref="LotTileLayer"/> marker.</summary>
        public static void CreateIfMissing()
        {
            if (File.Exists(HandmadeTileSetup.TemplateScenePath))
            {
                EnsureLayerMarkers();
                return;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(HandmadeTileSetup.TemplateScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildGrid(null);
            EditorSceneManager.SaveScene(scene, HandmadeTileSetup.TemplateScenePath);
        }

        /// <summary>A Grid of 4x4 unit cells with the six tilemap layers (each with its <see cref="LotTileLayer"/> marker), under <paramref name="parent"/> or at the scene root when null.</summary>
        internal static Grid BuildGrid(Transform parent)
        {
            var grid = new GameObject(GridName).AddComponent<Grid>();
            grid.transform.SetParent(parent, false);
            grid.cellSize = new Vector3(HandmadePalette.CellUnits, HandmadePalette.CellUnits, 0f);
            foreach (var (name, order, solid, kind) in Layers) AddLayer(grid.transform, name, order, solid, kind);
            return grid;
        }

        private static void AddLayer(Transform grid, string name, int order, bool solid, TileLayerKind kind)
        {
            var go = new GameObject(name);
            go.transform.SetParent(grid, false);
            go.AddComponent<Tilemap>();
            go.AddComponent<TilemapRenderer>().sortingOrder = order;
            SetKind(go.AddComponent<LotTileLayer>(), kind);
            if (!solid) return;
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            var composite = go.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            go.AddComponent<TilemapCollider2D>().compositeOperation = Collider2D.CompositeOperation.Merge;
        }

        // Opens the template next to what is open, adds the marker to any tilemap that lacks one (by layer name) and saves only when something changed.
        private static void EnsureLayerMarkers()
        {
            var scene = EditorSceneManager.OpenScene(HandmadeTileSetup.TemplateScenePath, OpenSceneMode.Additive);
            var changed = false;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var map in root.GetComponentsInChildren<Tilemap>(true))
                    changed |= EnsureMarker(map.gameObject);
            if (changed) EditorSceneManager.SaveScene(scene);
            EditorSceneManager.CloseScene(scene, true);
        }

        private static bool EnsureMarker(GameObject layer)
        {
            if (layer.GetComponent<LotTileLayer>() != null) return false;
            foreach (var (name, _, _, kind) in Layers)
            {
                if (name != layer.name) continue;
                SetKind(layer.AddComponent<LotTileLayer>(), kind);
                return true;
            }
            Debug.LogWarning("Template tilemap " + layer.name + " is not a known lot layer; give it a LotTileLayer by hand.", layer);
            return false;
        }

        private static void SetKind(LotTileLayer marker, TileLayerKind kind)
        {
            var so = new SerializedObject(marker);
            so.FindProperty("_kind").intValue = (int)kind;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
