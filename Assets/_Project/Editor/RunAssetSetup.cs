using System.IO;
using UncannyValet.Core.Ids;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// Creates the run assets when missing: one LotDefinition per lot and the RunDefinition that lists them in
    /// play order. Existing assets are never overwritten, so a designer's edits survive a rebuild.
    /// </summary>
    public static class RunAssetSetup
    {
        public const string RunPath = "Assets/_Project/Data/Run/RunDefinition.asset";
        public const string MidgardTrainingPath = "Assets/_Project/Data/Run/MidgardTrainingLot.asset";
        public const string MidgardCrazyPath = "Assets/_Project/Data/Run/MidgardCrazyLot.asset";

        public const string VanaheimTrainingPath = "Assets/_Project/Data/Run/VanaheimTrainingLot.asset";
        public const string VanaheimCrazyPath = "Assets/_Project/Data/Run/VanaheimCrazyLot.asset";

        private const string VanaheimTrainingScene = "Lot_VanaheimTraining";
        private const string VanaheimCrazyScene = "Lot_VanaheimCrazy";
        // Scene 0 stays Bootstrap.unity (the Midgard training lot) so existing tests and the web build entry keep working.
        private const string TrainingScene = "Bootstrap";
        private const string CrazyScene = "Lot_MidgardCrazy";
        // Placeholder blessing thresholds (the old one- and two-star scores) until the owner tunes them.
        private const int TrainingThreshold = 150;
        private const int CrazyThreshold = 300;
        // Slots of the arrival queue: the crazy lot is a little taller so four cars can really wait.
        private const int TrainingQueueSlots = 3;
        private const int CrazyQueueSlots = 4;

        public static void CreateIfMissing()
        {
            var training = CreateLot(MidgardTrainingPath, new LotId(World.Midgard, LotKind.Training), "Grandma's Diner",
                TrainingScene, TrainingThreshold, 150f, 15f, 25f, 2, TrainingQueueSlots, StoryPower.None, false);
            var crazy = CreateLot(MidgardCrazyPath, new LotId(World.Midgard, LotKind.Crazy), "Fair Night at Grandma's",
                CrazyScene, CrazyThreshold, 240f, 10f, 15f, 4, CrazyQueueSlots, StoryPower.ThunderStride, true);
            var vanaheimTraining = CreateLot(VanaheimTrainingPath, new LotId(World.Vanaheim, LotKind.Training), "Freyr's Harvest Barn",
                VanaheimTrainingScene, TrainingThreshold, 150f, 15f, 25f, 2, TrainingQueueSlots, StoryPower.None, false);
            var vanaheimCrazy = CreateLot(VanaheimCrazyPath, new LotId(World.Vanaheim, LotKind.Crazy), "Storm-Soaked Harvest Feast",
                VanaheimCrazyScene, CrazyThreshold, 240f, 10f, 15f, 4, CrazyQueueSlots, StoryPower.StormWind, false);
            ModifierAssetSetup.EnsureChances(training);
            ModifierAssetSetup.EnsureChances(crazy);
            ModifierAssetSetup.EnsureChances(vanaheimTraining);
            ModifierAssetSetup.EnsureChances(vanaheimCrazy);
            var run = AssetDatabase.LoadAssetAtPath<RunDefinition>(RunPath);
            if (run == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(RunPath));
                run = ScriptableObject.CreateInstance<RunDefinition>();
                AssetDatabase.CreateAsset(run, RunPath);
            }
            AppendMissing(run, training, crazy, vanaheimTraining, vanaheimCrazy);
        }

        // Each world task appends its lots in play order; a lot the run already lists keeps its place.
        private static void AppendMissing(RunDefinition run, params LotDefinition[] lots)
        {
            var so = new SerializedObject(run);
            var list = so.FindProperty("_lots");
            foreach (var lot in lots)
            {
                if (run.IndexOf(lot.Id) >= 0) continue;
                list.arraySize++;
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = lot;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(run);
        }

        private static LotDefinition CreateLot(string path, LotId id, string displayName, string scene, int threshold,
            float shiftSeconds, float gapMin, float gapMax, int maxWaiting, int queueSlots, StoryPower grant, bool pedestrianCurses)
        {
            var existing = AssetDatabase.LoadAssetAtPath<LotDefinition>(path);
            if (existing != null) return existing;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var lot = ScriptableObject.CreateInstance<LotDefinition>();
            AssetDatabase.CreateAsset(lot, path);
            var so = new SerializedObject(lot);
            so.FindProperty("_id._world").intValue = (int)id.World;
            so.FindProperty("_id._kind").intValue = (int)id.Kind;
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_sceneName").stringValue = scene;
            so.FindProperty("_blessingThreshold").intValue = threshold;
            so.FindProperty("_queueSlots").intValue = queueSlots;
            so.FindProperty("_pacing._shiftLengthSeconds").floatValue = shiftSeconds;
            so.FindProperty("_pacing._arrivalGapMinSeconds").floatValue = gapMin;
            so.FindProperty("_pacing._arrivalGapMaxSeconds").floatValue = gapMax;
            so.FindProperty("_pacing._maxWaitingCars").intValue = maxWaiting;
            so.FindProperty("_pedestrianCurses").boolValue = pedestrianCurses;
            so.FindProperty("_grant._power").intValue = (int)grant;
            so.FindProperty("_grant._level").intValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
            return lot;
        }
    }
}
