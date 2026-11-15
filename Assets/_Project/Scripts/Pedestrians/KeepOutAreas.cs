using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>Rectangles roaming people keep out of (the parking spots). Pure: no scene access, nothing allocated once the rectangles are added.</summary>
    public sealed class KeepOutAreas
    {
        private readonly List<Rect> _areas = new List<Rect>(32);

        /// <summary>How many rectangles are kept out of.</summary>
        public int Count => _areas.Count;

        /// <summary>Adds a rectangle in world space.</summary>
        public void Add(Rect area) => _areas.Add(area);

        /// <summary>Removes every rectangle.</summary>
        public void Clear() => _areas.Clear();

        /// <summary>True when the point lies inside any rectangle.</summary>
        public bool Contains(Vector2 point)
        {
            for (var i = 0; i < _areas.Count; i++)
                if (_areas[i].Contains(point)) return true;
            return false;
        }

        /// <summary>True when any straight piece of the walk touches a rectangle.</summary>
        public bool Crosses(IReadOnlyList<Vector2> walk)
        {
            if (_areas.Count == 0 || walk.Count == 0) return false;
            if (walk.Count == 1) return Contains(walk[0]);
            for (var i = 1; i < walk.Count; i++)
                for (var j = 0; j < _areas.Count; j++)
                    if (SegmentHits(_areas[j], walk[i - 1], walk[i])) return true;
            return false;
        }

        // Liang-Barsky clip of the segment against the rectangle.
        private static bool SegmentHits(Rect r, Vector2 a, Vector2 b)
        {
            var t0 = 0f;
            var t1 = 1f;
            var d = b - a;
            return Clip(-d.x, a.x - r.xMin, ref t0, ref t1) && Clip(d.x, r.xMax - a.x, ref t0, ref t1)
                && Clip(-d.y, a.y - r.yMin, ref t0, ref t1) && Clip(d.y, r.yMax - a.y, ref t0, ref t1);
        }

        private static bool Clip(float p, float q, ref float t0, ref float t1)
        {
            if (Mathf.Approximately(p, 0f)) return q >= 0f;
            var t = q / p;
            if (p < 0f)
            {
                if (t > t1) return false;
                if (t > t0) t0 = t;
            }
            else
            {
                if (t < t0) return false;
                if (t < t1) t1 = t;
            }
            return true;
        }
    }
}
