using System;

namespace UncannyValet.UI.Hud
{
    /// <summary>Pure wobble of the favour badge after the multiplier rises: a decaying vertical shake in whole pixels.</summary>
    public static class FavourWobble
    {
        public const float DurationSeconds = 0.5f;
        public const float AmplitudePixels = 2f;
        public const float HertzCycles = 4f;

        /// <summary>Vertical offset in pixels, <paramref name="secondsSinceRise"/> after the multiplier went up.</summary>
        public static int OffsetY(float secondsSinceRise)
        {
            if (secondsSinceRise < 0f || secondsSinceRise >= DurationSeconds) return 0;
            var fade = 1f - secondsSinceRise / DurationSeconds;
            var wave = (float)Math.Sin(secondsSinceRise * HertzCycles * 2.0 * Math.PI);
            return (int)Math.Round(AmplitudePixels * wave * fade);
        }
    }
}
