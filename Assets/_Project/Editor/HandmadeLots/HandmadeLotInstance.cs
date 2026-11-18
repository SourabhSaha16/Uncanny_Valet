using UncannyValet.Core.Events;
using UncannyValet.Lot.Handmade;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>Puts the lot prefab of a Handmade <see cref="LotDefinition"/> into a scene under construction and fills the lot's requirements from the definition.</summary>
    public static class HandmadeLotInstance
    {
        /// <summary>Instantiates the prefab under <paramref name="parent"/> and sets its <see cref="LotRequirements"/>; null (and an error) when the definition has no usable prefab.</summary>
        /// <param name="active">False for a scene lot the bootstrap switches on itself.</param>
        public static HandmadeLot Place(Transform parent, LotDefinition definition, bool active)
        {
            var prefab = definition.HandmadePrefab;
            if (prefab == null || prefab.GetComponent<HandmadeLot>() == null)
            {
                Debug.LogError(definition.name + " has no hand-made lot prefab with a HandmadeLot on its root.", definition);
                return null;
            }
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            var lot = instance.GetComponent<HandmadeLot>();
            Apply(lot, definition);
            instance.SetActive(active);
            return lot;
        }

        /// <summary>Writes the queue slot range, the services the modifier chances ask for, ambient traffic and the crowd size of the definition into the lot's requirements.</summary>
        public static void Apply(HandmadeLot lot, LotDefinition definition)
        {
            var so = new SerializedObject(lot);
            var req = so.FindProperty("_requirements");
            req.FindPropertyRelative("_minQueueSlots").intValue = definition.QueueSlots;
            req.FindPropertyRelative("_maxQueueSlots").intValue = definition.QueueSlots;
            req.FindPropertyRelative("_needsCharging").boolValue = Needs(definition, StationKind.Charging);
            req.FindPropertyRelative("_needsWash").boolValue = Needs(definition, StationKind.Wash);
            req.FindPropertyRelative("_ambientTraffic").boolValue = definition.AmbientTraffic;
            req.FindPropertyRelative("_ambientPedestrians").intValue = definition.AmbientPedestrians;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static bool Needs(LotDefinition definition, StationKind kind)
        {
            foreach (var entry in definition.ModifierChances)
                if (entry.Chance > 0f && entry.Modifier != null && entry.Modifier.ClearedAt == kind) return true;
            return false;
        }
    }
}
