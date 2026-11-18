using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>One Tile asset per square tile sprite; the asset carries no collider (the Building and Hedge tilemaps provide it).</summary>
    internal static class HandmadeTileAssets
    {
        /// <summary>Creates or refreshes a Tile asset for every sprite in the tile folder, sorted by name. Returns them in that order.</summary>
        public static List<Tile> Build()
        {
            var tiles = new List<Tile>();
            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { HandmadeTileSetup.SpriteFolder });
            var paths = new List<string>();
            foreach (var guid in guids) paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            paths.Sort(string.CompareOrdinal);
            foreach (var path in paths)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null) tiles.Add(LoadOrCreate(sprite));
            }
            return tiles;
        }

        private static Tile LoadOrCreate(Sprite sprite)
        {
            var path = HandmadeTileSetup.TileFolder + "/" + sprite.name + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.None;
            tile.color = Color.white;
            EditorUtility.SetDirty(tile);
            return tile;
        }
    }
}
