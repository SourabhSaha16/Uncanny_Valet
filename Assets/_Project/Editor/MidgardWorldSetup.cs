using System.Collections.Generic;
using UncannyValet.Core.Art;
using UncannyValet.Core.Contracts;
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
    /// Fills in the two Midgard lots (task 106) once: no services and eight regular bays, the crowd and traffic settings, the gag lines, the trees
    /// of the training lot, the stalls and the Ferris wheel of the fair, and the wheel's warm light. A lot that already has gag lines is left alone,
    /// so the owner's edits survive a rebuild.
    /// </summary>
    public static class MidgardWorldSetup
    {
        public const string TrainingGagPath = WatcherAssetSetup.Folder + "/MidgardTrainingGagLines.asset";
        public const string CrazyGagPath = WatcherAssetSetup.Folder + "/MidgardCrazyGagLines.asset";
        public const string PedestrianConfigPath = "Assets/_Project/Data/Pedestrians/PedestrianConfig.asset";

        private const int TrainingPedestrians = 3;
        private const int CrazyPedestrians = 10;
        private const int TreeCount = 4;
        private const float TreeSpacing = 3.4f;
        private const float StallSpacing = 3.2f;
        private const float StripInset = 2.2f;
        private const float FerrisX = 12f;
        private const float FerrisFromBottom = 3.2f;
        private const float FerrisClearance = 6f;
        private const int FerrisOrder = 5;
        private const int WheelLight = 14; // the string light of the bottom row that sits under the wheel
        private static readonly string[] StallPaths = MidgardArt.StallPaths;

        public static void CreateIfMissing()
        {
            var config = AssetDatabase.LoadAssetAtPath<LotConfig>(ProjectSetup.LotConfigPath);
            SoftenStalls();
            SetCrowdPool();
            Fill(RunAssetSetup.MidgardTrainingPath, TrainingGagPath, MidgardGagLines.TrainingAsset, config, false);
            Fill(RunAssetSetup.MidgardCrazyPath, CrazyGagPath, MidgardGagLines.CrazyAsset, config, true);
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
            var options = new LotLayoutOptions(true, crazy ? FerrisClearance : 0f);
            var geometry = new LotGeometry(config, lot.QueueSlots, options);
            var so = new SerializedObject(lot);
            so.FindProperty("_gagLines").objectReferenceValue = gag;
            so.FindProperty("_layoutOptions._servicesClosed").boolValue = true;
            so.FindProperty("_layoutOptions._cornerClearance").floatValue = options.CornerClearance;
            so.FindProperty("_ambientTraffic").boolValue = crazy;
            so.FindProperty("_ambientPedestrians").intValue = crazy ? CrazyPedestrians : TrainingPedestrians;
            ZeroServiceChances(so);
            AddProps(so, crazy ? Stalls(geometry) : Trees(geometry));
            if (crazy) AddWheel(so, geometry);
            so.ApplyModifiedPropertiesWithoutUndo();
            if (crazy) WarmTheWheel(geometry);
            EditorUtility.SetDirty(lot);
        }

        // No services in Midgard: every car is a plain car (the entries stay listed, with chance 0).
        private static void ZeroServiceChances(SerializedObject so)
        {
            var list = so.FindProperty("_modifierChances");
            for (var i = 0; i < list.arraySize; i++) list.GetArrayElementAtIndex(i).FindPropertyRelative("_chance").floatValue = 0f;
        }

        private static List<PropPlacement> Trees(LotGeometry g)
        {
            var tree = AssetDatabase.LoadAssetAtPath<PropRecipe>(MidgardArt.TreePath);
            var spots = new[] { g.Strip.yMax - StripInset, g.Strip.yMax - StripInset - TreeSpacing, g.AisleY - TreeSpacing, g.AisleY - 2f * TreeSpacing };
            var list = new List<PropPlacement>();
            for (var i = 0; i < TreeCount; i++) list.Add(new PropPlacement(tree, new Vector2(g.Strip.center.x, spots[i])));
            return list;
        }

        // Stalls line the right sidewalk strip above and below the crossing of the aisle; people walk in front of them.
        private static List<PropPlacement> Stalls(LotGeometry g)
        {
            var list = new List<PropPlacement>();
            var ys = new List<float>();
            for (var i = 0; i < 3; i++) ys.Add(g.Strip.yMax - StripInset + 0.8f - i * StallSpacing);
            for (var i = 1; i <= 3; i++) ys.Add(g.AisleY - i * StallSpacing);
            for (var i = 0; i < ys.Count; i++)
                list.Add(new PropPlacement(AssetDatabase.LoadAssetAtPath<PropRecipe>(StallPaths[i % StallPaths.Length]), new Vector2(g.Strip.center.x, ys[i])));
            return list;
        }

        private static void AddWheel(SerializedObject so, LotGeometry g)
        {
            var ferris = AssetDatabase.LoadAssetAtPath<PropRecipe>(MidgardArt.FerrisPath);
            AddProps(so, new List<PropPlacement> { new PropPlacement(ferris, new Vector2(FerrisX, g.Bounds.yMin + FerrisFromBottom), FerrisOrder) });
        }

        private static void AddProps(SerializedObject so, List<PropPlacement> placements)
        {
            var props = so.FindProperty("_props");
            var first = props.arraySize;
            props.arraySize = first + placements.Count;
            for (var i = 0; i < placements.Count; i++)
            {
                var entry = props.GetArrayElementAtIndex(first + i);
                entry.FindPropertyRelative("_recipe").objectReferenceValue = placements[i].Recipe;
                entry.FindPropertyRelative("_position").vector2Value = placements[i].Position;
                entry.FindPropertyRelative("_sortingOrder").intValue = placements[i].SortingOrder;
            }
        }

        // The night preset is at its 16-light cap: the string light that hangs under the wheel becomes a stronger warm light on the wheel.
        private static void WarmTheWheel(LotGeometry g)
        {
            var night = AssetDatabase.LoadAssetAtPath<LightingPreset>(MidgardArt.NightPath);
            if (night == null) return;
            var so = new SerializedObject(night);
            var light = so.FindProperty("_lights").GetArrayElementAtIndex(WheelLight);
            light.FindPropertyRelative("_position").vector2Value = new Vector2(FerrisX, g.Bounds.yMin + FerrisFromBottom + 0.5f);
            light.FindPropertyRelative("_radius").floatValue = 6f;
            light.FindPropertyRelative("_colour").colorValue = new Color(1f, 0.72f, 0.31f);
            light.FindPropertyRelative("_intensity").floatValue = 1.1f;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(night);
        }

        // Stalls are decor: only the diner and the wheel are solid.
        private static void SoftenStalls()
        {
            foreach (var path in StallPaths)
            {
                var stall = AssetDatabase.LoadAssetAtPath<PropRecipe>(path);
                if (stall == null || !stall.Solid) continue;
                var so = new SerializedObject(stall);
                so.FindProperty("_solid").boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(stall);
            }
        }

        // The crowd pool holds the biggest crowd any lot asks for; each lot then activates its own number of them.
        private static void SetCrowdPool()
        {
            var config = AssetDatabase.LoadAssetAtPath<UncannyValet.Pedestrians.PedestrianConfig>(PedestrianConfigPath);
            if (config == null || config.Count >= LotDefinition.MaxAmbientPedestrians) return;
            var so = new SerializedObject(config);
            so.FindProperty("_count").intValue = LotDefinition.MaxAmbientPedestrians;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }
    }
}
