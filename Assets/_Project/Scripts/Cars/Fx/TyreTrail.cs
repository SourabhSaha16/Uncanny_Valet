using UnityEngine;

namespace UncannyValet.Cars.Fx
{
    /// <summary>
    /// Follows one tyre and cuts its path into strokes at a fixed distance, not a fixed time. Each stroke runs from where the
    /// tyre was to where it is now, so it lies along the tyre's real travel direction.
    /// </summary>
    public sealed class TyreTrail
    {
        private bool _has;
        private Vector2 _last;

        /// <summary>Forgets the last position: the next point only starts a new trail (call when the slide ends).</summary>
        public void Break() => _has = false;

        /// <summary>
        /// Moves the tyre to <paramref name="now"/>. True with a stroke once it has travelled at least <paramref name="spacing"/>;
        /// the stroke is <paramref name="overlap"/> longer so neighbours join without gaps.
        /// </summary>
        public bool TryAdvance(Vector2 now, float spacing, float overlap, out MarkSegment segment)
        {
            segment = default;
            if (!_has) { _has = true; _last = now; return false; }
            var delta = now - _last;
            var distance = delta.magnitude;
            if (distance < spacing) return false;
            var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90f;
            segment = new MarkSegment((_last + now) * 0.5f, angle, distance + overlap);
            _last = now;
            return true;
        }
    }
}
