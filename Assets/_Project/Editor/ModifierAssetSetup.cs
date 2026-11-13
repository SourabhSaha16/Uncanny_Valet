using System.IO;
using UncannyValet.Core.Modifiers;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// Creates the two shipped service modifier definitions when missing and gives every lot that has no modifier chances yet the
    /// diner's (needs charge, then Muddy). Existing assets are never overwritten, so a designer's edits survive a rebuild.
    /// </summary>
    public static class ModifierAssetSetup
    {
        public const string MuddyPath = "Assets/_Project/Data/Modifiers/Muddy.asset";
        public const string NeedsChargePath = "Assets/_Project/Data/Modifiers/NeedsCharge.asset";

        public static void CreateIfMissing()
        {
            LoadOrCreate(MuddyPath, ModifierDefaults.Muddy());
            LoadOrCreate(NeedsChargePath, ModifierDefaults.NeedsCharge());
        }

        /// <summary>Fills an empty chance list with the diner's chances: needs charge first (the old roll order), then Muddy.</summary>
        public static void EnsureChances(LotDefinition lot)
        {
            var so = new SerializedObject(lot);
            var list = so.FindProperty("_modifierChances");
            if (list.arraySize > 0) return;
            list.arraySize = 2;
            Fill(list.GetArrayElementAtIndex(0), NeedsChargePath, ModifierDefaults.NeedsChargeChance);
            Fill(list.GetArrayElementAtIndex(1), MuddyPath, ModifierDefaults.MuddyChance);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(lot);
        }

        private static void Fill(SerializedProperty entry, string path, float chance)
        {
            entry.FindPropertyRelative("_modifier").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ServiceModifierDefinition>(path);
            entry.FindPropertyRelative("_chance").floatValue = chance;
        }

        private static void LoadOrCreate(string path, ServiceModifierDefinition.Values values)
        {
            if (AssetDatabase.LoadAssetAtPath<ServiceModifierDefinition>(path) != null) return;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var definition = ServiceModifierDefinition.Create(values);
            definition.hideFlags = HideFlags.None;
            AssetDatabase.CreateAsset(definition, path);
        }
    }
}
