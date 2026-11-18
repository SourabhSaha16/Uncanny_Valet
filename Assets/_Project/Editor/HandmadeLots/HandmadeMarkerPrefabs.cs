using System;
using System.IO;
using UncannyValet.Lot;
using UncannyValet.Lot.Handmade;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>
    /// Generates the 19 gameplay marker prefabs an artist drags into a lot. Idempotent: an existing prefab is opened and brought to the spec
    /// (same file, same GUID, same object ids), a missing one is created. Only the values that make a prefab what it is are set (the kind, the side,
    /// the lot config); every other field keeps what is on the prefab, and a new one gets the marker's own defaults.
    /// </summary>
    public static class HandmadeMarkerPrefabs
    {
        public const string Folder = "Assets/_Project/Prefabs/Lot";
        private const string LotConfigPath = "Assets/_Project/Data/Lot/LotConfig.asset";

        private readonly struct Spec
        {
            public readonly string Name;
            public readonly Type Marker;
            public readonly Action<SerializedObject> Configure;

            public Spec(string name, Type marker, Action<SerializedObject> configure = null)
            {
                Name = name;
                Marker = marker;
                Configure = configure;
            }
        }

        private static readonly Spec[] Specs =
        {
            new Spec("LotBay_Regular", typeof(LotBay), so => Enum(so, "_kind", (int)BayKind.Regular)),
            new Spec("LotBay_EV", typeof(LotBay), so => Enum(so, "_kind", (int)BayKind.Charging)),
            new Spec("LotBay_Wash", typeof(LotBay), so => Enum(so, "_kind", (int)BayKind.Wash)),
            new Spec("CurbZone", typeof(CurbZoneMarker)),
            new Spec("QueueSlot", typeof(QueueSlot)),
            new Spec("CarEntryRoad", typeof(CarEntryRoad)),
            new Spec("CarExitRoad", typeof(CarExitRoad)),
            new Spec("AmbientLane_EntryBack", typeof(AmbientLane), so => Enum(so, "_side", (int)AmbientLaneSide.EntryBack)),
            new Spec("AmbientLane_ExitBack", typeof(AmbientLane), so => Enum(so, "_side", (int)AmbientLaneSide.ExitBack)),
            new Spec("LotDoor_Main", typeof(LotDoor), so => Enum(so, "_kind", (int)DoorKind.Main)),
            new Spec("LotDoor_Side", typeof(LotDoor), so => Enum(so, "_kind", (int)DoorKind.Side)),
            new Spec("PatronWaitPoint", typeof(PatronWaitPoint)),
            new Spec("AmbientSpawnPoint", typeof(AmbientSpawnPoint)),
            new Spec("WalkNode", typeof(WalkNode)),
            new Spec("WalkLink", typeof(WalkLink)),
            new Spec("SolidBlock", typeof(SolidBlock)),
            new Spec("HazardPatch", typeof(HazardPatch)),
            new Spec("ValetStart", typeof(ValetStart)),
            new Spec("HandmadeLotRoot", typeof(HandmadeLot), SetLotConfig),
        };

        /// <summary>Number of marker prefabs the generator owns.</summary>
        public static int Count => Specs.Length;

        /// <summary>The asset path of the prefab called <paramref name="name"/>.</summary>
        public static string PathOf(string name) => Folder + "/" + name + ".prefab";

        /// <summary>Creates or refreshes every marker prefab.</summary>
        public static void Build()
        {
            Directory.CreateDirectory(Folder);
            foreach (var spec in Specs) BuildOne(spec);
        }

        private static void BuildOne(Spec spec)
        {
            var path = PathOf(spec.Name);
            var existing = File.Exists(path);
            var go = existing ? PrefabUtility.LoadPrefabContents(path) : new GameObject(spec.Name);
            var marker = go.GetComponent(spec.Marker);
            if (marker == null) marker = go.AddComponent(spec.Marker);
            var so = new SerializedObject(marker);
            spec.Configure?.Invoke(so);
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(go, path);
            if (existing) PrefabUtility.UnloadPrefabContents(go);
            else UnityEngine.Object.DestroyImmediate(go);
        }

        private static void Enum(SerializedObject so, string field, int value) => so.FindProperty(field).intValue = value;

        private static void SetLotConfig(SerializedObject so)
        {
            var config = so.FindProperty("_config");
            if (config.objectReferenceValue == null) config.objectReferenceValue = AssetDatabase.LoadAssetAtPath<LotConfig>(LotConfigPath);
        }
    }
}
