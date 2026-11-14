using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UncannyValet.Run;
using UnityEditor;

namespace UncannyValet.EditorTools
{
    /// <summary>Hands what a lot definition's mechanics add to the world (its <see cref="LotWorldSpec"/>) to the lot builder of a generated scene.</summary>
    public static class LotWorldSceneSetup
    {
        /// <summary>Writes the mud patch list of the definition's modifiers into the lot builder; an empty list clears it.</summary>
        public static void Apply(LotLayoutBuilder lot, LotDefinition definition)
        {
            var patches = definition.BuildWorldSpec().MudPatches;
            var so = new SerializedObject(lot);
            var list = so.FindProperty("_mudPatches");
            list.arraySize = patches.Count;
            for (var i = 0; i < patches.Count; i++)
            {
                var entry = list.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("_center").vector2Value = patches[i].Center;
                entry.FindPropertyRelative("_size").floatValue = patches[i].Size;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
