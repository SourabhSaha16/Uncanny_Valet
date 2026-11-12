using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// A decaying camera shake: two sines at unrelated speeds per axis, scaled by a falling envelope. Pure maths with the
    /// time passed in (use unscaled time so it works while time is frozen). A new shake only replaces a running one when
    /// it is at least as strong as what is left of it, so hits never stack above the cap.
    /// </summary>
    public struct CameraShake
    {
        /// <summary>Seeded strength: world units at the biggest hit.</summary>
        public const float DefaultStrength = 0.35f;
        /// <summary>Seeded length in seconds.</summary>
        public const float DefaultSeconds = 0.35f;
        /// <summary>Seeded impact force that gives the full strength.</summary>
        public const float DefaultFullForce = 8f;

        private const float FastHertz = 31f;
        private const float SlowHertz = 17f;
        private const float PhaseStep = 2.399963f; // golden angle: a new phase per shake without any random state

        private float _startedAt;
        private float _seconds;
        private float _amplitude;
        private float _phase;
        private bool _active;

        /// <summary>Writes the default shake numbers into the tuning store (Bootstrap calls this).</summary>
        public static void SeedDefaults(ITuning t)
        {
            t.Seed(TuningKey.ShakeStrength, DefaultStrength);
            t.Seed(TuningKey.ShakeSeconds, DefaultSeconds);
            t.Seed(TuningKey.ShakeFullForce, DefaultFullForce);
        }

        /// <summary>Shake size for a hit: zero at or below the damage limit, rising linearly to the strength at the full force.</summary>
        public static float Amplitude(float force, float minForce, float fullForce, float strength)
        {
            if (force <= minForce || strength <= 0f) return 0f;
            var span = Mathf.Max(fullForce - minForce, Mathf.Epsilon);
            return strength * Mathf.Clamp01((force - minForce) / span);
        }

        /// <summary>Starts a shake of this size (world units) and length; keeps the running one if it is still stronger.</summary>
        public void Start(float amplitude, float seconds, float now)
        {
            if (amplitude <= 0f || seconds <= 0f) return;
            if (_active && Envelope(now) * _amplitude > amplitude) return;
            _active = true;
            _amplitude = amplitude;
            _seconds = seconds;
            _startedAt = now;
            _phase += PhaseStep;
        }

        /// <summary>The camera offset at a time: never longer than the amplitude, zero once the length has passed.</summary>
        public Vector2 Offset(float now)
        {
            var e = Envelope(now);
            if (e <= 0f) return Vector2.zero;
            var t = now - _startedAt;
            var x = Mathf.Sin(t * FastHertz + _phase) + Mathf.Sin(t * SlowHertz + _phase * 2f);
            var y = Mathf.Sin(t * FastHertz * 0.8f + _phase * 3f) + Mathf.Sin(t * SlowHertz * 1.3f + _phase);
            return new Vector2(x, y) * (0.5f * _amplitude * e);
        }

        private float Envelope(float now)
        {
            if (!_active) return 0f;
            var left = 1f - (now - _startedAt) / _seconds;
            if (left <= 0f) { _active = false; return 0f; }
            return left * left;
        }
    }
}
