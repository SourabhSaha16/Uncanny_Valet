using UncannyValet.Bootstrap;
using UncannyValet.Bootstrap.Sandbox;
using UncannyValet.Cars;
using UncannyValet.Economy;
using UncannyValet.EditorTools.Art;
using UncannyValet.Lot;
using UncannyValet.Pedestrians;
using UncannyValet.Run;
using UncannyValet.UI;
using UncannyValet.UI.Mood;
using UncannyValet.UI.Sandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// Builds Sandbox.unity: one SandboxBootstrap with the car factory, the valet, the camera, the F2 panel, the world picker and one inactive world
    /// template per lot definition of the SandboxWorldsConfig. It is not in Build Settings; the sandbox web build lists it itself.
    /// </summary>
    public static class SandboxSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Sandbox.unity";
        public const string WorldsPath = "Assets/_Project/Data/Sandbox/SandboxWorldsConfig.asset";
        private const string DefaultCarPath = "Assets/_Project/Data/Cars/Sedan.asset";
        private const string CarFlashConfigPath = "Assets/_Project/Data/Cars/CarFlashConfig.asset";
        private const string HudEmotionConfigPath = "Assets/_Project/Data/UI/HudEmotionConfig.asset";
        private const string BlessingConfigPath = "Assets/_Project/Data/Economy/BlessingConfig.asset";

        private static readonly string[] DefaultWorlds =
        {
            RunAssetSetup.MidgardTrainingPath, RunAssetSetup.MidgardCrazyPath,
            RunAssetSetup.VanaheimTrainingPath, RunAssetSetup.VanaheimCrazyPath,
        };

        /// <summary>Creates the worlds list with the four lots when it does not exist yet; an existing list is never changed.</summary>
        public static void CreateWorldsIfMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<SandboxWorldsConfig>(WorldsPath) != null) return;
            var config = ArtAssetUtil.LoadOrCreate<SandboxWorldsConfig>(WorldsPath);
            var so = new SerializedObject(config);
            var list = so.FindProperty("_worlds");
            list.arraySize = DefaultWorlds.Length;
            for (var i = 0; i < DefaultWorlds.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = BootstrapSceneBuilder.Load<LotDefinition>(DefaultWorlds[i]);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Builds the sandbox scene from the SandboxWorldsConfig.</summary>
        public static void Build()
        {
            CreateWorldsIfMissing();
            SandboxHandmadeWorld.EnsureListed();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Loaded after NewScene: creating a scene unloads unused assets, which would null earlier references.
            var worlds = BootstrapSceneBuilder.RequireAsset<SandboxWorldsConfig>(WorldsPath);
            var lotConfig = BootstrapSceneBuilder.RequireAsset<LotConfig>(ProjectSetup.LotConfigPath);
            var root = new GameObject("Sandbox");
            var bootstrap = root.AddComponent<SandboxBootstrap>();
            var carArt = CarArtBuilder.Build();
            var lotArt = LotArtBuilder.Build();
            var factory = BootstrapSceneBuilder.Child<CarFactory>(root, "CarFactory");
            var valet = ValetSceneBuilder.BuildValet(root, ValhallaAssetSetup.Thor());
            var camera = ValetSceneBuilder.BuildCamera();
            var panel = BootstrapSceneBuilder.Child<TuningPanel>(root, "TuningPanel");
            var picker = BootstrapSceneBuilder.Child<SandboxPicker>(root, "SandboxPicker");
            BootstrapSceneBuilder.Set(picker, "_tuningPanel", panel);
            WireFactory(root, factory, carArt, lotConfig);

            var slots = new SandboxWorldSlot[worlds.Worlds.Count];
            for (var i = 0; i < slots.Length; i++) slots[i] = BuildSlot(root, worlds.Worlds[i], lotArt, lotConfig);
            Wire(bootstrap, factory, valet, camera, panel, picker, slots, carArt);

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("Sandbox scene built: " + ScenePath + " with " + slots.Length + " worlds");
        }

        private static void WireFactory(GameObject root, CarFactory factory, CarArtConfig carArt, LotConfig lotConfig)
        {
            BootstrapSceneBuilder.Set(factory, "_defaultDefinition", BootstrapSceneBuilder.Load<CarDefinition>(DefaultCarPath));
            BootstrapSceneBuilder.Set(factory, "_art", carArt);
            BootstrapSceneBuilder.Set(factory, "_flash", ArtAssetUtil.LoadOrCreate<CarFlashConfig>(CarFlashConfigPath));
            BootstrapSceneBuilder.AddHandling(root, factory, lotConfig);
        }

        // An inactive root holding the lot builder and the decor for one lot definition; the sandbox clones it to show the world.
        private static SandboxWorldSlot BuildSlot(GameObject root, LotDefinition definition, LotArtConfig lotArt, LotConfig lotConfig)
        {
            if (definition.Layout == LotLayoutKind.Handmade) return SandboxHandmadeWorld.BuildSlot(root, definition);
            var go = new GameObject("World " + definition.DisplayName);
            go.transform.SetParent(root.transform, false);
            go.SetActive(false);
            var lotGo = new GameObject("Lot");
            lotGo.transform.SetParent(go.transform, false);
            var lot = lotGo.AddComponent<LotLayoutBuilder>();
            var decor = BootstrapSceneBuilder.BuildDecor(go, lotArt, lotConfig);
            BootstrapSceneBuilder.Set(lot, "_config", lotConfig);
            BootstrapSceneBuilder.Set(lot, "_art", lotArt);
            BootstrapSceneBuilder.SetInt(lot, "_queueSlots", definition.QueueSlots);
            BootstrapSceneBuilder.SetInt(decor, "_queueSlots", definition.QueueSlots);
            LotWorldSceneSetup.Apply(lot, definition);
            LotWorldSceneSetup.ApplyLayout(lot, decor, definition);
            LotWorldSceneSetup.ApplyArt(lot, decor, definition);
            var slot = go.AddComponent<SandboxWorldSlot>();
            var so = new SerializedObject(slot);
            so.FindProperty("_definition").objectReferenceValue = definition;
            so.FindProperty("_lot").objectReferenceValue = lot;
            so.FindProperty("_decor").objectReferenceValue = decor;
            so.ApplyModifiedPropertiesWithoutUndo();
            DressMidgard(go, definition, lotConfig, lotArt);
            return slot;
        }

        // The two Midgard worlds get the Valhalla art and its pedestrians; the crazy lot keeps its fair props (stalls and wheel are solid).
        private static void DressMidgard(GameObject world, LotDefinition definition, LotConfig lotConfig, LotArtConfig lotArt)
        {
            var path = AssetDatabase.GetAssetPath(definition);
            var training = path == RunAssetSetup.MidgardTrainingPath;
            if (!training && path != RunAssetSetup.MidgardCrazyPath) return;
            var skin = world.AddComponent<ValhallaLotSkin>();
            BootstrapSceneBuilder.Set(skin, "_art", ValhallaAssetSetup.LotArt());
            BootstrapSceneBuilder.Set(skin, "_config", lotConfig);
            BootstrapSceneBuilder.Set(skin, "_lotArt", lotArt);
            var so = new SerializedObject(skin);
            so.FindProperty("_keepProps").boolValue = !training;
            so.ApplyModifiedPropertiesWithoutUndo();
            var crowd = BootstrapSceneBuilder.Child<PedestrianCrowd>(world, "Pedestrians");
            crowd.gameObject.SetActive(false);
            BootstrapSceneBuilder.Set(crowd, "_config", ValhallaAssetSetup.Crowd());
            BootstrapSceneBuilder.Set(world.AddComponent<SandboxCrowd>(), "_crowd", crowd);
        }

        private static void Wire(SandboxBootstrap bootstrap, CarFactory factory, GameObject valet, CameraFollow camera, TuningPanel panel,
            SandboxPicker picker, SandboxWorldSlot[] slots, CarArtConfig carArt)
        {
            var so = new SerializedObject(bootstrap);
            so.FindProperty("_carFactory").objectReferenceValue = factory;
            so.FindProperty("_valet").objectReferenceValue = valet;
            so.FindProperty("_camera").objectReferenceValue = camera;
            so.FindProperty("_tuningPanel").objectReferenceValue = panel;
            so.FindProperty("_picker").objectReferenceValue = picker;
            var list = so.FindProperty("_worlds");
            list.arraySize = slots.Length;
            for (var i = 0; i < slots.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
            Seed(so.FindProperty("_seed"), carArt);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Seed(SerializedProperty seed, CarArtConfig carArt)
        {
            seed.FindPropertyRelative("_shift").objectReferenceValue = BootstrapSceneBuilder.RequireAsset<Core.Config.ShiftConfig>(ProjectSetup.ShiftConfigPath);
            seed.FindPropertyRelative("_patrons").objectReferenceValue = BootstrapSceneBuilder.RequireAsset<Patrons.PatronConfig>(ProjectSetup.PatronConfigPath);
            seed.FindPropertyRelative("_blessings").objectReferenceValue = BootstrapSceneBuilder.Load<BlessingConfig>(BlessingConfigPath);
            seed.FindPropertyRelative("_director").objectReferenceValue = BootstrapSceneBuilder.RequireAsset<Director.DirectorConfig>(ProjectSetup.DirectorConfigPath);
            seed.FindPropertyRelative("_profile").objectReferenceValue = BootstrapSceneBuilder.RequireAsset<Director.DirectorProfile>(ProjectSetup.DinerDirectorProfilePath);
            seed.FindPropertyRelative("_traffic").objectReferenceValue = TrafficArtBuilder.Build(carArt, BootstrapSceneBuilder.Load<CarDefinition>(DefaultCarPath));
            seed.FindPropertyRelative("_powers").objectReferenceValue = BootstrapSceneBuilder.RequireAsset<Powers.PowerBarConfig>(ProjectSetup.PowerBarConfigPath);
            seed.FindPropertyRelative("_audio").objectReferenceValue = BootstrapSceneBuilder.RequireAsset<Audio.AudioConfig>(ProjectSetup.AudioConfigPath);
            seed.FindPropertyRelative("_emotions").objectReferenceValue = ArtAssetUtil.LoadOrCreate<HudEmotionConfig>(HudEmotionConfigPath);
        }
    }
}
