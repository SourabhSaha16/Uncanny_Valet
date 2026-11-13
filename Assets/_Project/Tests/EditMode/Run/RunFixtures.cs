using System.Collections.Generic;
using UncannyValet.Core.Ids;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.Tests.Run
{
    /// <summary>Builds in-memory LotDefinition and RunDefinition assets for the run tests; destroys them on <see cref="Dispose"/>.</summary>
    public sealed class RunFixtures
    {
        private readonly List<Object> _made = new List<Object>();

        public LotDefinition Lot(World world, LotKind kind, string scene = null, string name = "Lot")
        {
            var lot = Make<LotDefinition>(world + "_" + kind);
            var so = new SerializedObject(lot);
            so.FindProperty("_id._world").intValue = (int)world;
            so.FindProperty("_id._kind").intValue = (int)kind;
            so.FindProperty("_displayName").stringValue = name;
            so.FindProperty("_sceneName").stringValue = scene ?? (world + "_" + kind);
            so.FindProperty("_layout").intValue = (int)LotLayoutKind.Diner;
            so.ApplyModifiedPropertiesWithoutUndo();
            return lot;
        }

        public RunDefinition Run(params LotDefinition[] lots)
        {
            var run = Make<RunDefinition>("Run");
            var so = new SerializedObject(run);
            var list = so.FindProperty("_lots");
            list.arraySize = lots.Length;
            for (var i = 0; i < lots.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = lots[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            return run;
        }

        public RunDefinition MidgardRun() => Run(Lot(World.Midgard, LotKind.Training, "Bootstrap", "Grandma's Diner"),
            Lot(World.Midgard, LotKind.Crazy, "Crazy", "Fair Night at Grandma's"));

        /// <summary>Sets a serialized field of an asset, for example <c>_blessingThreshold</c> or <c>_pacing._shiftLengthSeconds</c>.</summary>
        public static void Set(Object asset, string path, float value)
        {
            var so = new SerializedObject(asset);
            var prop = so.FindProperty(path);
            if (prop.propertyType == SerializedPropertyType.Integer || prop.propertyType == SerializedPropertyType.Enum) prop.intValue = (int)value;
            else prop.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetString(Object asset, string path, string value)
        {
            var so = new SerializedObject(asset);
            so.FindProperty(path).stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public void Dispose()
        {
            foreach (var asset in _made) Object.DestroyImmediate(asset);
            _made.Clear();
        }

        private T Make<T>(string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            _made.Add(asset);
            return asset;
        }
    }
}
