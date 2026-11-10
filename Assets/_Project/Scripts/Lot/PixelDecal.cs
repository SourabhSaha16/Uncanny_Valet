using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Paints a pixel-art icon bitmap (the same one the HUD uses) from square pixels, so it stays crisp at any zoom.
    /// Used under the number of each charging bay and under the text of the wash slot.
    /// </summary>
    public static class PixelDecal
    {
        /// <summary>Size of one icon pixel in world units; small, like the plug under the bay numbers.</summary>
        public const float PixelSize = 0.07f;
        /// <summary>Vertical position of the icon centre, as a share of the cell height from its centre (negative = below).</summary>
        public const float YOffsetShare = -0.33f;
        /// <summary>Sorting order of the icon, above the cell fill and its label.</summary>
        public const int Order = -4;

        /// <summary>Maps a bitmap key to its colour; false for a clear key.</summary>
        public delegate bool KeyColor(char key, out Color32 color);

        /// <summary>Builds the decal centred on <paramref name="center"/>; one rectangle per horizontal run of one colour.</summary>
        public static void Build(RectFactory rects, Transform parent, Vector2 center, string[] bitmap, KeyColor colors)
        {
            var width = bitmap[0].Length;
            var left = center.x - width * PixelSize * 0.5f;
            var top = center.y + bitmap.Length * PixelSize * 0.5f;
            for (var row = 0; row < bitmap.Length; row++)
            {
                var x = 0;
                while (x < width) x += Run(rects, parent, bitmap[row], x, row, left, top, colors);
            }
        }

        // Draws the run of equal keys starting at x and returns its length.
        private static int Run(RectFactory rects, Transform parent, string line, int x, int row, float left, float top,
            KeyColor colors)
        {
            var end = x;
            while (end < line.Length && line[end] == line[x]) end++;
            var length = end - x;
            if (!colors(line[x], out var color)) return length;
            var size = new Vector2(length * PixelSize, PixelSize);
            var at = new Vector2(left + (x + length * 0.5f) * PixelSize, top - (row + 0.5f) * PixelSize);
            rects.Make(parent, "Icon " + row + "." + x, at, size, color, Order);
            return length;
        }
    }
}
