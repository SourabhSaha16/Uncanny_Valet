using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>
    /// Pure timing of the Thunder stride glow: counts the boost down and says how strong the glow is. Strong with a gentle pulse
    /// while the boost runs, blinking on and off in the last warning seconds, zero when it is not running.
    /// </summary>
    public sealed class ThunderAuraClock
    {
        private readonly ThunderAuraTiming _timing;
        private float _duration;
        private float _elapsed;

        public ThunderAuraClock(ThunderAuraTiming timing) { _timing = timing; }

        /// <summary>True while the boost is running.</summary>
        public bool IsActive => _duration > 0f && _elapsed < _duration;

        /// <summary>Seconds left; 0 when it is not running.</summary>
        public float Remaining => IsActive ? _duration - _elapsed : 0f;

        /// <summary>True in the last warning seconds.</summary>
        public bool InWarning => IsActive && Remaining <= Mathf.Min(_timing.WarningSeconds, _duration);

        /// <summary>The pulse wave, 0 to 1, from the time since the start.</summary>
        public float Pulse01 => 0.5f + 0.5f * Mathf.Sin(_elapsed * _timing.PulseHz * 2f * Mathf.PI);

        /// <summary>How strong the glow is, 0 to 1: 0 when not running, blinking 1 and 0 in the warning, else pulsing between the floor and 1.</summary>
        public float Strength
        {
            get
            {
                if (!IsActive) return 0f;
                if (!InWarning) return Mathf.Lerp(_timing.PulseFloor, 1f, Pulse01);
                var into = Mathf.Min(_timing.WarningSeconds, _duration) - Remaining;
                return (int)(into * _timing.BlinkHz * 2f) % 2 == 0 ? 1f : 0f;
            }
        }

        /// <summary>Starts (or restarts, when it is already running) the boost for <paramref name="seconds"/>.</summary>
        public void Start(float seconds)
        {
            _duration = Mathf.Max(0f, seconds);
            _elapsed = 0f;
        }

        /// <summary>Stops the glow at once.</summary>
        public void Clear() { _duration = 0f; _elapsed = 0f; }

        /// <summary>Moves time on by <paramref name="deltaSeconds"/>.</summary>
        public void Tick(float deltaSeconds)
        {
            if (IsActive) _elapsed += deltaSeconds;
        }
    }
}
