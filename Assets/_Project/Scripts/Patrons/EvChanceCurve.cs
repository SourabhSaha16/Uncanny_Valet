using UnityEngine;

namespace UncannyValet.Patrons
{
    /// <summary>Pure rule: the EV chance rises from the shift's base chance to its late chance once the shift passes a start fraction.</summary>
    public static class EvChanceCurve
    {
        /// <param name="shiftProgress">Fraction of the shift elapsed (0..1).</param>
        /// <param name="lateStart">Fraction of the shift at which the ramp begins.</param>
        public static float Evaluate(float baseChance, float lateChance, float lateStart, float shiftProgress)
        {
            var span = 1f - lateStart;
            var t = span <= 0f ? (shiftProgress >= lateStart ? 1f : 0f) : Mathf.Clamp01((shiftProgress - lateStart) / span);
            return Mathf.Lerp(baseChance, lateChance, t);
        }
    }
}
