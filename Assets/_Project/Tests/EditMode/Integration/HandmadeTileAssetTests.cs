using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.EditorTools.HandmadeLots;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace UncannyValet.Tests.Integration
{
    /// <summary>The hand-made lot authoring kit (#194): square tiles, one Tile asset per tile, a palette that holds them all, the scenery prefabs and the scene template.</summary>
    public class HandmadeTileAssetTests
    {
        private const int TilePixels = 88;
        private const float PixelsPerUnit = 22f;
        private const float TileUnits = 4f;
        private static readonly string[] LayerNames = { "Ground", "Sidewalk", "Markings", "Building", "Hedge", "Overhead" };
        private static readonly int[] LayerOrders = { -30, -28, -27, -5, 0, 5 };

        private static List<Sprite> TileSprites()
        {
            var sprites = new List<Sprite>();
            foreach (var guid in AssetDatabase.FindAssets("t:Sprite", new[] { HandmadeTileSetup.SpriteFolder }))
                sprites.Add(AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)));
            return sprites;
        }

        [Test]
        public void TileSet_HasTilesOfEveryGroup()
        {
            var groups = new HashSet<string>();
            foreach (var sprite in TileSprites()) groups.Add(HandmadePaletteGroup(sprite.name));
            foreach (var expected in new[] { "ground", "sidewalk", "marking", "hedge", "building", "roof", "overhead" })
                Assert.IsTrue(groups.Contains(expected), "no tile of the group " + expected);
        }

        [Test]
        public void EveryTile_Is88By88Pixels_AndFourUnitsAcross()
        {
            var sprites = TileSprites();
            Assert.IsNotEmpty(sprites, "run tools/slice-valhalla-tiles.py");
            foreach (var sprite in sprites)
            {
                Assert.AreEqual(TilePixels, (int)sprite.rect.width, sprite.name + " width");
                Assert.AreEqual(TilePixels, (int)sprite.rect.height, sprite.name + " height");
                Assert.AreEqual(PixelsPerUnit, sprite.pixelsPerUnit, sprite.name + " pixels per unit");
                Assert.AreEqual(TileUnits, sprite.bounds.size.x, 1e-3f, sprite.name + " width in units");
                Assert.AreEqual(TileUnits, sprite.bounds.size.y, 1e-3f, sprite.name + " height in units");
            }
        }

        [Test]
        public void EveryTileSprite_HasATileAsset_AndThePaletteHoldsAllOfThem()
        {
            var palette = AssetDatabase.LoadAssetAtPath<GameObject>(HandmadeTileSetup.PalettePath);
            Assert.IsNotNull(palette, "palette prefab missing: run Uncanny Valet/Setup/Hand-made Lot Kit");
            var map = palette.GetComponentInChildren<Tilemap>();
            var inPalette = new HashSet<TileBase>();
            foreach (var position in map.cellBounds.allPositionsWithin)
                if (map.GetTile(position) != null) inPalette.Add(map.GetTile(position));
            foreach (var sprite in TileSprites())
            {
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(HandmadeTileSetup.TileFolder + "/" + sprite.name + ".asset");
                Assert.IsNotNull(tile, "no Tile asset for " + sprite.name);
                Assert.AreSame(sprite, tile.sprite, sprite.name + " tile sprite");
                Assert.IsTrue(inPalette.Contains(tile), sprite.name + " is not in the palette");
            }
        }

        [Test]
        public void Palette_GridIsFourUnitCells()
        {
            var palette = AssetDatabase.LoadAssetAtPath<GameObject>(HandmadeTileSetup.PalettePath);
            var grid = palette.GetComponent<Grid>();
            Assert.AreEqual(TileUnits, grid.cellSize.x, 1e-3f);
            Assert.AreEqual(TileUnits, grid.cellSize.y, 1e-3f);
        }

        [Test]
        public void SceneryPrefabs_ExistWithASprite()
        {
            foreach (var (_, name) in HandmadeSceneryPrefabs.Props)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HandmadeSceneryPrefabs.PathOf(name));
                Assert.IsNotNull(prefab, "scenery prefab " + name);
                Assert.IsNotNull(prefab.GetComponent<SpriteRenderer>().sprite, name + " has no sprite");
            }
        }

        [Test]
        public void TemplateScene_HasTheLayersWithTheirOrdersAndColliders()
        {
            var scene = EditorSceneManager.OpenScene(HandmadeTileSetup.TemplateScenePath, OpenSceneMode.Additive);
            try
            {
                var grid = FindGrid(scene);
                Assert.IsNotNull(grid, "no Grid in the template");
                Assert.AreEqual(TileUnits, grid.cellSize.x, 1e-3f);
                for (var i = 0; i < LayerNames.Length; i++)
                {
                    var layer = grid.transform.Find(LayerNames[i]);
                    Assert.IsNotNull(layer, "layer " + LayerNames[i]);
                    Assert.AreEqual(LayerOrders[i], layer.GetComponent<TilemapRenderer>().sortingOrder, LayerNames[i] + " order");
                    var solid = LayerNames[i] == "Building" || LayerNames[i] == "Hedge";
                    Assert.AreEqual(solid, layer.GetComponent<TilemapCollider2D>() != null, LayerNames[i] + " collider");
                    Assert.AreEqual(solid, layer.GetComponent<CompositeCollider2D>() != null, LayerNames[i] + " composite");
                    if (solid) Assert.AreEqual(RigidbodyType2D.Static, layer.GetComponent<Rigidbody2D>().bodyType, LayerNames[i] + " body");
                }
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }

        private static Grid FindGrid(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var grid = root.GetComponent<Grid>();
                if (grid != null) return grid;
            }
            return null;
        }

        private static string HandmadePaletteGroup(string tileName) => tileName.Substring(0, tileName.IndexOf('_'));
    }
}
