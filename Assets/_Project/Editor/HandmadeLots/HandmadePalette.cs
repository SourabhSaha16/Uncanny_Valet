using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>
    /// The Tile Palette prefab: a 4x4 unit grid, one row per name prefix (ground, marking, sidewalk, hedge, building, roof, overhead) with the tiles
    /// of that group left to right.
    /// </summary>
    internal static class HandmadePalette
    {
        /// <summary>Cell size in units: 88 pixels at 22 pixels per unit.</summary>
        public const float CellUnits = 4f;
        private const char GroupSeparator = '_';

        /// <summary>The groups (name prefixes) of the tiles, in order of first appearance; the palette has one row each.</summary>
        public static List<string> Groups(IReadOnlyList<Tile> tiles)
        {
            var groups = new List<string>();
            foreach (var tile in tiles)
            {
                var group = GroupOf(tile.name);
                if (!groups.Contains(group)) groups.Add(group);
            }
            return groups;
        }

        /// <summary>The name prefix before the first underscore ("sidewalk_corner_a" is in "sidewalk").</summary>
        public static string GroupOf(string tileName)
        {
            var at = tileName.IndexOf(GroupSeparator);
            return at < 0 ? tileName : tileName.Substring(0, at);
        }

        /// <summary>Creates the palette prefab when it is missing, then lays every tile out in it (an existing palette is cleared first).</summary>
        public static void Build(IReadOnlyList<Tile> tiles)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(HandmadeTileSetup.PalettePath) == null) Create();
            var root = PrefabUtility.LoadPrefabContents(HandmadeTileSetup.PalettePath);
            try
            {
                var map = root.GetComponentInChildren<Tilemap>();
                map.ClearAllTiles();
                var groups = Groups(tiles);
                var column = new int[groups.Count];
                foreach (var tile in tiles)
                {
                    var row = groups.IndexOf(GroupOf(tile.name));
                    map.SetTile(new Vector3Int(column[row]++, -row, 0), tile);
                }
                PrefabUtility.SaveAsPrefabAsset(root, HandmadeTileSetup.PalettePath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void Create()
        {
            GridPaletteUtility.CreateNewPalette(HandmadeTileSetup.PaletteFolder, HandmadeTileSetup.PaletteName, GridLayout.CellLayout.Rectangle,
                GridPalette.CellSizing.Manual, new Vector3(CellUnits, CellUnits, 0f), GridLayout.CellSwizzle.XYZ);
        }
    }
}
