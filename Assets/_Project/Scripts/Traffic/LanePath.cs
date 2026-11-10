using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>
    /// A world-space polyline measured by distance. Gives position and heading at a distance along it.
    /// Rebuild reuses its buffers, so it only allocates when a route gets longer than any before.
    /// </summary>
    public sealed class LanePath
    {
        private const int InitialCapacity = 8;
        private const float QuarterTurnDegrees = 90f;

        private Vector2[] _points = new Vector2[InitialCapacity];
        private float[] _cumulative = new float[InitialCapacity];
        private int _count;

        /// <summary>Total length of the polyline (0 when it has fewer than two points).</summary>
        public float Length => _count > 1 ? _cumulative[_count - 1] : 0f;

        /// <summary>Replaces the polyline with <paramref name="points"/>.</summary>
        public void Rebuild(IReadOnlyList<Vector2> points)
        {
            _count = points.Count;
            if (_points.Length < _count)
            {
                _points = new Vector2[_count];
                _cumulative = new float[_count];
            }
            for (int i = 0; i < _count; i++)
            {
                _points[i] = points[i];
                _cumulative[i] = i == 0 ? 0f : _cumulative[i - 1] + Vector2.Distance(_points[i - 1], _points[i]);
            }
        }

        /// <summary>Position at <paramref name="distance"/>, clamped to the ends.</summary>
        public Vector2 PositionAt(float distance)
        {
            if (_count == 0) return Vector2.zero;
            if (_count == 1) return _points[0];
            int segment = SegmentAt(distance);
            float length = _cumulative[segment + 1] - _cumulative[segment];
            float t = length > 0f ? Mathf.Clamp01((distance - _cumulative[segment]) / length) : 0f;
            return Vector2.Lerp(_points[segment], _points[segment + 1], t);
        }

        /// <summary>Heading at <paramref name="distance"/> in degrees, for a sprite drawn nose up (transform.up points along the path).</summary>
        public float HeadingAt(float distance)
        {
            if (_count < 2) return 0f;
            int segment = SegmentAt(distance);
            Vector2 direction = _points[segment + 1] - _points[segment];
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - QuarterTurnDegrees;
        }

        private int SegmentAt(float distance)
        {
            int last = _count - 2;
            for (int i = 0; i < last; i++)
            {
                if (distance < _cumulative[i + 1]) return i;
            }
            return last;
        }
    }
}
