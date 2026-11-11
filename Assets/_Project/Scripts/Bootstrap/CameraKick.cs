using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>A short camera nudge: out along a direction and back again, as a half sine. Pure maths, time passed in.</summary>
    public struct CameraKick
    {
        private Vector2 _direction;
        private float _startedAt;
        private float _seconds;
        private float _distance;
        private bool _active;

        /// <summary>Starts a kick; a new one replaces any running kick. A zero direction, distance or time does nothing.</summary>
        public void Start(Vector2 direction, float distance, float seconds, float now)
        {
            _active = direction.sqrMagnitude > 0f && distance > 0f && seconds > 0f;
            _direction = direction.normalized;
            _distance = distance;
            _seconds = seconds;
            _startedAt = now;
        }

        /// <summary>The camera offset at a time: zero before and after the kick, the full distance at its midpoint.</summary>
        public Vector2 Offset(float now)
        {
            if (!_active) return Vector2.zero;
            var t = (now - _startedAt) / _seconds;
            if (t >= 1f) _active = false;
            if (t < 0f || t >= 1f) return Vector2.zero;
            return _direction * (_distance * Mathf.Sin(t * Mathf.PI));
        }
    }
}
