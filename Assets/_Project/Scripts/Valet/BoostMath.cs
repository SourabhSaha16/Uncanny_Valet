using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>Pure numbers of the speed boost on the valet: the speed multiplier and the rainbow outline colour.</summary>
    public static class BoostMath
    {
        private const float Saturation = 0.85f;
        private const float Brightness = 1f;

        /// <summary>The factor on walking speed: 1 when not boosted, otherwise the boost value (never below 1).</summary>
        public static float Multiplier(bool boosted, float boostValue) => boosted ? Mathf.Max(1f, boostValue) : 1f;

        /// <summary>Hue 0..1 of the rainbow after <paramref name="seconds"/>, going round <paramref name="cyclesPerSecond"/> times a second.</summary>
        public static float Hue(float seconds, float cyclesPerSecond)
        {
            var h = seconds * cyclesPerSecond;
            return h - Mathf.Floor(h);
        }

        /// <summary>The bright rainbow colour at <paramref name="hue01"/> (0..1).</summary>
        public static Color Rainbow(float hue01) => Color.HSVToRGB(hue01, Saturation, Brightness);

        /// <summary>True when the outline is drawn this moment. It always shows until the last <paramref name="warnSeconds"/>, then blinks at <paramref name="blinksPerSecond"/>.</summary>
        public static bool OutlineVisible(float secondsLeft, float warnSeconds, float seconds, float blinksPerSecond)
        {
            if (secondsLeft > warnSeconds) return true;
            return Mathf.FloorToInt(seconds * blinksPerSecond * 2f) % 2 == 0;
        }
    }
}
