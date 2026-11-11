using System;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>
    /// One row of the difficulty mapping: how a dial moves one underlying tuning key. The effect is anchored at three
    /// dial positions (minimum, default, maximum) and interpolated between them.
    /// </summary>
    [Serializable]
    public struct DialLink
    {
        [SerializeField, Tooltip("The dial (a TuningKey in the Difficulty group) that drives the key.")]
        private TuningKey _dial;
        [SerializeField, Tooltip("The underlying key that is derived.")]
        private TuningKey _key;
        [SerializeField, Tooltip("Scale: the key's starting value is multiplied by the anchor (around the pivot). Add: the anchor is added to it.")]
        private DialEffect _effect;
        [SerializeField, Tooltip("Scale only: the value that never moves. Use 1 for boost multipliers (1 = no boost), 0 for plain numbers.")]
        private float _pivot;
        [SerializeField, Tooltip("Effect with the dial at its minimum.")]
        private float _atMin;
        [SerializeField, Tooltip("Effect with the dial at its default. Must be 1 (Scale) or 0 (Add) so the default dial position changes nothing.")]
        private float _atDefault;
        [SerializeField, Tooltip("Effect with the dial at its maximum.")]
        private float _atMax;

        public DialLink(TuningKey dial, TuningKey key, float atMin, float atMax, DialEffect effect = DialEffect.Scale, float pivot = 0f)
        {
            _dial = dial; _key = key; _effect = effect; _pivot = pivot; _atMin = atMin; _atMax = atMax;
            _atDefault = effect == DialEffect.Scale ? 1f : 0f;
        }

        public TuningKey Dial => _dial;
        public TuningKey Key => _key;
        public DialEffect Effect => _effect;
        public float Pivot => _pivot;
        public float AtMin => _atMin;
        public float AtDefault => _atDefault;
        public float AtMax => _atMax;
    }
}
