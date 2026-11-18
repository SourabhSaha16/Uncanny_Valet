using UncannyValet.Core.Art;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// Makes prop recipes solid (cars bump off them like off the lot's walls) with a block fitted to the prop's footprint. Recipes created
    /// before a prop was made solid are patched in place, so a re-run of the setup always leaves them solid.
    /// </summary>
    internal static class PropSolidity
    {
        /// <summary>Marks the recipe at <paramref name="path"/> solid with a block of <paramref name="colliderSize"/> world units.</summary>
        public static void MakeSolid(string path, Vector2 colliderSize)
        {
            var recipe = AssetDatabase.LoadAssetAtPath<PropRecipe>(path);
            if (recipe == null) return;
            var so = new SerializedObject(recipe);
            so.FindProperty("_solid").boolValue = true;
            so.FindProperty("_colliderSize").vector2Value = colliderSize;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(recipe);
        }
    }
}
