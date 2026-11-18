using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>Pure polyline helpers shared by the authored layers and the validator.</summary>
    public static class RouteMath
    {
        /// <summary>Distance from <paramref name="p"/> to the segment a-b.</summary>
        public static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            return Vector2.Distance(p, ClosestOnSegment(p, a, b));
        }

        /// <summary>The point of the segment a-b nearest to <paramref name="p"/>.</summary>
        public static Vector2 ClosestOnSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            var len = ab.sqrMagnitude;
            if (len < 1e-8f) return a;
            return a + ab * Mathf.Clamp01(Vector2.Dot(p - a, ab) / len);
        }

        /// <summary>Distance from <paramref name="p"/> to the polyline; float.MaxValue when it has no points.</summary>
        public static float DistanceToPolyline(Vector2 p, IReadOnlyList<Vector2> line)
        {
            if (line.Count == 0) return float.MaxValue;
            if (line.Count == 1) return Vector2.Distance(p, line[0]);
            var best = float.MaxValue;
            for (var i = 0; i + 1 < line.Count; i++) best = Mathf.Min(best, DistanceToSegment(p, line[i], line[i + 1]));
            return best;
        }

        /// <summary>Distance travelled along the polyline from its first point to the point nearest <paramref name="p"/>.</summary>
        public static float DistanceAlong(Vector2 p, IReadOnlyList<Vector2> line)
        {
            var best = float.MaxValue;
            var along = 0f;
            var walked = 0f;
            for (var i = 0; i + 1 < line.Count; i++)
            {
                var onSegment = ClosestOnSegment(p, line[i], line[i + 1]);
                var gap = Vector2.Distance(p, onSegment);
                if (gap < best)
                {
                    best = gap;
                    along = walked + Vector2.Distance(line[i], onSegment);
                }
                walked += Vector2.Distance(line[i], line[i + 1]);
            }
            return along;
        }

        /// <summary>True when any segment of the polyline passes through the rectangle.</summary>
        public static bool Crosses(IReadOnlyList<Vector2> line, OrientedRect solid)
        {
            for (var i = 0; i + 1 < line.Count; i++)
                if (solid.IntersectsSegment(line[i], line[i + 1])) return true;
            return false;
        }
    }
}
