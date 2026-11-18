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
    /// Fills in the two Vanaheim lots (task 107) once: closed services plus the wash bays (one right beside the bay block in Freyr's Harvest Barn,
    /// three in a column right beside it in the storm feast lot), the Muddy shares, the crowd, the barn sign and the dirt ground, the props and the
    /// lantern lights, the mud patches of the crazy lot and the gag lines. A lot that already has gag lines keeps the owner's edits; only what follows
    /// the wash bays (layout options, props, mud, lanterns) is rebuilt each time.
    /// </summary>
    public static class VanaheimWorldSetup
    {
        public const string TrainingGagPath = WatcherAssetSetup.Folder + "/VanaheimTrainingGagLines.asset";
        public const string CrazyGagPath = WatcherAssetSetup.Folder + "/VanaheimCrazyGagLines.asset";
        public const string MudPath = "Assets/_Project/Data/Run/VanaheimMudPatches.asset";
        public const string WheatMudPath = "Assets/_Project/Data/Run/VanaheimWheatMud.asset";

        private const int TrainingPedestrians = 4;
        private const int CrazyPedestrians = 8;
        private const float TrainingMuddy = 0.3f;
        private const float CrazyMuddy = 0.6f;
        // The wash bays stand this far from the bay block, right beside it; the strip keeps clear of them (the lot is not widened).
        private const float WashBayGap = 0.5f;
        private const string BuildingSign = "BARN";
        private static readonly Color TrainingGround = new Color(0.63f, 0.48f, 0.29f);
        private static readonly Color CrazyGround = new Color(0.4f, 0.3f, 0.21f);
        private static readonly Vector2 HaySize = new Vector2(0.9f, 0.7f);
        private static readonly Vector2 FenceSize = new Vector2(2f, 0.5f);
        private static readonly Vector2 StallSize = new Vector2(2.7f, 1.7f);
        private static readonly Vector2 BannerSize = new Vector2(0.8f, 1.8f);

        public static void CreateIfMissing()
        {
            var config = AssetDatabase.LoadAssetAtPath<LotConfig>(ProjectSetup.LotConfigPath);
            SolidifyDecor();
            Fill(RunAssetSetup.VanaheimTrainingPath, TrainingGagPath, VanaheimGagLines.TrainingAsset, config, false);
            Fill(RunAssetSetup.VanaheimCrazyPath, CrazyGagPath, VanaheimGagLines.CrazyAsset, config, true);
            AssetDatabase.SaveAssets();
        }

        private static void Fill(string lotPath, string gagPath, System.Func<GagLineAsset> makeGag, LotConfig config, bool crazy)
        {
            var lot = AssetDatabase.LoadAssetAtPath<LotDefinition>(lotPath);
            if (lot == null) return;
            if (lot.GagLines == null) FillOnce(lot, gagPath, makeGag, crazy);
            ApplyLayout(lot, config, crazy);
        }

        private static void FillOnce(LotDefinition lot, string gagPath, System.Func<GagLineAsset> makeGag, bool crazy)
        {
            var gag = AssetDatabase.LoadAssetAtPath<GagLineAsset>(gagPath);
            if (gag == null)
            {
                AssetDatabase.CreateAsset(makeGag(), gagPath);
                gag = AssetDatabase.LoadAssetAtPath<GagLineAsset>(gagPath);
            }
            var so = new SerializedObject(lot);
            so.FindProperty("_gagLines").objectReferenceValue = gag;
            so.FindProperty("_ambientTraffic").boolValue = true;
            so.FindProperty("_ambientPedestrians").intValue = crazy ? CrazyPedestrians : TrainingPedestrians;
            so.FindProperty("_pedestrianCurses").boolValue = false;
            so.FindProperty("_freePedestrianRoutes").boolValue = crazy;
            so.FindProperty("_buildingSign").stringValue = BuildingSign;
            so.FindProperty("_groundColour").colorValue = crazy ? CrazyGround : TrainingGround;
            so.FindProperty("_palette").objectReferenceValue = AssetDatabase.LoadAssetAtPath<WorldPalette>(VanaheimArt.PalettePath);
            so.FindProperty("_lighting").objectReferenceValue = AssetDatabase.LoadAssetAtPath<LightingPreset>(crazy ? VanaheimArt.StormyPath : VanaheimArt.SunnyPath);
            SetMuddyShare(so, crazy ? CrazyMuddy : TrainingMuddy);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(lot);
        }

        // Everything that follows the wash bays (layout options, props, mud, lanterns) is rebuilt from the geometry on every run.
        private static void ApplyLayout(LotDefinition lot, LotConfig config, bool crazy)
        {
            var options = new LotLayoutOptions(true, 0f, crazy ? 3 : 1, 0f, WashBayGap);
            var geometry = new LotGeometry(config, lot.QueueSlots, options);
            var so = new SerializedObject(lot);
            SetOptions(so, options);
            so.FindProperty("_props").arraySize = 0;
            MidgardWorldSetup.AddProps(so, crazy ? VanaheimProps.Crazy(geometry) : VanaheimProps.Training(geometry));
            AddMud(so, crazy ? MudPath : WheatMudPath, crazy ? VanaheimMud.Create(geometry) : VanaheimMud.CreateWheat(geometry));
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
            so.FindProperty("_layoutOptions._washBayGap").floatValue = options.WashBayGap;
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

        private static void AddMud(SerializedObject so, string path, MudPatchModifier made)
        {
            AssetDatabase.DeleteAsset(path); // the patches follow the wash bays and the props, so they are rebuilt from the geometry every time
            AssetDatabase.CreateAsset(made, path);
            var mud = AssetDatabase.LoadAssetAtPath<MudPatchModifier>(path);
            var list = so.FindProperty("_modifiers");
            list.arraySize = 1;
            list.GetArrayElementAtIndex(0).objectReferenceValue = mud;
        }

        // Hay bales, fences, stalls and banners are solid like the barn: cars bump off them. Each block is fitted to the sprite's footprint.
        private static void SolidifyDecor()
        {
            PropSolidity.MakeSolid(VanaheimArt.HayPath, HaySize);
            PropSolidity.MakeSolid(VanaheimArt.FencePath, FenceSize);
            PropSolidity.MakeSolid(VanaheimArt.StallPath, StallSize);
            PropSolidity.MakeSolid(VanaheimArt.BannerPath, BannerSize);
        }
    }
}
