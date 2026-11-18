using UnityEngine;

namespace UncannyValet.Cars.Fx
{
    /// <summary>
    /// Follows one tyre and cuts its path into strokes. A stroke is cut after the tyre travelled the spacing, or sooner when the
    /// path bends by the max bend angle (so curves are many short chords, straights stay long). Each stroke runs from where the
    /// tyre was to where it is now, so it lies along the tyre's real travel direction.
    /// </summary>
    public sealed class TyreTrail
    {
        private bool _has;
        private bool _hasPrevious;
        private Vector2 _last;
        private Vector2 _frame;
        private Vector2 _step;
        private Vector2 _previousDirection;

        /// <summary>Forgets the last position: the next point only starts a new trail (call when the slide ends).</summary>
        public void Break()
        {
            _has = false;
            _hasPrevious = false;
        }

        /// <summary>
        /// Moves the tyre to <paramref name="now"/>. True with a stroke once it has travelled at least <paramref name="spacing"/>,
        /// or at least <paramref name="minLength"/> while the path has bent by <paramref name="maxBendDegrees"/>. The stroke is
        /// <paramref name="overlap"/> longer so neighbours join without gaps.
        /// </summary>
        public bool TryAdvance(Vector2 now, float spacing, float overlap, float maxBendDegrees, float minLength,
            out MarkSegment segment)
        {
            segment = default;
            if (!_has) { _has = true; _last = now; _frame = now; _step = Vector2.zero; return false; }
            _step = now - _frame;
            _frame = now;
            var delta = now - _last;
            var distance = delta.magnitude;
            var cut = distance >= spacing || (distance >= minLength && Bend(delta) >= maxBendDegrees);
            if (!cut) return false;
            var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90f;
            segment = new MarkSegment((_last + now) * 0.5f, angle, distance + overlap);
            _previousDirection = delta;
            _hasPrevious = true;
            _last = now;
            return true;
        }

        private float Bend(Vector2 chord)
        {
            if (_hasPrevious) return Vector2.Angle(_previousDirection, chord);
            return _step.sqrMagnitude > 0f ? 2f * Vector2.Angle(_step, chord) : 0f;
        }
    }
}
