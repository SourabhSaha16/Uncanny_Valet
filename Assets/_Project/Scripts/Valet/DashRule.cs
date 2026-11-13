using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>
    /// The on-foot dash as pure maths: a straight line covered at an even speed over the dash time. Each physics step it is
    /// told how far the body really moved since the last step and ends when the distance is covered, the time is up, or the
    /// body was held back (a solid thing in the way).
    /// </summary>
    public sealed class DashRule
    {
        /// <summary>A step that moved less than this share of what was asked counts as blocked.</summary>
        public const float BlockedShare = 0.5f;

        private const float Tolerance = 1e-4f;

        private Vector2 _direction;
        private float _speed;
        private float _distance;
        private float _seconds;
        private float _travelled;
        private float _elapsed;
        private float _lastAsked;

        /// <summary>True while the dash still moves the body.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Metres covered so far (the real movement the rule was told about).</summary>
        public float Travelled => _travelled;

        /// <summary>Starts a dash. A zero direction, distance or time starts nothing (it ends at once).</summary>
        public void Begin(Vector2 direction, float distance, float seconds)
        {
            _direction = direction.normalized;
            _distance = distance;
            _seconds = seconds;
            _speed = seconds > 0f ? distance / seconds : 0f;
            _travelled = 0f;
            _elapsed = 0f;
            _lastAsked = 0f;
            IsActive = direction.sqrMagnitude > 0f && distance > 0f && seconds > 0f;
        }

        /// <summary>Stops the dash.</summary>
        public void End() => IsActive = false;

        /// <summary>
        /// The velocity for the next physics step; zero once the dash ended (check <see cref="IsActive"/> after the call).
        /// <paramref name="moved"/> is how far the body travelled during the previous step.
        /// </summary>
        public Vector2 Step(float deltaTime, float moved)
        {
            if (!IsActive || deltaTime <= 0f) return Vector2.zero;
            _travelled += moved;
            var blocked = _lastAsked > 0f && moved < _lastAsked * BlockedShare;
            var left = _distance - _travelled;
            if (blocked || left <= Tolerance || _elapsed >= _seconds - Tolerance)
            {
                IsActive = false;
                return Vector2.zero;
            }
            var speed = Mathf.Min(_speed, left / deltaTime);
            _lastAsked = speed * deltaTime;
            _elapsed += deltaTime;
            return _direction * speed;
        }
    }
}
