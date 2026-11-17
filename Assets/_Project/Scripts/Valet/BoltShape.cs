using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>Pure maths for a jagged lightning streak: points along a line pushed sideways by given amounts, snapped to the pixel grid.</summary>
    public static class BoltShape
    {
        /// <summary>
        /// Fills <paramref name="points"/> (at least 2) from <paramref name="from"/> to <paramref name="to"/>. The points between
        /// the ends are pushed sideways by <paramref name="jag"/> times each value of <paramref name="offsets"/> (-1..1, one per
        /// inner point), and every point is snapped to 1/<paramref name="pixelsPerUnit"/>.
        /// </summary>
        public static void Layout(Vector2 from, Vector2 to, float jag, float[] offsets, float pixelsPerUnit, Vector2[] points)
        {
            var last = points.Length - 1;
            var side = new Vector2(-(to.y - from.y), to.x - from.x).normalized * jag;
            points[0] = Snap(from, pixelsPerUnit);
            points[last] = Snap(to, pixelsPerUnit);
            for (var i = 1; i < last; i++)
            {
                var along = Vector2.Lerp(from, to, i / (float)last);
                var offset = offsets != null && i - 1 < offsets.Length ? offsets[i - 1] : 0f;
                points[i] = Snap(along + side * offset, pixelsPerUnit);
            }
        }

        /// <summary>Rounds a position to the pixel grid.</summary>
        public static Vector2 Snap(Vector2 p, float pixelsPerUnit) =>
            new Vector2(Mathf.Round(p.x * pixelsPerUnit) / pixelsPerUnit, Mathf.Round(p.y * pixelsPerUnit) / pixelsPerUnit);

        /// <summary>Opacity of a streak that has lived <paramref name="age"/> of <paramref name="lifetime"/> seconds: 1 at birth, 0 at the end.</summary>
        public static float Fade(float age, float lifetime) => lifetime <= 0f ? 0f : Mathf.Clamp01(1f - age / lifetime);
    }
}
