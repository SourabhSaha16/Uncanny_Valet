using System;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>Tunables of the power meter HUD. The durations only drive the countdown rings; the powers module owns the real ones.</summary>
    [Serializable]
    public sealed class PowerMeterSettings
    {
        [SerializeField, Min(0.1f), Tooltip("Fill animation speed: the shown fill closes this fraction of the gap per second.")]
        private float _fillSmoothing = 8f;
        [SerializeField, Range(0.1f, 2f), Tooltip("Seconds the bar pulses after it fills or a level is reached.")]
        private float _pulseSeconds = 0.45f;
        [SerializeField, Range(0f, 1f), Tooltip("Bar fill at which level 1 is reached; the notch is drawn here.")]
        private float _level1Notch = 0.34f;
        [SerializeField, Range(0f, 1f), Tooltip("Bar fill at which level 2 is reached.")]
        private float _level2Notch = 0.67f;
        [SerializeField, Min(0.1f), Tooltip("Countdown ring length of Speed Boost, seconds (match the powers config).")]
        private float _speedBoostSeconds = 4f;
        [SerializeField, Min(0.1f), Tooltip("Countdown ring length of Freeze Time, seconds (match the powers config).")]
        private float _freezeSeconds = 6f;
        [SerializeField, Min(0.1f), Tooltip("Seconds the Park Smash icon stays lit; it is instant so it has no real countdown.")]
        private float _smashFlashSeconds = 1.2f;

        public float FillSmoothing => _fillSmoothing;
        public float PulseSeconds => _pulseSeconds;

        /// <summary>Bar fill (0 to 1) at which meter level 1 to 3 is reached; level 3 is always a full bar.</summary>
        public float NotchFill(int level) => level == 1 ? _level1Notch : level == 2 ? _level2Notch : 1f;

        /// <summary>Seconds the ring of a power (1 to 3) counts down.</summary>
        public float DurationOf(int power) => power == 1 ? _speedBoostSeconds : power == 2 ? _freezeSeconds : _smashFlashSeconds;
    }
}
