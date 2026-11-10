using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Pure rule for the tiny rubber-ball bounce a pedestrian does when a car or the valet bumps them: a short push away
    /// from the bumper that eases out quickly and settles back to the route point (so the route is never disturbed).
    /// A cooldown stops a car that keeps touching them from re-triggering every step.
    /// </summary>
    public sealed class PedestrianBounceRule
    {
        private float _distance = 0.2f;
        private float _seconds = 0.25f;
        private float _cooldown = 0.6f;
        private Vector2 _direction;
        private float _strength;
        private float _elapsed;
        private float _cooldownLeft;

        /// <summary>True while the bounce plays.</summary>
        public bool Active { get; private set; }

        /// <summary>Current displacement from the route point (zero when not bouncing).</summary>
        public Vector2 Offset { get; private set; }

        /// <summary>Sets the peak distance, bounce length and the pause before another bounce may start.</summary>
        public void Configure(float distance, float seconds, float cooldown)
        {
            _distance = Mathf.Max(0f, distance);
            _seconds = Mathf.Max(0.01f, seconds);
            _cooldown = Mathf.Max(0f, cooldown);
        }

        /// <summary>Bump strength 0..1 from the impact speed: zero below <paramref name="minSpeed"/>, one at <paramref name="fullSpeed"/> and above.</summary>
        public static float Strength(float speed, float minSpeed, float fullSpeed)
            => speed < minSpeed || fullSpeed <= 0f ? 0f : Mathf.Clamp01(speed / fullSpeed);

        /// <summary>Displacement shape for progress 0..1: leaves fast (ease-out), peaks early, settles back to 0.</summary>
        public static float Shape(float progress)
        {
            var u = Mathf.Clamp01(progress);
            var eased = 1f - (1f - u) * (1f - u);
            return Mathf.Sin(Mathf.PI * eased);
        }

        /// <summary>Starts a bounce away along <paramref name="away"/>. False while bouncing, cooling down, or when the strength is zero.</summary>
        public bool TryStart(Vector2 away, float strength01)
        {
            if (Active || _cooldownLeft > 0f || strength01 <= 0f || away.sqrMagnitude < Mathf.Epsilon) return false;
            _direction = away.normalized;
            _strength = Mathf.Clamp01(strength01);
            _elapsed = 0f;
            _cooldownLeft = _cooldown + _seconds;
            Active = true;
            return true;
        }

        /// <summary>Advances the bounce and the cooldown by one step.</summary>
        public void Step(float deltaSeconds)
        {
            if (_cooldownLeft > 0f) _cooldownLeft -= deltaSeconds;
            if (!Active) return;
            _elapsed += deltaSeconds;
            Active = _elapsed < _seconds;
            Offset = Active ? _direction * (_distance * _strength * Shape(_elapsed / _seconds)) : Vector2.zero;
        }
    }
}
