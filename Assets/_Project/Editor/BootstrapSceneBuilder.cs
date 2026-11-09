using UncannyValet.Audio;
using UncannyValet.Bootstrap;
using UncannyValet.Cars;
using UncannyValet.Core.Config;
using UncannyValet.EditorTools.Art;
using UncannyValet.Economy;
using UncannyValet.Lot;
using UncannyValet.Night;
using UncannyValet.Patrons;
using UncannyValet.UI;
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
        private const float ValetScale = 1.4f;
        private const float ValetColliderRadius = 0.3f; // times the scale: about the old 0.4 world radius
        private const int ValetSortingOrder = 20;
        private static readonly string[] ValetLayerNames = { "Model", "Outfit", "Hair" };

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
            var carArt = CarArtBuilder.Build();
            var lotArt = LotArtBuilder.Build();
            var valet = BuildValet(root, ValetArtBuilder.Build());
            var decor = BuildDecor(root, lotArt);
            var camera = BuildCamera();

            Set(factory, "_defaultDefinition", Load<CarDefinition>("Assets/_Project/Data/Cars/Sedan.asset"));
            Set(lot, "_config", Load<LotConfig>(ProjectSetup.LotConfigPath));
            Set(lot, "_art", lotArt);
            Set(factory, "_art", carArt);
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

        private static GameObject BuildValet(GameObject parent, ValetArtConfig art)
        {
            var go = new GameObject("Valet");
            go.transform.SetParent(parent.transform, false);
            go.SetActive(false);
            go.transform.localScale = new Vector3(ValetScale, ValetScale, 1f);

            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = ValetColliderRadius;
            var layers = AddValetLayers(go, art);

            go.AddComponent<KeyboardInputSource>();
            var mover = go.AddComponent<OnFootMover>();
            var interactor = go.AddComponent<Interactor>();
            var seat = go.AddComponent<SeatController>();

            var so = new SerializedObject(seat);
            so.FindProperty("_mover").objectReferenceValue = mover;
            so.FindProperty("_interactor").objectReferenceValue = interactor;
            SetArray(so.FindProperty("_bodyRenderers"), layers);
            SetArray(so.FindProperty("_bodyColliders"), collider);
            so.ApplyModifiedPropertiesWithoutUndo();
            return go;
        }

        private static SpriteRenderer[] AddValetLayers(GameObject valet, ValetArtConfig art)
        {
            var layers = new SpriteRenderer[ValetLayerNames.Length];
            for (var i = 0; i < layers.Length; i++)
            {
                var child = new GameObject(ValetLayerNames[i]);
                child.transform.SetParent(valet.transform, false);
                layers[i] = child.AddComponent<SpriteRenderer>();
                layers[i].sortingOrder = ValetSortingOrder + i;
            }
            var animator = valet.AddComponent<ValetAnimator>();
            var so = new SerializedObject(animator);
            so.FindProperty("_config").objectReferenceValue = art;
            so.FindProperty("_model").objectReferenceValue = layers[0];
            so.FindProperty("_outfit").objectReferenceValue = layers[1];
            so.FindProperty("_hair").objectReferenceValue = layers[2];
            so.ApplyModifiedPropertiesWithoutUndo();
            return layers;
        }

        private static CameraFollow BuildCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(-9f, 0f, -10f); // valet start; CameraFollow snaps here
            var cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            cam.orthographic = true;
            cam.orthographicSize = CameraFollow.DefaultOrthographicSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.07f, 0.09f);
            return go.AddComponent<CameraFollow>();
        }

        private static void SetArray(SerializedProperty array, params Object[] items)
        {
            array.arraySize = items.Length;
            for (var i = 0; i < items.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
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
