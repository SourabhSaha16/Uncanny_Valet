using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Pure maths: evenly spaced marks along a polyline route, each pointing along the direction of travel (turns included).</summary>
    public static class RouteMarks
    {
        /// <summary>
        /// Marks along <paramref name="route"/> starting <paramref name="startDistance"/> from its first point, one every
        /// <paramref name="spacing"/>. Each mark points along the segment it sits on.
        /// </summary>
        public static List<RouteMark> Along(IReadOnlyList<Vector2> route, float startDistance, float spacing)
        {
            var marks = new List<RouteMark>();
            if (spacing <= 0f) return marks;
            var next = Mathf.Max(0f, startDistance);
            for (var i = 1; i < route.Count; i++)
            {
                var from = route[i - 1];
                var to = route[i];
                var length = Vector2.Distance(from, to);
                if (length <= Mathf.Epsilon) continue;
                var direction = (to - from) / length;
                for (; next <= length; next += spacing) marks.Add(new RouteMark(from + direction * next, direction));
                next -= length;
            }
            return marks;
        }
    }
}
