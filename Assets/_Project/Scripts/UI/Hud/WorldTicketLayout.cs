using System;

namespace UncannyValet.UI.Hud
{
    /// <summary>Pure sizes shared by the small panels drawn over the lot (hints, mood bubbles): border and keeping them on the canvas.</summary>
    public static class WorldTicketLayout
    {
        public const int Border = 3;
        public const int ScreenMargin = 2;

        /// <summary>World units below the car centre where the status icons (droplet, plug, thumbs up) start.</summary>
        public const float IconDropWorldUnits = 1.3f;

        /// <summary>World units below the car centre of the middle of a 9 pixel icon (32 pixels per unit): where the wash pie takes the droplet's place.</summary>
        public const float IconCentreDropWorldUnits = IconDropWorldUnits + 4.5f / 32f;

        /// <summary>Keeps a box fully on the canvas.</summary>
        public static int ClampToCanvas(int position, int size, int canvasSize)
            => Math.Max(ScreenMargin, Math.Min(position, canvasSize - ScreenMargin - size));
    }
}
