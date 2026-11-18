using System.IO;
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

        /// <summary>The layers, back to front: name, sorting order, solid.</summary>
        public static readonly (string Name, int Order, bool Solid)[] Layers =
        {
            ("Ground", -30, false), ("Sidewalk", -28, false), ("Markings", -27, false),
            ("Building", -5, true), ("Hedge", 0, true), ("Overhead", 5, false),
        };

        /// <summary>Creates the scene when the file does not exist.</summary>
        public static void CreateIfMissing()
        {
            if (File.Exists(HandmadeTileSetup.TemplateScenePath)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(HandmadeTileSetup.TemplateScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var grid = new GameObject(GridName).AddComponent<Grid>();
            grid.cellSize = new Vector3(HandmadePalette.CellUnits, HandmadePalette.CellUnits, 0f);
            foreach (var (name, order, solid) in Layers) AddLayer(grid.transform, name, order, solid);
            EditorSceneManager.SaveScene(scene, HandmadeTileSetup.TemplateScenePath);
        }

        private static void AddLayer(Transform grid, string name, int order, bool solid)
        {
            var go = new GameObject(name);
            go.transform.SetParent(grid, false);
            go.AddComponent<Tilemap>();
            go.AddComponent<TilemapRenderer>().sortingOrder = order;
            if (!solid) return;
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            var composite = go.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            go.AddComponent<TilemapCollider2D>().compositeOperation = Collider2D.CompositeOperation.Merge;
        }
    }
}
