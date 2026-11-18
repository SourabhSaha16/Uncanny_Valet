using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Pure one-way path following along a polyline. Copies the route into a reusable buffer when a walk begins, so
    /// starting a walk allocates nothing once the buffer has grown to the longest route.
    /// </summary>
    public sealed class PathFollower
    {
        private const float StartLimit = 0.99f;

        private readonly List<Vector2> _points = new List<Vector2>(12);
        private int _next = 1;
        private Vector2 _position;
        private Vector2 _heading;

        /// <summary>True while a walk is in progress (begun and not yet finished).</summary>
        public bool Walking { get; private set; }

        public Vector2 Position => _position;

        /// <summary>Unit direction of the current segment; zero before any walk.</summary>
        public Vector2 Heading => _heading;

        /// <summary>Total length of the route that was begun.</summary>
        public float Length { get; private set; }

        /// <summary>Starts a walk along <paramref name="route"/> (copied), starting <paramref name="startFraction"/> (0..1) of the way along it. A route of one point only places the walker.</summary>
        public void Begin(List<Vector2> route, float startFraction = 0f)
        {
            _points.Clear();
            _points.AddRange(route);
            _heading = Vector2.zero;
            Length = 0f;
            for (var i = 1; i < _points.Count; i++) Length += Vector2.Distance(_points[i - 1], _points[i]);
            _position = _points.Count > 0 ? _points[0] : Vector2.zero;
            _next = 1;
            Walking = _points.Count >= 2;
            if (Walking) Advance(Mathf.Clamp01(startFraction) * Length * StartLimit);
        }

        /// <summary>Distance still to walk along the route (zero when not walking).</summary>
        public float Remaining
        {
            get
            {
                if (!Walking || _next >= _points.Count) return 0f;
                var left = Vector2.Distance(_position, _points[_next]);
                for (var i = _next + 1; i < _points.Count; i++) left += Vector2.Distance(_points[i - 1], _points[i]);
                return left;
            }
        }

        /// <summary>
        /// Puts the walker at <paramref name="position"/> (knocked or pushed there). A walk in progress then heads straight for its
        /// final point from there, with no return to the route; a standing walker just stays at the new spot.
        /// </summary>
        public void Displace(Vector2 position)
        {
            _position = position;
            if (!Walking) return;
            var destination = _points[_points.Count - 1];
            _points.Clear();
            _points.Add(position);
            _points.Add(destination);
            _next = 1;
        }

        /// <summary>
        /// The point <paramref name="distance"/> further along the route from the walker, following its corners (the last point when the
        /// route is shorter). The walker's own position when not walking. Lets a look-ahead see round a corner.
        /// </summary>
        public Vector2 PointAhead(float distance)
        {
            if (!Walking) return _position;
            var from = _position;
            for (var i = _next; i < _points.Count; i++)
            {
                var seg = Vector2.Distance(from, _points[i]);
                if (distance <= seg && seg > Mathf.Epsilon) return Vector2.Lerp(from, _points[i], distance / seg);
                distance -= seg;
                from = _points[i];
            }
            return from;
        }

        /// <summary>Stops walking and keeps the current position.</summary>
        public void Stop() => Walking = false;

        /// <summary>Walks forward by a distance. Returns true on the call that reaches the end of the route.</summary>
        public bool Advance(float distance)
        {
            if (!Walking) return false;
            while (distance > 0f && _next < _points.Count)
            {
                var target = _points[_next];
                var remaining = Vector2.Distance(_position, target);
                if (remaining > Mathf.Epsilon) _heading = (target - _position) / remaining;
                if (distance < remaining)
                {
                    _position += _heading * distance;
                    return false;
                }
                distance -= remaining;
                _position = target;
                _next++;
            }
            if (_next < _points.Count) return false;
            Walking = false;
            return true;
        }
    }
}
