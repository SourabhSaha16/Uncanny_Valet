using System.IO;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Watchers;
using UncannyValet.Run;
using UncannyValet.Watchers;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.Watchers
{
    /// <summary>
    /// Creates the watcher assets when missing (the config, and for each character a text asset and a definition) and points the Midgard lots at
    /// their watcher when they have none. Existing assets are never overwritten, so the owner's edits to lines, looks and spots survive a rebuild.
    /// </summary>
    public static class WatcherAssetSetup
    {
        public const string Folder = "Assets/_Project/Data/Watchers";
        public const string ConfigPath = Folder + "/WatcherConfig.asset";
        public const string MrsOkaforPath = Folder + "/MrsOkafor.asset";
        public const string GrandmaPath = Folder + "/Grandma.asset";
        public const string FreyjaPath = Folder + "/Freyja.asset";
        public const string FarmhandPath = Folder + "/VanirFarmhand.asset";

        /// <summary>Where every watcher stands (Midgard and Vanaheim: the diner or the barn door), in lot space: on the open ground beside the diner's side door, clear of the curb and the exit lane.</summary>
        public static readonly Vector2 MidgardSpot = new Vector2(-11f, 7.4f);

        public static void CreateIfMissing()
        {
            Directory.CreateDirectory(Folder);
            if (AssetDatabase.LoadAssetAtPath<WatcherConfig>(ConfigPath) == null)
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<WatcherConfig>(), ConfigPath);
            var okafor = Character(MrsOkaforPath, World.Midgard, WatcherKind.God, OkaforWatcher.Speaker, OkaforWatcher.Look(), OkaforWatcher.Text());
            var grandma = Character(GrandmaPath, World.Midgard, WatcherKind.Trainer, GrandmaWatcher.Speaker, GrandmaWatcher.Look(), GrandmaWatcher.Text());
            var freyja = Character(FreyjaPath, World.Vanaheim, WatcherKind.God, FreyjaWatcher.Speaker, FreyjaWatcher.Look(), FreyjaWatcher.Text());
            var farmhand = Character(FarmhandPath, World.Vanaheim, WatcherKind.Trainer, FarmhandWatcher.Speaker, FarmhandWatcher.Look(), FarmhandWatcher.Text());
            Assign(RunAssetSetup.MidgardCrazyPath, okafor);
            Assign(RunAssetSetup.MidgardTrainingPath, grandma);
            Assign(RunAssetSetup.VanaheimCrazyPath, freyja);
            Assign(RunAssetSetup.VanaheimTrainingPath, farmhand);
        }

        // Creates the text next to the definition and the definition itself, once.
        private static WatcherDefinition Character(string path, World world, WatcherKind kind, string speaker, WatcherLook look, WatcherTextAsset text)
        {
            var existing = AssetDatabase.LoadAssetAtPath<WatcherDefinition>(path);
            if (existing != null) return existing;
            var textPath = path.Replace(".asset", "Text.asset");
            AssetDatabase.CreateAsset(text, textPath);
            var definition = WatcherDefinition.Create(Path.GetFileNameWithoutExtension(path), world, kind, speaker, look, text);
            AssetDatabase.CreateAsset(definition, path);
            return definition;
        }

        // Names the watcher in a lot that has none yet, with the shared spot beside the building door.
        private static void Assign(string lotPath, WatcherDefinition watcher)
        {
            var lot = AssetDatabase.LoadAssetAtPath<LotDefinition>(lotPath);
            if (lot == null || lot.Watcher != null) return;
            var so = new SerializedObject(lot);
            so.FindProperty("_watcher").objectReferenceValue = watcher;
            so.FindProperty("_watcherSpot").vector2Value = MidgardSpot;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(lot);
        }
    }
}
