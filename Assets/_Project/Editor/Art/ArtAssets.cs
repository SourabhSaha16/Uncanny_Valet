using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.Art
{
    /// <summary>Creates the world art assets (palettes, recipes, presets) when missing; existing assets are never overwritten, so a designer's edits survive a rebuild.</summary>
    internal static class ArtAssets
    {
        public const string Root = "Assets/_Project/Data/Art/";

        /// <summary>The asset at <paramref name="path"/>, made by <paramref name="make"/> and saved there when it does not exist yet.</summary>
        public static T Ensure<T>(string path, Func<T> make) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var asset = make();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        // The Sprite-Unlit-Default material of the URP package, the one the project's 2D renderer lists as its default unlit material.
        private const string UnlitMaterialGuid = "9dfc825aed78fcd4ba02077103263b40";

        /// <summary>The shared unlit 2D sprite material (for glowing layers); null when the package does not have it.</summary>
        public static Material UnlitMaterial()
            => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(UnlitMaterialGuid));
    }
}
