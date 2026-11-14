using UncannyValet.Core.Art;
using UncannyValet.Lot;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.Art
{
    /// <summary>
    /// Creates the Midgard and Vanaheim palettes, prop recipes and lighting presets when missing, and points the two Midgard lots at their
    /// palette and lighting (day for the training diner, night for the fair) with the OPEN sign on the diner. A lot that already has a
    /// palette is left alone, so designer edits survive a rebuild. Placing props into the other lots belongs to their own tasks.
    /// </summary>
    public static class ArtAssetSetup
    {
        // The fair night lot is the taller one: its inside is where the string lights and lanterns hang.
        private const int FairQueueSlots = 4;
        private const float SignAboveDoor = 1.2f;
        private const float SignInsetFromFace = 1.2f;
        private const int SignOrder = 6;

        public static void CreateIfMissing()
        {
            var config = AssetDatabase.LoadAssetAtPath<LotConfig>(ProjectSetup.LotConfigPath);
            var geometry = new LotGeometry(config, FairQueueSlots);
            MidgardArt.CreateIfMissing(ArtAssets.UnlitMaterial(), geometry.Bounds);
            VanaheimArt.CreateIfMissing(geometry.Bounds);
            Wire(RunAssetSetup.MidgardTrainingPath, MidgardArt.DayPath, config);
            Wire(RunAssetSetup.MidgardCrazyPath, MidgardArt.NightPath, config);
            AssetDatabase.SaveAssets();
        }

        private static void Wire(string lotPath, string lightingPath, LotConfig config)
        {
            var lot = AssetDatabase.LoadAssetAtPath<LotDefinition>(lotPath);
            var so = new SerializedObject(lot);
            if (so.FindProperty("_palette").objectReferenceValue != null) return;
            so.FindProperty("_palette").objectReferenceValue = AssetDatabase.LoadAssetAtPath<WorldPalette>(MidgardArt.PalettePath);
            so.FindProperty("_lighting").objectReferenceValue = AssetDatabase.LoadAssetAtPath<LightingPreset>(lightingPath);
            var geometry = new LotGeometry(config, lot.QueueSlots);
            var position = new Vector2(geometry.Diner.xMax - SignInsetFromFace, geometry.MainDoor.yMax + SignAboveDoor);
            var props = so.FindProperty("_props");
            props.arraySize = 1;
            var entry = props.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("_recipe").objectReferenceValue = AssetDatabase.LoadAssetAtPath<PropRecipe>(MidgardArt.OpenSignPath);
            entry.FindPropertyRelative("_position").vector2Value = position;
            entry.FindPropertyRelative("_sortingOrder").intValue = SignOrder;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(lot);
        }
    }
}
