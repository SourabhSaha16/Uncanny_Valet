using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>Pure polyline maths for roaming: a person's place on a route is a fraction (0..1) of its length.</summary>
    public static class RoamPath
    {
        /// <summary>Total length of the polyline.</summary>
        public static float Length(IReadOnlyList<Vector2> route)
        {
            var total = 0f;
            for (var i = 1; i < route.Count; i++) total += Vector2.Distance(route[i - 1], route[i]);
            return total;
        }

        /// <summary>The point <paramref name="fraction"/> (clamped to 0..1) of the way along the route.</summary>
        public static Vector2 PointAt(IReadOnlyList<Vector2> route, float fraction)
        {
            if (route.Count == 0) return Vector2.zero;
            var left = Mathf.Clamp01(fraction) * Length(route);
            for (var i = 1; i < route.Count; i++)
            {
                var seg = Vector2.Distance(route[i - 1], route[i]);
                if (left <= seg && seg > 0f) return Vector2.Lerp(route[i - 1], route[i], left / seg);
                left -= seg;
            }
            return route[route.Count - 1];
        }

        /// <summary>
        /// Writes the walk from <paramref name="from"/> to <paramref name="to"/> (fractions of the route, either order) into
        /// <paramref name="into"/> (cleared first): both end points and every route corner between them.
        /// </summary>
        public static void Slice(IReadOnlyList<Vector2> route, float from, float to, List<Vector2> into)
        {
            into.Clear();
            if (route.Count == 0) return;
            var total = Length(route);
            var low = Mathf.Min(from, to) * total;
            var high = Mathf.Max(from, to) * total;
            into.Add(PointAt(route, from));
            var cumulative = 0f;
            for (var i = 1; i < route.Count - 1; i++)
            {
                cumulative += Vector2.Distance(route[i - 1], route[i]);
                if (cumulative > low && cumulative < high) into.Add(route[i]);
            }
            if (to < from && into.Count > 2) into.Reverse(1, into.Count - 1);
            into.Add(PointAt(route, to));
        }
    }
}
