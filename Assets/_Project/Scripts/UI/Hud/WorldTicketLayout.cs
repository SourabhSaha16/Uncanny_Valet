using System;

namespace UncannyValet.UI.Hud
{
    /// <summary>Pure sizes shared by the small panels drawn over the lot (hints, mood bubbles): border and keeping them on the canvas.</summary>
    public static class WorldTicketLayout
    {
        public const int Border = 3;
        public const int ScreenMargin = 2;

        /// <summary>Keeps a box fully on the canvas.</summary>
        public static int ClampToCanvas(int position, int size, int canvasSize)
            => Math.Max(ScreenMargin, Math.Min(position, canvasSize - ScreenMargin - size));
    }
}
