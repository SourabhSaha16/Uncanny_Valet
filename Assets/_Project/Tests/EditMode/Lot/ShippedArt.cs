using System.Collections.Generic;
using UncannyValet.Core.Art;
using UnityEditor;

namespace UncannyValet.Tests.Lot
{
    /// <summary>Finds the shipped palettes, prop recipes and lighting presets under Data/Art.</summary>
    public static class ShippedArt
    {
        private static readonly string[] Folders = { "Assets/_Project/Data/Art" };

        public static List<T> All<T>() where T : UnityEngine.Object
        {
            var found = new List<T>();
            foreach (var guid in AssetDatabase.FindAssets("t:" + typeof(T).Name, Folders))
                found.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
            return found;
        }

        public static T Named<T>(string name) where T : UnityEngine.Object => All<T>().Find(asset => asset.name == name);

        /// <summary>The palette a recipe is drawn with in these tests: its own world's, by name prefix.</summary>
        public static WorldPalette PaletteFor(PropRecipe recipe)
            => Named<WorldPalette>(recipe.name.StartsWith("Vanaheim") ? "VanaheimPalette" : "MidgardPalette");
    }
}
