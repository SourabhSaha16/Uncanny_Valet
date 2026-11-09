using System;

namespace UncannyValet.UI.Hud
{
    /// <summary>Pure sizes of the floating ticket over a car: letter, request icons, patience bar.</summary>
    public static class WorldTicketLayout
    {
        public const int Border = 3;
        public const int LetterWidth = 5;
        public const int IconSize = 9;
        public const int IconGap = 1;
        public const int LetterGap = 3;
        public const int BarHeight = 3;
        public const int BarGap = 2;
        public const int MinWidth = 24;
        public const int ScreenMargin = 2;

        public static int Height => Border + IconSize + BarGap + BarHeight + Border;

        /// <summary>Width for a ticket showing this many request icons.</summary>
        public static int Width(int iconCount)
        {
            var icons = iconCount <= 0 ? 0 : LetterGap + iconCount * (IconSize + IconGap) - IconGap;
            return Math.Max(MinWidth, Border * 2 + LetterWidth + icons);
        }

        /// <summary>Keeps a box fully on the canvas.</summary>
        public static int ClampToCanvas(int position, int size, int canvasSize)
            => Math.Max(ScreenMargin, Math.Min(position, canvasSize - ScreenMargin - size));
    }
}
