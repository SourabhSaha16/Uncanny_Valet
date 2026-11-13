using System;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Run
{
    /// <summary>Where one difficulty dial starts in a lot (0..10). A dial not listed keeps the global baseline.</summary>
    [Serializable]
    public struct DialPosition
    {
        public const float MaxPosition = 10f;

        [SerializeField, Tooltip("One of the Dial* tuning keys.")]
        private TuningKey _dial;
        [SerializeField, Range(0f, MaxPosition)]
        private float _position;

        public DialPosition(TuningKey dial, float position)
        {
            _dial = dial;
            _position = position;
        }

        public TuningKey Dial => _dial;
        public float Position => _position;

        /// <summary>True when the key is one of the eight difficulty dials and the position is on the slider.</summary>
        public bool IsValid => IsDialKey(_dial) && _position >= 0f && _position <= MaxPosition;

        /// <summary>The eight dials are the contiguous keys DialIntensity to DialSkillReward.</summary>
        public static bool IsDialKey(TuningKey key) => key >= TuningKey.DialIntensity && key <= TuningKey.DialSkillReward;
    }
}
