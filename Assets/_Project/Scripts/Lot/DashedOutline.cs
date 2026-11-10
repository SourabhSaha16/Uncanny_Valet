using System;
using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Pure maths: the dash rectangles of a dotted (dashed) rectangle outline. Each edge starts and ends on a dash.</summary>
    public static class DashedOutline
    {
        /// <summary>Dashes along the border of <paramref name="box"/>, drawn inside it; the corners are shared, never doubled.</summary>
        public static List<Rect> Dashes(Rect box, float thickness, float dashLength, float gapLength)
        {
            var dashes = new List<Rect>();
            var t = Mathf.Min(thickness, Mathf.Min(box.width, box.height) * 0.5f);
            Along(dashes, box.xMin, box.xMax, dashLength, gapLength, (a, b) => Rect.MinMaxRect(a, box.yMax - t, b, box.yMax));
            Along(dashes, box.xMin, box.xMax, dashLength, gapLength, (a, b) => Rect.MinMaxRect(a, box.yMin, b, box.yMin + t));
            Along(dashes, box.yMin + t, box.yMax - t, dashLength, gapLength, (a, b) => Rect.MinMaxRect(box.xMin, a, box.xMin + t, b));
            Along(dashes, box.yMin + t, box.yMax - t, dashLength, gapLength, (a, b) => Rect.MinMaxRect(box.xMax - t, a, box.xMax, b));
            return dashes;
        }

        /// <summary>How many dashes fit a run of the given length (at least one).</summary>
        public static int Count(float length, float dashLength, float gapLength)
            => Mathf.Max(1, Mathf.RoundToInt((length + gapLength) / Mathf.Max(0.001f, dashLength + gapLength)));

        private static void Along(List<Rect> into, float from, float to, float dash, float gap, Func<float, float, Rect> make)
        {
            var length = to - from;
            if (length <= 0f) return;
            var count = Count(length, dash, gap);
            var size = Mathf.Max(0.001f, (length - (count - 1) * gap) / count);
            var stride = count > 1 ? (length - size) / (count - 1) : 0f;
            for (var i = 0; i < count; i++) into.Add(make(from + stride * i, from + stride * i + size));
        }
    }
}
