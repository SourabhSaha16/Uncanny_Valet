using System;
using UnityEngine;

namespace UncannyValet.Core.Music
{
    /// <summary>One ambience layer and its level.</summary>
    [Serializable]
    public struct AmbienceLayerMix
    {
        [SerializeField] private AmbienceKind _kind;
        [SerializeField, Range(0f, 1f), Tooltip("Level of this layer inside the ambience set.")]
        private float _level;

        public AmbienceLayerMix(AmbienceKind kind, float level) { _kind = kind; _level = level; }

        public AmbienceKind Kind => _kind;
        public float Level => _level;
    }
}
