using System;
using UncannyValet.UI.Kit;

namespace UncannyValet.UI.Hud
{
    /// <summary>Pure sizes and flash timing of a licence plate box: a light plate with a dark border and pixel text.</summary>
    public static class PlateLayout
    {
        public const int Border = 1;
        public const int PadX = 2;
        public const int PadY = 1;
        /// <summary>Thickness of the flashing glow drawn around a plate whose car is wanted back.</summary>
        public const int Glow = 2;
        /// <summary>Seconds per flash right after the pickup is requested.</summary>
        public const float SlowPeriod = 1f;
        /// <summary>Seconds per flash when patience is gone.</summary>
        public const float FastPeriod = 1f / 6f;

        /// <summary>Plate width in virtual pixels for a plate text of this length.</summary>
        public static int Width(int length) => PixelTextLayout.Width(length, 1) + 2 * (Border + PadX);

        /// <summary>Plate height in virtual pixels.</summary>
        public static int Height => PixelGlyphs.Height + 2 * (Border + PadY);

        /// <summary>Seconds per flash: slow while patience is fresh, fast as it runs out.</summary>
        public static float FlashPeriod(float elapsed01, bool expired)
        {
            var t = expired ? 1f : Math.Min(1f, Math.Max(0f, elapsed01));
            return SlowPeriod + (FastPeriod - SlowPeriod) * t;
        }
    }
}
