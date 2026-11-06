using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Pure maths for the mud overlay.</summary>
    public static class CarMudMath
    {
        /// <summary>Overlay alpha: full at clean 0, gone at clean 1.</summary>
        public static float Alpha(float clean01, float maxAlpha)
        {
            return Mathf.Clamp01(1f - Mathf.Clamp01(clean01)) * Mathf.Clamp01(maxAlpha);
        }
    }
}
