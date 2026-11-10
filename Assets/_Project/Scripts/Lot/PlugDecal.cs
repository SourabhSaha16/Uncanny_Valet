using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Paints the plug icon (the same bitmap as the HUD info box) from square pixels, so it stays crisp at any zoom.
    /// Used under the number of each charging bay.
    /// </summary>
    public static class PlugDecal
    {
        /// <summary>Builds the decal centred on <paramref name="center"/>; one rectangle per horizontal run of one colour.</summary>
        public static void Build(RectFactory rects, Transform parent, Vector2 center, float pixelSize, int order)
        {
            var rows = PlugIconArt.Rows;
            var width = rows[0].Length;
            var left = center.x - width * pixelSize * 0.5f;
            var top = center.y + rows.Length * pixelSize * 0.5f;
            for (var row = 0; row < rows.Length; row++)
            {
                var x = 0;
                while (x < width) x += Run(rects, parent, rows[row], x, row, left, top, pixelSize, order);
            }
        }

        // Draws the run of equal keys starting at x and returns its length.
        private static int Run(RectFactory rects, Transform parent, string line, int x, int row, float left, float top,
            float pixelSize, int order)
        {
            var end = x;
            while (end < line.Length && line[end] == line[x]) end++;
            var length = end - x;
            if (!PlugIconArt.TryColor(line[x], out var color)) return length;
            var size = new Vector2(length * pixelSize, pixelSize);
            var at = new Vector2(left + (x + length * 0.5f) * pixelSize, top - (row + 0.5f) * pixelSize);
            rects.Make(parent, "Plug " + row + "." + x, at, size, color, order);
            return length;
        }
    }
}
