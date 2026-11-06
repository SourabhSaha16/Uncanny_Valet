using System;

namespace UncannyValet.Audio
{
    /// <summary>
    /// Maps the number of waiting cars to a smoothed music tempo multiplier: no cars is the minimum,
    /// the configured count reaches the maximum, and the value eases toward its target over time.
    /// </summary>
    public sealed class UrgencyRule
    {
        private readonly float _min;
        private readonly float _max;
        private readonly int _pendingForMax;
        private readonly float _smoothingSeconds;

        public UrgencyRule(float minMultiplier, float maxMultiplier, int pendingForMax, float smoothingSeconds)
        {
            _min = minMultiplier;
            _max = Math.Max(minMultiplier, maxMultiplier);
            _pendingForMax = Math.Max(1, pendingForMax);
            _smoothingSeconds = Math.Max(0f, smoothingSeconds);
            Current = _min;
        }

        /// <summary>The smoothed multiplier right now.</summary>
        public float Current { get; private set; }

        /// <summary>The multiplier the rule is heading for with this many cars waiting.</summary>
        public float Target(int pending)
        {
            float t = Math.Min(Math.Max(pending, 0), _pendingForMax) / (float)_pendingForMax;
            return _min + (_max - _min) * t;
        }

        /// <summary>Eases <see cref="Current"/> toward the target and returns it. Allocation-free.</summary>
        public float Step(int pending, float deltaSeconds)
        {
            float target = Target(pending);
            if (_smoothingSeconds <= 0f) Current = target;
            else if (deltaSeconds > 0f)
                Current += (target - Current) * (1f - (float)Math.Exp(-deltaSeconds / _smoothingSeconds));
            return Current;
        }
    }
}
