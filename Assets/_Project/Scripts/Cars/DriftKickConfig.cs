using System;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Tunable numbers of the drift entry kick, turn boost and grip hold (issue 151).</summary>
    [Serializable]
    public sealed class DriftKickConfig
    {
        [SerializeField, Range(0f, 6f), Tooltip("Extra yaw rate (rad/s) at the moment Shift is pressed while steering; fades over the kick length.")]
        private float _kickRate = 3f;
        [SerializeField, Range(0.02f, 0.5f), Tooltip("Seconds the entry kick takes to fade out.")]
        private float _kickSeconds = 0.12f;
        [SerializeField, Range(1f, 2f), Tooltip("Turn rate multiplier while the drift button is held.")]
        private float _turnScale = 1.3f;
        [SerializeField, Range(0f, 1f), Tooltip("Seconds the car keeps its drift grip after Shift is released.")]
        private float _holdSeconds = 0.25f;
        [SerializeField, Range(0.01f, 1f), Tooltip("Seconds grip takes to blend back to normal after the hold.")]
        private float _blendSeconds = 0.2f;

        /// <summary>Entry kick yaw rate, radians per second.</summary>
        public float KickRate => _kickRate;
        /// <summary>Entry kick length, seconds.</summary>
        public float KickSeconds => _kickSeconds;
        /// <summary>Turn rate multiplier while drifting.</summary>
        public float TurnScale => _turnScale;
        /// <summary>Seconds drift grip is kept after release.</summary>
        public float HoldSeconds => _holdSeconds;
        /// <summary>Seconds grip blends back to normal.</summary>
        public float BlendSeconds => _blendSeconds;
    }
}
