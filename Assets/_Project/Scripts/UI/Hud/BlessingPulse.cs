using System;

namespace UncannyValet.UI.Hud
{
    /// <summary>Pure pulse of the blessing bar after blessings are earned: 1 at the moment of the gain, fading linearly to 0.</summary>
    public static class BlessingPulse
    {
        public const float DurationSeconds = 0.4f;

        /// <summary>Brightness 0..1, <paramref name="secondsSinceGain"/> after blessings were earned.</summary>
        public static float Strength(float secondsSinceGain)
        {
            if (secondsSinceGain < 0f || secondsSinceGain >= DurationSeconds) return 0f;
            return 1f - Math.Max(0f, secondsSinceGain) / DurationSeconds;
        }
    }
}
