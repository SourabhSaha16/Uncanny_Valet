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

        // Scene 0 stays Bootstrap.unity (the Midgard training lot) so existing tests and the web build entry keep working.
        private const string TrainingScene = "Bootstrap";
        private const string CrazyScene = "Lot_MidgardCrazy";
        // Placeholder blessing thresholds until the blessing task tunes them.
        private const int TrainingThreshold = 3;
        private const int CrazyThreshold = 5;

        public static void CreateIfMissing()
        {
            var training = CreateLot(MidgardTrainingPath, new LotId(World.Midgard, LotKind.Training), "Grandma's Diner",
                TrainingScene, TrainingThreshold, 150f, 15f, 25f, 2, StoryPower.None);
            var crazy = CreateLot(MidgardCrazyPath, new LotId(World.Midgard, LotKind.Crazy), "Fair Night at Grandma's",
                CrazyScene, CrazyThreshold, 240f, 10f, 15f, 4, StoryPower.ThunderStride);
            if (AssetDatabase.LoadAssetAtPath<RunDefinition>(RunPath) != null) return;
            Directory.CreateDirectory(Path.GetDirectoryName(RunPath));
            var run = ScriptableObject.CreateInstance<RunDefinition>();
            AssetDatabase.CreateAsset(run, RunPath);
            var so = new SerializedObject(run);
            var lots = so.FindProperty("_lots");
            lots.arraySize = 2;
            lots.GetArrayElementAtIndex(0).objectReferenceValue = training;
            lots.GetArrayElementAtIndex(1).objectReferenceValue = crazy;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static LotDefinition CreateLot(string path, LotId id, string displayName, string scene, int threshold,
            float shiftSeconds, float gapMin, float gapMax, int maxWaiting, StoryPower grant)
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
            so.FindProperty("_pacing._shiftLengthSeconds").floatValue = shiftSeconds;
            so.FindProperty("_pacing._arrivalGapMinSeconds").floatValue = gapMin;
            so.FindProperty("_pacing._arrivalGapMaxSeconds").floatValue = gapMax;
            so.FindProperty("_pacing._maxWaitingCars").intValue = maxWaiting;
            so.FindProperty("_grant._power").intValue = (int)grant;
            so.FindProperty("_grant._level").intValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
            return lot;
        }
    }
}
