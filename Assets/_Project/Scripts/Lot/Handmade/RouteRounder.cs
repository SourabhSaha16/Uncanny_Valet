using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>
    /// Rounds the corners of an authored polyline so a car can follow it: each interior corner is replaced by a quadratic curve that starts and
    /// ends up to <c>radius</c> before and after it (never more than half of either neighbouring leg). The first and last point stay where they are.
    /// </summary>
    public static class RouteRounder
    {
        private const float StraightDot = 0.9999f;

        /// <param name="points">The authored polyline.</param>
        /// <param name="radius">How far from a corner the curve begins; 0 or less copies the polyline.</param>
        /// <param name="stepsPerCorner">Segments per rounded corner (2 or more).</param>
        /// <param name="into">Receives the result (cleared first).</param>
        public static void Round(IReadOnlyList<Vector2> points, float radius, int stepsPerCorner, List<Vector2> into)
        {
            into.Clear();
            if (points.Count == 0) return;
            into.Add(points[0]);
            for (var i = 1; i + 1 < points.Count; i++)
            {
                if (radius <= 0f || IsStraight(points[i - 1], points[i], points[i + 1])) into.Add(points[i]);
                else Corner(points[i - 1], points[i], points[i + 1], radius, Mathf.Max(2, stepsPerCorner), into);
            }
            if (points.Count > 1) into.Add(points[points.Count - 1]);
        }

        private static bool IsStraight(Vector2 a, Vector2 p, Vector2 b)
        {
            var in1 = (p - a).normalized;
            var out1 = (b - p).normalized;
            return Vector2.Dot(in1, out1) > StraightDot;
        }

        private static void Corner(Vector2 a, Vector2 p, Vector2 b, float radius, int steps, List<Vector2> into)
        {
            var cut = Mathf.Min(radius, Vector2.Distance(a, p) * 0.5f, Vector2.Distance(p, b) * 0.5f);
            var start = p + (a - p).normalized * cut;
            var end = p + (b - p).normalized * cut;
            for (var s = 0; s <= steps; s++)
            {
                var t = s / (float)steps;
                var u = 1f - t;
                into.Add(u * u * start + 2f * u * t * p + t * t * end);
            }
        }
    }
}
