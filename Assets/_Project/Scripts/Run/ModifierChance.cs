using System;
using UncannyValet.Core.Modifiers;
using UnityEngine;

namespace UncannyValet.Run
{
    /// <summary>One service modifier of a lot and the chance a new car arrives with it (0 to 1).</summary>
    [Serializable]
    public struct ModifierChance
    {
        [SerializeField, Tooltip("The service modifier (Muddy, Needs charge, ...). Its position in the lot's list is its roll order.")]
        private ServiceModifierDefinition _modifier;
        [SerializeField, Range(0f, 1f), Tooltip("Chance 0 to 1 that a new car arrives with this modifier. The rolls are one draw in list order, so the chances of a lot add up to at most 1.")]
        private float _chance;

        public ModifierChance(ServiceModifierDefinition modifier, float chance)
        {
            _modifier = modifier;
            _chance = chance;
        }

        public ServiceModifierDefinition Modifier => _modifier;
        public float Chance => _chance;

        /// <summary>True when the modifier is set and the chance is between 0 and 1.</summary>
        public bool IsValid => _modifier != null && _chance >= 0f && _chance <= 1f;
    }
}
