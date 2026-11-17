using System;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>Tunables of the power bar HUD. The power numbers (durations, cooldowns) come from the powers module through IPowerView.</summary>
    [Serializable]
    public sealed class PowerBarSettings
    {
        [SerializeField, Min(0.1f), Tooltip("Fill animation speed: the shown fill closes this fraction of the gap per second.")]
        private float _fillSmoothing = 8f;
        [SerializeField, Range(0.1f, 2f), Tooltip("Seconds the bar pulses after it gains a drop.")]
        private float _pulseSeconds = 0.45f;
        [SerializeField, Range(0.1f, 2f), Tooltip("Seconds the bolt that was just spent flashes.")]
        private float _spentFlashSeconds = 0.5f;
        [SerializeField, Range(0.2f, 5f), Tooltip("Seconds a power icon flashes at the start of the first lot after the power (or its upgrade) was earned.")]
        private float _iconFlashSeconds = 1.5f;

        public float FillSmoothing => _fillSmoothing;
        public float PulseSeconds => _pulseSeconds;
        public float SpentFlashSeconds => _spentFlashSeconds;
        public float IconFlashSeconds => _iconFlashSeconds;
    }
}
