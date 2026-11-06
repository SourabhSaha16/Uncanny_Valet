using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Pure geometry for the charge bar fill, in the track's local unit space (track spans -0.5..0.5).</summary>
    public static class CarChargeBarMath
    {
        /// <summary>Charge at or above this counts as full.</summary>
        public const float FullThreshold = 0.999f;

        /// <summary>Fraction of the track left for the fill once a border is kept on both sides.</summary>
        public static float UsableFraction(float trackLength, float border)
        {
            if (trackLength <= 0f) return 0f;
            return Mathf.Max(0f, 1f - 2f * Mathf.Max(0f, border) / trackLength);
        }

        /// <summary>Fill height as a fraction of the track.</summary>
        public static float FillHeight(float charge01, float usableY) => usableY * Mathf.Clamp01(charge01);

        /// <summary>Local Y centre of a fill anchored to the track's bottom border.</summary>
        public static float FillCentreY(float charge01, float usableY)
        {
            return -usableY * 0.5f + FillHeight(charge01, usableY) * 0.5f;
        }

        /// <summary>True when the charge is full.</summary>
        public static bool IsFull(float charge01) => charge01 >= FullThreshold;
    }
}
