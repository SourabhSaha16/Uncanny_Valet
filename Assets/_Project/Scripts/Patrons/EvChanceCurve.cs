using UnityEngine;

namespace UncannyValet.Patrons
{
    /// <summary>Pure rule: the EV chance rises from the night's base chance to its late chance once the night passes a start fraction.</summary>
    public static class EvChanceCurve
    {
        /// <param name="nightProgress">Fraction of the night elapsed (0..1).</param>
        /// <param name="lateStart">Fraction of the night at which the ramp begins.</param>
        public static float Evaluate(float baseChance, float lateChance, float lateStart, float nightProgress)
        {
            var span = 1f - lateStart;
            var t = span <= 0f ? (nightProgress >= lateStart ? 1f : 0f) : Mathf.Clamp01((nightProgress - lateStart) / span);
            return Mathf.Lerp(baseChance, lateChance, t);
        }
    }
}
