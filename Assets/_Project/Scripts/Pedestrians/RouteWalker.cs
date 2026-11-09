using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Pure path following along an open polyline: walks to the last point, turns round and walks back, forever.
    /// Allocation-free. The route array is read, never changed.
    /// </summary>
    public sealed class RouteWalker
    {
        private readonly Vector2[] _points;
        private int _from;
        private int _to = 1;
        private int _direction = 1;
        private float _along;
        private float _segmentLength;

        public RouteWalker(Vector2[] points)
        {
            _points = points ?? new Vector2[0];
            Length = MeasureLength(_points);
            if (_points.Length >= 2) _segmentLength = Vector2.Distance(_points[0], _points[1]);
        }

        /// <summary>Length of the route in one direction.</summary>
        public float Length { get; }

        /// <summary>Length of a full walk there and back.</summary>
        public float CycleLength => Length * 2f;

        /// <summary>True when the route has at least two points, so there is somewhere to walk.</summary>
        public bool CanWalk => _points.Length >= 2;

        /// <summary>Where the walker is now.</summary>
        public Vector2 Position
        {
            get
            {
                if (!CanWalk) return _points.Length == 1 ? _points[0] : Vector2.zero;
                var t = _segmentLength > 0f ? _along / _segmentLength : 1f;
                return Vector2.Lerp(_points[_from], _points[_to], t);
            }
        }

        /// <summary>Unit direction of travel on the current segment (zero when there is no route).</summary>
        public Vector2 Heading => CanWalk ? (_points[_to] - _points[_from]).normalized : Vector2.zero;

        /// <summary>Walks <paramref name="distance"/> along the route. Negative or zero distances do nothing.</summary>
        public void Advance(float distance)
        {
            if (!CanWalk || distance <= 0f) return;
            var guard = _points.Length * 2 + 4; // stops zero-length segments from spinning forever
            while (distance > 0f && guard-- > 0)
            {
                var remaining = _segmentLength - _along;
                if (distance < remaining)
                {
                    _along += distance;
                    return;
                }
                distance -= remaining;
                Turn();
            }
        }

        /// <summary>Jumps forward by a distance within one full cycle, so walkers sharing a route can be spread out.</summary>
        public void StartAt(float distance)
        {
            if (CycleLength > 0f) Advance(distance % CycleLength);
        }

        private void Turn()
        {
            _from = _to;
            if (_to + _direction < 0 || _to + _direction >= _points.Length) _direction = -_direction;
            _to = _from + _direction;
            _along = 0f;
            _segmentLength = Vector2.Distance(_points[_from], _points[_to]);
        }

        private static float MeasureLength(Vector2[] points)
        {
            var total = 0f;
            for (var i = 1; i < points.Length; i++) total += Vector2.Distance(points[i - 1], points[i]);
            return total;
        }
    }
}
