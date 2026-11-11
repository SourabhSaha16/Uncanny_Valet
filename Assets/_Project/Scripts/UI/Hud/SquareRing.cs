using System;

namespace UncannyValet.UI.Hud
{
    /// <summary>Pure geometry of a one pixel square ring, walked clockwise from the top left corner (the countdown ring).</summary>
    public static class SquareRing
    {
        /// <summary>Number of pixels on the ring of a square of the given side (0 below 2).</summary>
        public static int Cells(int side) => side < 2 ? 0 : 4 * (side - 1);

        /// <summary>Offset of ring pixel <paramref name="index"/> (0 to Cells-1) from the square's top left.</summary>
        public static void Cell(int side, int index, out int x, out int y)
        {
            var edge = side - 1;
            var run = index % edge;
            switch (index / edge)
            {
                case 0: x = run; y = 0; break;
                case 1: x = edge; y = run; break;
                case 2: x = edge - run; y = edge; break;
                default: x = 0; y = edge - run; break;
            }
        }

        /// <summary>Pixels lit for a remaining fraction (0 to 1), rounded up so a running timer never looks empty.</summary>
        public static int Lit(int side, float remaining01)
        {
            var fraction = Math.Min(Math.Max(remaining01, 0f), 1f);
            return (int)Math.Ceiling(fraction * Cells(side) - 0.0001f);
        }
    }
}
