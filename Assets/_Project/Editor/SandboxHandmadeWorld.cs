using UncannyValet.Bootstrap.Sandbox;
using UncannyValet.EditorTools.HandmadeLots;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>The hand-made world of the sandbox (#194): the DEV hand-made Midgard lot as a picker entry after the four generated worlds, and the slot that holds its prefab.</summary>
    public static class SandboxHandmadeWorld
    {
        /// <summary>Adds the DEV hand-made lot to the world list when it is missing (an existing list is never reordered); the lot's definition must exist.</summary>
        public static void EnsureListed()
        {
            var definition = AssetDatabase.LoadAssetAtPath<LotDefinition>(RunAssetSetup.MidgardHandmadePath);
            var config = AssetDatabase.LoadAssetAtPath<SandboxWorldsConfig>(SandboxSceneBuilder.WorldsPath);
            if (definition == null || config == null) return;
            foreach (var world in config.Worlds)
                if (world == definition) return;
            var so = new SerializedObject(config);
            var list = so.FindProperty("_worlds");
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = definition;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        /// <summary>An inactive world root with the lot prefab instance (active, so it builds when the sandbox clones the world) and the slot component.</summary>
        public static SandboxWorldSlot BuildSlot(GameObject root, LotDefinition definition)
        {
            var go = new GameObject("World " + definition.DisplayName);
            go.transform.SetParent(root.transform, false);
            go.SetActive(false);
            var lot = HandmadeLotInstance.Place(go.transform, definition, true);
            var slot = go.AddComponent<SandboxWorldSlot>();
            var so = new SerializedObject(slot);
            so.FindProperty("_definition").objectReferenceValue = definition;
            so.FindProperty("_handmadeLot").objectReferenceValue = lot;
            so.ApplyModifiedPropertiesWithoutUndo();
            return slot;
        }
    }
}
