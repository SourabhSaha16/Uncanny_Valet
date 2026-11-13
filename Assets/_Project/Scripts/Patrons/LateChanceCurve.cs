using UnityEngine;

namespace UncannyValet.Patrons
{
    /// <summary>Pure rule: a modifier's arrival chance rises from the lot's chance to the plan's late chance once the shift passes a start fraction.</summary>
    public static class LateChanceCurve
    {
        /// <param name="shiftProgress">Fraction of the shift elapsed (0..1).</param>
        /// <param name="lateStart">Fraction of the shift at which the ramp begins.</param>
        public static float Evaluate(float baseChance, float lateChance, float lateStart, float shiftProgress)
        {
            if (lateChance < 0f) return baseChance;
            var span = 1f - lateStart;
            var t = span <= 0f ? (shiftProgress >= lateStart ? 1f : 0f) : Mathf.Clamp01((shiftProgress - lateStart) / span);
            return Mathf.Lerp(baseChance, lateChance, t);
        }
    }
}
