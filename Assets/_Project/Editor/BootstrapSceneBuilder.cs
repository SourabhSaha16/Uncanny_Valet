using UncannyValet.Audio;
using UncannyValet.Bootstrap;
using UncannyValet.Cars;
using UncannyValet.Core.Config;
using UncannyValet.Director;
using UncannyValet.EditorTools.Art;
using UncannyValet.Economy;
using UncannyValet.Lot;
using UncannyValet.Night;
using UncannyValet.Patrons;
using UncannyValet.Pedestrians;
using UncannyValet.Powers;
using UncannyValet.Traffic;
using UncannyValet.UI;
using UncannyValet.UI.Mood;
using UncannyValet.Valet;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>Builds Bootstrap.unity: one GameBootstrap with pre-wired, inactive child objects.</summary>
    public static class BootstrapSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Bootstrap.unity";
        private const string DefaultCarPath = "Assets/_Project/Data/Cars/Sedan.asset";
        private const string CarFlashConfigPath = "Assets/_Project/Data/Cars/CarFlashConfig.asset";
        private const string HudEmotionConfigPath = "Assets/_Project/Data/UI/HudEmotionConfig.asset";

        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Bootstrap");
            var bootstrap = root.AddComponent<GameBootstrap>();

            var factory = Child<CarFactory>(root, "CarFactory");
            var lot = Child<LotLayoutBuilder>(root, "Lot");
            var clock = Child<ShiftClock>(root, "ShiftClock");
            var spawner = Child<PatronSpawner>(root, "PatronSpawner");
            var ui = Child<GameUi>(root, "GameUi");
            var tuningPanel = Child<TuningPanel>(root, "TuningPanel");
            var audio = Child<AudioDirector>(root, "AudioDirector");
            var director = Child<PatronDirector>(root, "PatronDirector");
            var directorOverlay = Child<DirectorOverlay>(root, "DirectorOverlay");
            Set(directorOverlay, "_tuningPanel", tuningPanel);
            var carArt = CarArtBuilder.Build();
            var lotArt = LotArtBuilder.Build();
            var valet = ValetSceneBuilder.BuildValet(root, ValetArtBuilder.Build());
            var decor = BuildDecor(root, lotArt);
            var crowd = Child<PedestrianCrowd>(root, "Pedestrians");
            Set(crowd, "_config", PedestrianArtBuilder.Build());
            var traffic = Child<TrafficModule>(root, "Traffic");
            var trafficConfig = TrafficArtBuilder.Build(carArt, Load<CarDefinition>(DefaultCarPath));
            Set(traffic, "_config", trafficConfig);
            var camera = ValetSceneBuilder.BuildCamera();
            var powers = PowersSceneBuilder.Build(root, camera);

            Set(factory, "_defaultDefinition", Load<CarDefinition>(DefaultCarPath));
            Set(lot, "_config", Load<LotConfig>(ProjectSetup.LotConfigPath));
            Set(lot, "_art", lotArt);
            Set(factory, "_art", carArt);
            Set(factory, "_flash", ArtAssetUtil.LoadOrCreate<CarFlashConfig>(CarFlashConfigPath));
            AddHandling(root, factory, Load<LotConfig>(ProjectSetup.LotConfigPath));
            Set(ui, "_emotions", ArtAssetUtil.LoadOrCreate<HudEmotionConfig>(HudEmotionConfigPath));
            var patronConfig = Load<PatronConfig>(ProjectSetup.PatronConfigPath);
            Set(spawner, "_config", patronConfig);
            Set(audio, "_config", RequireAsset<AudioConfig>(ProjectSetup.AudioConfigPath));

            var so = new SerializedObject(bootstrap);
            so.FindProperty("_shiftConfig").objectReferenceValue = RequireAsset<ShiftConfig>(ProjectSetup.ShiftConfigPath);
            so.FindProperty("_tipConfig").objectReferenceValue = Load<TipConfig>("Assets/_Project/Data/Economy/TipConfig.asset");
            so.FindProperty("_patronConfig").objectReferenceValue = patronConfig;
            so.FindProperty("_tuningPanel").objectReferenceValue = tuningPanel;
            so.FindProperty("_carFactory").objectReferenceValue = factory;
            so.FindProperty("_lot").objectReferenceValue = lot;
            so.FindProperty("_clock").objectReferenceValue = clock;
            so.FindProperty("_valet").objectReferenceValue = valet;
            so.FindProperty("_spawner").objectReferenceValue = spawner;
            so.FindProperty("_ui").objectReferenceValue = ui;
            so.FindProperty("_camera").objectReferenceValue = camera;
            so.FindProperty("_audio").objectReferenceValue = audio;
            so.FindProperty("_decor").objectReferenceValue = decor;
            so.FindProperty("_pedestrians").objectReferenceValue = crowd;
            so.FindProperty("_director").objectReferenceValue = director;
            so.FindProperty("_directorConfig").objectReferenceValue = RequireAsset<DirectorConfig>(ProjectSetup.DirectorConfigPath);
            so.FindProperty("_directorProfile").objectReferenceValue = RequireAsset<DirectorProfile>(ProjectSetup.DinerDirectorProfilePath);
            so.FindProperty("_directorOverlay").objectReferenceValue = directorOverlay;
            so.FindProperty("_traffic").objectReferenceValue = traffic;
            so.FindProperty("_trafficConfig").objectReferenceValue = trafficConfig;
            so.FindProperty("_powers").objectReferenceValue = powers;
            so.FindProperty("_powerConfig").objectReferenceValue = RequireAsset<PowerMeterConfig>(ProjectSetup.PowerMeterConfigPath);
            so.ApplyModifiedPropertiesWithoutUndo();

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Bootstrap scene built: " + ScenePath);
        }

        private static T Child<T>(GameObject parent, string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.SetActive(false);
            return go.AddComponent<T>();
        }

        private static LotDecor BuildDecor(GameObject parent, LotArtConfig art)
        {
            var go = new GameObject("LotDecor"); // active: builds its scenery in Awake, independent of the lot
            go.transform.SetParent(parent.transform, false);
            var decor = go.AddComponent<LotDecor>();
            Set(decor, "_config", Load<LotConfig>(ProjectSetup.LotConfigPath));
            Set(decor, "_art", art);
            return decor;
        }

        // The handling config follows the lot's aisle; the check component reports a later mismatch at start.
        private static void AddHandling(GameObject root, CarFactory factory, LotConfig lot)
        {
            var handling = HandlingAssetSetup.LoadAndSync(lot);
            Set(factory, "_handling", handling);
            var check = root.AddComponent<HandlingAisleCheck>();
            Set(check, "_lot", lot);
            Set(check, "_handling", handling);
        }

        private static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);

        private static T RequireAsset<T>(string path) where T : Object
        {
            var asset = Load<T>(path);
            if (asset == null) Debug.LogError("Missing asset " + path + ". Run Uncanny Valet/Setup/Create Config Assets first.");
            return asset;
        }
    }
}
