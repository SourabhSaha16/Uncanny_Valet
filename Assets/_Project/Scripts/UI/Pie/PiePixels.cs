using System;

namespace UncannyValet.UI.Pie
{
    /// <summary>Pure pixel masks of the pie: which pixel of a square sprite is part of the disc, its outline ring or the wedge.</summary>
    public static class PiePixels
    {
        /// <summary>Width of the dark outline ring in pixels.</summary>
        public const float Outline = 1.5f;

        /// <summary>True when the pixel centre lies inside a circle of the given diameter filling the square.</summary>
        public static bool InDisc(int x, int y, int size) => Distance(x, y, size) <= size / 2f;

        /// <summary>True when the pixel is inside the outline ring's inner edge, where the wedge may be drawn.</summary>
        public static bool InInnerDisc(int x, int y, int size) => Distance(x, y, size) <= size / 2f - Outline;

        /// <summary>
        /// True when the pixel belongs to the wedge that is left after cutting away <paramref name="cutFraction"/> of the circle clockwise
        /// from 12 o'clock. The row index <paramref name="y"/> counts up from the bottom, like a texture.
        /// </summary>
        public static bool InWedge(int x, int y, int size, float cutFraction)
            => InInnerDisc(x, y, size) && ClockwiseFraction(x, y, size) >= cutFraction;

        /// <summary>Position of the pixel centre round the circle, 0 at 12 o'clock growing clockwise to 1.</summary>
        public static float ClockwiseFraction(int x, int y, int size)
        {
            var centre = size / 2f;
            var turn = Math.Atan2(x + 0.5f - centre, y + 0.5f - centre) / (2.0 * Math.PI);
            return (float)(turn < 0.0 ? turn + 1.0 : turn);
        }

        private static float Distance(int x, int y, int size)
        {
            var centre = size / 2f;
            var dx = x + 0.5f - centre;
            var dy = y + 0.5f - centre;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
