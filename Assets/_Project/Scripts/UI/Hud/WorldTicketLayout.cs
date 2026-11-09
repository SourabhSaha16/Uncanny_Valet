using System;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Pure sizes of the info box over a car: a key-tag swatch on the left, then one line per need
    /// (an icon and a meter).
    /// </summary>
    public static class WorldTicketLayout
    {
        public const int Border = 3;
        public const int TagSize = 9;
        public const int TagGap = 3;
        public const int IconSize = 9;
        public const int IconGap = 2;
        public const int MeterWidth = 18;
        public const int MeterHeight = 5;
        public const int LineGap = 2;
        public const int ScreenMargin = 2;

        /// <summary>Box width; every line has the same width.</summary>
        public static int Width => Border * 2 + TagSize + TagGap + IconSize + IconGap + MeterWidth;

        /// <summary>Box height for this many need lines (at least one).</summary>
        public static int Height(int lines)
        {
            var count = Math.Max(1, lines);
            return Border * 2 + Math.Max(TagSize, count * IconSize + (count - 1) * LineGap);
        }

        /// <summary>X of the icon column, relative to the box.</summary>
        public static int LineX => Border + TagSize + TagGap;

        /// <summary>Y of a need line, relative to the box.</summary>
        public static int LineY(int index) => Border + index * (IconSize + LineGap);

        /// <summary>Pixels of a meter that is filled to <paramref name="level01"/> (clamped to 0..1).</summary>
        public static int MeterFill(float level01)
        {
            var clamped = Math.Min(1f, Math.Max(0f, level01));
            return (int)(clamped * MeterWidth + 0.5f);
        }

        /// <summary>Keeps a box fully on the canvas.</summary>
        public static int ClampToCanvas(int position, int size, int canvasSize)
            => Math.Max(ScreenMargin, Math.Min(position, canvasSize - ScreenMargin - size));
    }
}
