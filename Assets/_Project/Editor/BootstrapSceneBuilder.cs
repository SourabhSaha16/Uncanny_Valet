using UncannyValet.Audio;
using UncannyValet.Bootstrap;
using UncannyValet.Cars;
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

        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Bootstrap");
            var bootstrap = root.AddComponent<GameBootstrap>();

            var factory = Child<CarFactory>(root, "CarFactory");
            var lot = Child<LotLayoutBuilder>(root, "Lot");
            var clock = Child<NightClock>(root, "NightClock");
            var spawner = Child<PatronSpawner>(root, "PatronSpawner");
            var overlay = Child<DebugOverlay>(root, "DebugOverlay");
            var summaryCard = Child<NightSummaryCard>(root, "NightSummaryCard");
            var audio = Child<AudioDirector>(root, "AudioDirector");
            var valet = BuildValet(root);
            var camera = BuildCamera();

            Set(factory, "_defaultDefinition", Load<CarDefinition>("Assets/_Project/Data/Cars/Sedan.asset"));
            Set(lot, "_config", Load<LotConfig>(ProjectSetup.LotConfigPath));
            Set(spawner, "_config", Load<PatronConfig>(ProjectSetup.PatronConfigPath));
            Set(audio, "_config", RequireAsset<AudioConfig>(ProjectSetup.AudioConfigPath));

            var so = new SerializedObject(bootstrap);
            so.FindProperty("_runConfig").objectReferenceValue = Load<RunConfig>("Assets/_Project/Data/Economy/RunConfig.asset");
            so.FindProperty("_tipConfig").objectReferenceValue = Load<TipConfig>("Assets/_Project/Data/Economy/TipConfig.asset");
            so.FindProperty("_planConfig").objectReferenceValue = RequireAsset<NightPlanConfig>(ProjectSetup.NightPlanConfigPath);
            so.FindProperty("_carFactory").objectReferenceValue = factory;
            so.FindProperty("_lot").objectReferenceValue = lot;
            so.FindProperty("_clock").objectReferenceValue = clock;
            so.FindProperty("_valet").objectReferenceValue = valet;
            so.FindProperty("_spawner").objectReferenceValue = spawner;
            so.FindProperty("_overlay").objectReferenceValue = overlay;
            so.FindProperty("_camera").objectReferenceValue = camera;
            so.FindProperty("_summaryCard").objectReferenceValue = summaryCard;
            so.FindProperty("_audio").objectReferenceValue = audio;
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

        private static GameObject BuildValet(GameObject parent)
        {
            var go = new GameObject("Valet");
            go.transform.SetParent(parent.transform, false);
            go.SetActive(false);
            go.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            var sprite = go.AddComponent<SpriteRenderer>();
            sprite.sortingOrder = 20;

            go.AddComponent<KeyboardInputSource>();
            var mover = go.AddComponent<OnFootMover>();
            var interactor = go.AddComponent<Interactor>();
            var seat = go.AddComponent<SeatController>();

            var so = new SerializedObject(seat);
            so.FindProperty("_mover").objectReferenceValue = mover;
            so.FindProperty("_interactor").objectReferenceValue = interactor;
            SetArray(so.FindProperty("_bodyRenderers"), sprite);
            SetArray(so.FindProperty("_bodyColliders"), collider);
            so.ApplyModifiedPropertiesWithoutUndo();
            return go;
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

        private static void SetArray(SerializedProperty array, Object item)
        {
            array.arraySize = 1;
            array.GetArrayElementAtIndex(0).objectReferenceValue = item;
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
