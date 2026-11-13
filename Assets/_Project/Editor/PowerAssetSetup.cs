using System.IO;
using UncannyValet.Core.Ids;
using UncannyValet.Powers;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// Creates the four story power definitions when missing (filled with <see cref="PowerDefaults"/>) and lists them in the
    /// power config. Existing assets are never overwritten, so a designer's edits survive a rebuild.
    /// </summary>
    public static class PowerAssetSetup
    {
        private const string Folder = "Assets/_Project/Data/Powers/";

        private static readonly StoryPower[] Powers =
            { StoryPower.ThunderStride, StoryPower.StormWind, StoryPower.FreezeTime, StoryPower.MjolnirRecall };

        public static void CreateIfMissing()
        {
            var config = AssetDatabase.LoadAssetAtPath<PowerBarConfig>(ProjectSetup.PowerBarConfigPath);
            var so = new SerializedObject(config);
            var list = so.FindProperty("_definitions");
            for (var i = 0; i < Powers.Length; i++)
            {
                var definition = LoadOrCreate(Powers[i]);
                if (!Listed(list, definition)) Add(list, definition);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        private static PowerDefinition LoadOrCreate(StoryPower power)
        {
            var path = Folder + power + "Definition.asset";
            var existing = AssetDatabase.LoadAssetAtPath<PowerDefinition>(path);
            if (existing != null) return existing;
            Directory.CreateDirectory(Folder);
            var definition = ScriptableObject.CreateInstance<PowerDefinition>();
            definition.ApplyDefaults(power);
            AssetDatabase.CreateAsset(definition, path);
            return definition;
        }

        private static bool Listed(SerializedProperty list, PowerDefinition definition)
        {
            for (var i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == definition) return true;
            return false;
        }

        private static void Add(SerializedProperty list, PowerDefinition definition)
        {
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = definition;
        }
    }
}
