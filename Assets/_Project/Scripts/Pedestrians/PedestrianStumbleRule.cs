using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Pure rule for the stumble of a pedestrian who is knocked over: a stronger, longer version of the bounce
    /// (a bigger push away from whatever hit them) with a spin, that settles back to the route point.
    /// </summary>
    public sealed class PedestrianStumbleRule
    {
        private float _distance = 0.7f;
        private float _spins = 1f;
        private float _seconds;
        private float _elapsed;
        private Vector2 _direction;

        /// <summary>True while the stumble plays.</summary>
        public bool Active { get; private set; }

        /// <summary>Current displacement from the route point (zero when not stumbling).</summary>
        public Vector2 Offset { get; private set; }

        /// <summary>Current spin of the sprites in degrees (zero when not stumbling).</summary>
        public float SpinDegrees { get; private set; }

        /// <summary>Sets the peak push distance (world units) and how many full turns the person spins.</summary>
        public void Configure(float distance, float spins)
        {
            _distance = Mathf.Max(0f, distance);
            _spins = Mathf.Max(0f, spins);
        }

        /// <summary>Starts a stumble away along <paramref name="push"/> lasting <paramref name="seconds"/>. False while already stumbling or when the push or time is empty.</summary>
        public bool TryStart(Vector2 push, float seconds)
        {
            if (Active || seconds <= 0f || push.sqrMagnitude < Mathf.Epsilon) return false;
            _direction = push.normalized;
            _seconds = seconds;
            _elapsed = 0f;
            Active = true;
            return true;
        }

        /// <summary>Advances the stumble by one step.</summary>
        public void Step(float deltaSeconds)
        {
            if (!Active) return;
            _elapsed += deltaSeconds;
            Active = _elapsed < _seconds;
            var progress = Mathf.Clamp01(_elapsed / _seconds);
            Offset = Active ? _direction * (_distance * PedestrianBounceRule.Shape(progress)) : Vector2.zero;
            SpinDegrees = Active ? 360f * _spins * progress : 0f;
        }
    }
}
