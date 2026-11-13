using System;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>Tunables of the power bar HUD. The freeze length only drives the countdown ring; the powers module owns the real one.</summary>
    [Serializable]
    public sealed class PowerBarSettings
    {
        [SerializeField, Min(0.1f), Tooltip("Fill animation speed: the shown fill closes this fraction of the gap per second.")]
        private float _fillSmoothing = 8f;
        [SerializeField, Range(0.1f, 2f), Tooltip("Seconds the bar pulses after it gains a drop.")]
        private float _pulseSeconds = 0.45f;
        [SerializeField, Range(0.1f, 2f), Tooltip("Seconds the bolt that was just spent flashes.")]
        private float _spentFlashSeconds = 0.5f;
        [SerializeField, Min(0.1f), Tooltip("Countdown ring length of Freeze Time, seconds (match the powers config).")]
        private float _freezeSeconds = 6f;

        public float FillSmoothing => _fillSmoothing;
        public float PulseSeconds => _pulseSeconds;
        public float SpentFlashSeconds => _spentFlashSeconds;
        public float FreezeSeconds => _freezeSeconds;
    }
}
