using UnityEngine;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Pure countdown in game minutes. Can be paused and resumed, expires exactly once, and can be stopped.
    /// Feed it the current minute on every call; it never reads a clock itself.
    /// </summary>
    public sealed class TicketTimer
    {
        private readonly float _length;
        private float _elapsed;
        private float _lastMinute;

        public TicketTimer(float lengthMinutes) { _length = lengthMinutes; }

        /// <summary>Started and not yet stopped (an expired timer stays active until stopped).</summary>
        public bool IsActive { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsExpired { get; private set; }

        /// <summary>0 = just started, 1 = out of time.</summary>
        public float Elapsed01 => _length <= 0f ? 1f : Mathf.Clamp01(_elapsed / _length);

        public void Start(float minute)
        {
            if (IsActive) return;
            IsActive = true;
            _lastMinute = minute;
        }

        public void Pause(float minute)
        {
            if (!IsActive || IsPaused) return;
            Advance(minute);
            IsPaused = true;
        }

        public void Resume(float minute)
        {
            if (!IsActive || !IsPaused) return;
            IsPaused = false;
            _lastMinute = minute;
        }

        /// <summary>Advances to this minute. Returns true only on the call where the timer runs out.</summary>
        public bool Tick(float minute)
        {
            if (!IsActive || IsExpired) return false;
            Advance(minute);
            if (_elapsed < _length) return false;
            IsExpired = true;
            return true;
        }

        /// <summary>Stops the timer. Returns true if it was active, so callers announce the stop once.</summary>
        public bool Stop()
        {
            var wasActive = IsActive;
            IsActive = false;
            return wasActive;
        }

        private void Advance(float minute)
        {
            if (!IsPaused) _elapsed += Mathf.Max(0f, minute - _lastMinute);
            _lastMinute = minute;
        }
    }
}
