using System.IO;
using UncannyValet.Audio;
using UncannyValet.Cars;
using UncannyValet.Core.Config;
using UncannyValet.Director;
using UncannyValet.Lot;
using UncannyValet.Patrons;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// Creates the missing config assets and the Bootstrap scene. Menu: Uncanny Valet/Setup.
    /// Batch: -executeMethod UncannyValet.EditorTools.ProjectSetup.RunBatch
    /// </summary>
    public static class ProjectSetup
    {
        public const string LotConfigPath = "Assets/_Project/Data/Lot/LotConfig.asset";
        public const string PatronConfigPath = "Assets/_Project/Data/Patrons/PatronConfig.asset";
        public const string AudioConfigPath = "Assets/_Project/Data/Audio/AudioConfig.asset";
        public const string ShiftConfigPath = "Assets/_Project/Data/Shift/ShiftConfig.asset";
        public const string DirectorConfigPath = "Assets/_Project/Data/Director/DirectorConfig.asset";
        public const string DinerDirectorProfilePath = "Assets/_Project/Data/Director/DinerDirectorProfile.asset";

        [MenuItem("Uncanny Valet/Setup/Create Config Assets")]
        public static void CreateConfigs()
        {
            CreateIfMissing<LotConfig>(LotConfigPath);
            CreateIfMissing<PatronConfig>(PatronConfigPath);
            CreateIfMissing<AudioConfig>(AudioConfigPath);
            CreateIfMissing<ShiftConfig>(ShiftConfigPath);
            CreateIfMissing<DirectorConfig>(DirectorConfigPath);
            CreateIfMissing<DirectorProfile>(DinerDirectorProfilePath);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Uncanny Valet/Setup/Build Everything")]
        public static void RunBatch()
        {
            CreateConfigs();
            BootstrapSceneBuilder.Build();
        }

        private static void CreateIfMissing<T>(string path) where T : ScriptableObject
        {
            if (AssetDatabase.LoadAssetAtPath<T>(path) != null) return;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<T>(), path);
        }
    }
}
