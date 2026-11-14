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

        /// <summary>Hands the definition's world palette, props and lighting preset to the lot builder, and the palette to the decor (the diner re-skin); empty references clear them.</summary>
        public static void ApplyArt(LotLayoutBuilder lot, LotDecor decor, LotDefinition definition)
        {
            var so = new SerializedObject(lot);
            so.FindProperty("_palette").objectReferenceValue = definition.Palette;
            so.FindProperty("_lighting").objectReferenceValue = definition.Lighting;
            var props = so.FindProperty("_props");
            props.arraySize = definition.Props.Count;
            for (var i = 0; i < definition.Props.Count; i++)
            {
                var entry = props.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("_recipe").objectReferenceValue = definition.Props[i].Recipe;
                entry.FindPropertyRelative("_position").vector2Value = definition.Props[i].Position;
                entry.FindPropertyRelative("_sortingOrder").intValue = definition.Props[i].SortingOrder;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            var decorSo = new SerializedObject(decor);
            decorSo.FindProperty("_palette").objectReferenceValue = definition.Palette;
            decorSo.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
