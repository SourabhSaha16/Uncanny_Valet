using System.Collections.Generic;
using UncannyValet.Core.Art;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Modifiers;
using UncannyValet.Core.Watchers;
using UncannyValet.EditorTools.Art;
using UncannyValet.EditorTools.Watchers;
using UncannyValet.Lot;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// Fills in the two Vanaheim lots (task 107) once: closed services plus the wash troughs (one beside the bay block in Freyr's Harvest Barn,
    /// three at the far end of the widened storm feast lot), the Muddy shares, the crowd, the barn sign and the dirt ground, the props and the
    /// lantern lights, the mud patches of the crazy lot and the gag lines. A lot that already has gag lines is left alone, so the owner's edits
    /// survive a rebuild.
    /// </summary>
    public static class VanaheimWorldSetup
    {
        public const string TrainingGagPath = WatcherAssetSetup.Folder + "/VanaheimTrainingGagLines.asset";
        public const string CrazyGagPath = WatcherAssetSetup.Folder + "/VanaheimCrazyGagLines.asset";
        public const string MudPath = "Assets/_Project/Data/Run/VanaheimMudPatches.asset";

        private const int TrainingPedestrians = 4;
        private const int CrazyPedestrians = 8;
        private const float TrainingMuddy = 0.3f;
        private const float CrazyMuddy = 0.6f;
        // The lot is widened by this much; the troughs stand this far from the bay block (14), and the strip keeps clear of the last trough.
        private const float CrazyExtension = 13.5f;
        private const float CrazyTroughDistance = 14f;
        private const string BuildingSign = "BARN";
        private static readonly Color TrainingGround = new Color(0.63f, 0.48f, 0.29f);
        private static readonly Color CrazyGround = new Color(0.4f, 0.3f, 0.21f);
        private static readonly string[] DecorPaths = { VanaheimArt.HayPath, VanaheimArt.FencePath, VanaheimArt.StallPath, VanaheimArt.BannerPath };

        public static void CreateIfMissing()
        {
            var config = AssetDatabase.LoadAssetAtPath<LotConfig>(ProjectSetup.LotConfigPath);
            SoftenDecor();
            Fill(RunAssetSetup.VanaheimTrainingPath, TrainingGagPath, VanaheimGagLines.TrainingAsset, config, false);
            Fill(RunAssetSetup.VanaheimCrazyPath, CrazyGagPath, VanaheimGagLines.CrazyAsset, config, true);
            AssetDatabase.SaveAssets();
        }

        private static void Fill(string lotPath, string gagPath, System.Func<GagLineAsset> makeGag, LotConfig config, bool crazy)
        {
            var lot = AssetDatabase.LoadAssetAtPath<LotDefinition>(lotPath);
            if (lot == null || lot.GagLines != null) return;
            var gag = AssetDatabase.LoadAssetAtPath<GagLineAsset>(gagPath);
            if (gag == null)
            {
                AssetDatabase.CreateAsset(makeGag(), gagPath);
                gag = AssetDatabase.LoadAssetAtPath<GagLineAsset>(gagPath);
            }
            var options = crazy ? new LotLayoutOptions(true, 0f, 3, CrazyExtension, CrazyTroughDistance) : new LotLayoutOptions(true, 0f, 1);
            var geometry = new LotGeometry(config, lot.QueueSlots, options);
            var so = new SerializedObject(lot);
            so.FindProperty("_gagLines").objectReferenceValue = gag;
            SetOptions(so, options);
            so.FindProperty("_ambientTraffic").boolValue = true;
            so.FindProperty("_ambientPedestrians").intValue = crazy ? CrazyPedestrians : TrainingPedestrians;
            so.FindProperty("_pedestrianCurses").boolValue = false;
            so.FindProperty("_buildingSign").stringValue = BuildingSign;
            so.FindProperty("_groundColour").colorValue = crazy ? CrazyGround : TrainingGround;
            so.FindProperty("_palette").objectReferenceValue = AssetDatabase.LoadAssetAtPath<WorldPalette>(VanaheimArt.PalettePath);
            so.FindProperty("_lighting").objectReferenceValue = AssetDatabase.LoadAssetAtPath<LightingPreset>(crazy ? VanaheimArt.StormyPath : VanaheimArt.SunnyPath);
            SetMuddyShare(so, crazy ? CrazyMuddy : TrainingMuddy);
            so.FindProperty("_props").arraySize = 0;
            MidgardWorldSetup.AddProps(so, crazy ? VanaheimProps.Crazy(geometry) : VanaheimProps.Training(geometry));
            if (crazy) AddMud(so, geometry);
            so.ApplyModifiedPropertiesWithoutUndo();
            if (crazy) VanaheimLanterns.Hang(geometry);
            EditorUtility.SetDirty(lot);
        }

        private static void SetOptions(SerializedObject so, LotLayoutOptions options)
        {
            so.FindProperty("_layoutOptions._servicesClosed").boolValue = options.ServicesClosed;
            so.FindProperty("_layoutOptions._cornerClearance").floatValue = options.CornerClearance;
            so.FindProperty("_layoutOptions._washTroughs").intValue = options.WashTroughs;
            so.FindProperty("_layoutOptions._extensionWidth").floatValue = options.ExtensionWidth;
            so.FindProperty("_layoutOptions._troughDistance").floatValue = options.TroughDistance;
        }

        // No EV cars anywhere in the story (their chance is 0; the entry stays listed); the Muddy share is the lot's.
        private static void SetMuddyShare(SerializedObject so, float muddy)
        {
            var list = so.FindProperty("_modifierChances");
            for (var i = 0; i < list.arraySize; i++)
            {
                var entry = list.GetArrayElementAtIndex(i);
                var modifier = (ServiceModifierDefinition)entry.FindPropertyRelative("_modifier").objectReferenceValue;
                entry.FindPropertyRelative("_chance").floatValue = modifier.Id == ModifierId.Muddy ? muddy : 0f;
            }
        }

        private static void AddMud(SerializedObject so, LotGeometry g)
        {
            var mud = AssetDatabase.LoadAssetAtPath<MudPatchModifier>(MudPath);
            if (mud == null)
            {
                AssetDatabase.CreateAsset(VanaheimMud.Create(g), MudPath);
                mud = AssetDatabase.LoadAssetAtPath<MudPatchModifier>(MudPath);
            }
            var list = so.FindProperty("_modifiers");
            list.arraySize = 1;
            list.GetArrayElementAtIndex(0).objectReferenceValue = mud;
        }

        // Hay bales, fences, stalls and banners are decor (only the barn is solid; the trough is a wash slot a car drives into).
        private static void SoftenDecor()
        {
            foreach (var path in DecorPaths)
            {
                var recipe = AssetDatabase.LoadAssetAtPath<PropRecipe>(path);
                if (recipe == null || !recipe.Solid) continue;
                var so = new SerializedObject(recipe);
                so.FindProperty("_solid").boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(recipe);
            }
        }
    }
}
