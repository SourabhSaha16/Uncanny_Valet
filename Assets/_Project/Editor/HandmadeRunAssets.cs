using System.IO;
using UncannyValet.EditorTools.HandmadeLots;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// Creates the DEV hand-made run assets when missing (#194): a lot definition that copies the real Midgard Training lot and swaps in the hand-made test prefab
    /// (same LotId, which is unique inside the DEV run because it replaces the real one there), and a run that lists it in the real lot's place. The real run is not touched.
    /// </summary>
    public static class HandmadeRunAssets
    {
        private const string DisplayName = "Grandma's Diner (hand-made)";
        private const int QueueSlots = 2;

        /// <summary>Creates the lot and the run; needs the real run and <see cref="HandmadeTestLot.PrefabPath"/> to exist.</summary>
        public static void CreateIfMissing()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HandmadeTestLot.PrefabPath);
            var real = AssetDatabase.LoadAssetAtPath<LotDefinition>(RunAssetSetup.MidgardTrainingPath);
            var run = AssetDatabase.LoadAssetAtPath<RunDefinition>(RunAssetSetup.RunPath);
            if (prefab == null || real == null || run == null)
            {
                Debug.LogError("The hand-made DEV run needs the test lot prefab, the Midgard training lot and the run: run Setup/Build Everything.");
                return;
            }
            var lot = CreateLot(real, prefab);
            CreateRun(run, real, lot);
        }

        /// <summary>Builds the DEV scene of the hand-made lot (Lot_MidgardHandmade.unity) when the DEV run exists. It is not added to Build Settings.</summary>
        public static void BuildDevScene()
        {
            if (AssetDatabase.LoadAssetAtPath<RunDefinition>(RunAssetSetup.DevHandmadeRunPath) == null) return;
            BootstrapSceneBuilder.BuildLot(RunAssetSetup.DevHandmadeRunPath, 0, BootstrapSceneBuilder.SceneFolder + RunAssetSetup.MidgardHandmadeScene + ".unity");
        }

        private static LotDefinition CreateLot(LotDefinition real, GameObject prefab)
        {
            var existing = AssetDatabase.LoadAssetAtPath<LotDefinition>(RunAssetSetup.MidgardHandmadePath);
            if (existing != null) return existing;
            var lot = ScriptableObject.CreateInstance<LotDefinition>();
            EditorUtility.CopySerialized(real, lot);
            Directory.CreateDirectory(Path.GetDirectoryName(RunAssetSetup.MidgardHandmadePath));
            AssetDatabase.CreateAsset(lot, RunAssetSetup.MidgardHandmadePath);
            var so = new SerializedObject(lot);
            so.FindProperty("_displayName").stringValue = DisplayName;
            so.FindProperty("_sceneName").stringValue = RunAssetSetup.MidgardHandmadeScene;
            so.FindProperty("_layout").intValue = (int)LotLayoutKind.Handmade;
            so.FindProperty("_handmadePrefab").objectReferenceValue = prefab;
            so.FindProperty("_queueSlots").intValue = QueueSlots;
            so.FindProperty("_props").arraySize = 0;
            so.FindProperty("_modifierChances").arraySize = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
            return lot;
        }

        private static void CreateRun(RunDefinition real, LotDefinition realTraining, LotDefinition handmade)
        {
            if (AssetDatabase.LoadAssetAtPath<RunDefinition>(RunAssetSetup.DevHandmadeRunPath) != null) return;
            var dev = ScriptableObject.CreateInstance<RunDefinition>();
            AssetDatabase.CreateAsset(dev, RunAssetSetup.DevHandmadeRunPath);
            var so = new SerializedObject(dev);
            var list = so.FindProperty("_lots");
            list.arraySize = real.Lots.Count;
            for (var i = 0; i < real.Lots.Count; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = real.Lots[i] == realTraining ? handmade : real.Lots[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
