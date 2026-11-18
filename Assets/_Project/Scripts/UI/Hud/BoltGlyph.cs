using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>A lightning bolt drawn procedurally from pixel rows, with a one pixel dark outline. No art file.</summary>
    public static class BoltGlyph
    {
        /// <summary>Width of the bolt without its outline.</summary>
        public const int Width = 7;

        /// <summary>Height of the bolt without its outline.</summary>
        public const int Height = 10;

        // Per row: first column and run length. Together they trace a zig-zag bolt.
        private static readonly int[] Starts = { 3, 2, 2, 1, 3, 3, 2, 2, 1, 1 };
        private static readonly int[] Lengths = { 4, 4, 3, 6, 4, 3, 3, 2, 2, 1 };

        /// <summary>Pixels the bolt covers (rows times runs), for tests.</summary>
        public static int PixelCount
        {
            get
            {
                var count = 0;
                for (var row = 0; row < Height; row++) count += Lengths[row];
                return count;
            }
        }

        /// <summary>Draws the bolt with its top left corner at (x, y) in the given colour on a dark outline; alpha fades both.</summary>
        public static void Draw(PixelCanvas canvas, int x, int y, Color color, float alpha = 1f)
        {
            var ink = PixelPalette.Ink;
            ink.a *= alpha;
            color.a *= alpha;
            for (var row = 0; row < Height; row++)
            {
                canvas.Fill(x + Starts[row] - 1, y + row, Lengths[row] + 2, 1, ink);
                canvas.Fill(x + Starts[row], y + row - 1, Lengths[row], 1, ink);
                canvas.Fill(x + Starts[row], y + row + 1, Lengths[row], 1, ink);
            }
            for (var row = 0; row < Height; row++) canvas.Fill(x + Starts[row], y + row, Lengths[row], 1, color);
        }

        /// <summary>
        /// Draws the bolt magnified by <paramref name="scale"/> with its top left corner (outline included) at (x, y): the bottom
        /// <paramref name="litRows"/> rows in <paramref name="lit"/>, the rest in <paramref name="empty"/>, on a dark outline.
        /// </summary>
        public static void DrawFilled(PixelCanvas canvas, int x, int y, int scale, int litRows, Color lit, Color empty)
        {
            var ink = PixelPalette.Ink;
            for (var row = 0; row < Height; row++)
            {
                var top = y + (row + 1) * scale;
                var left = x + (Starts[row] + 1) * scale;
                canvas.Fill(left - scale, top, (Lengths[row] + 2) * scale, scale, ink);
                canvas.Fill(left, top - scale, Lengths[row] * scale, scale, ink);
                canvas.Fill(left, top + scale, Lengths[row] * scale, scale, ink);
            }
            for (var row = 0; row < Height; row++)
                canvas.Fill(x + (Starts[row] + 1) * scale, y + (row + 1) * scale, Lengths[row] * scale, scale, row >= Height - litRows ? lit : empty);
        }
    }
}
