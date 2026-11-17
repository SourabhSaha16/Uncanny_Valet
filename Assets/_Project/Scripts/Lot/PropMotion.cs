using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Pure motion rules of the animated prop layers.</summary>
    public static class PropMotion
    {
        /// <summary>Turn angle in degrees after <paramref name="time"/> seconds at <paramref name="degreesPerSecond"/>.</summary>
        public static float SpinAngle(float time, float degreesPerSecond) => Mathf.Repeat(time * degreesPerSecond, 360f);

        /// <summary>True while a blinking layer is on: the first half of each <paramref name="periodSeconds"/>.</summary>
        public static bool BlinkOn(float time, float periodSeconds)
            => periodSeconds <= 0f || Mathf.Repeat(time, periodSeconds) < periodSeconds * 0.5f;

        /// <summary>Light strength multiplier (1 at full, 1 minus <paramref name="depth"/> at the dimmest) of a flickering light at <paramref name="time"/>.</summary>
        public static float Flicker(float time, float speed, float depth, float seed)
            => 1f - depth * Mathf.PerlinNoise(time * speed, seed);
    }
}
