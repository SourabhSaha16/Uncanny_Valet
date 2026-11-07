using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Pure maths for the mud overlay. Washing wipes the mud away across the car's width (local X, left to
    /// right) by shrinking the overlay from its left edge; it never fades. The overlay sprite is 1x1, centred,
    /// and a child of the car, so the numbers below are fractions of the car's width.
    /// </summary>
    public static class CarMudMath
    {
        /// <summary>Fraction of the car width still covered in mud: 1 when filthy, 0 when clean.</summary>
        public static float Width(float clean01) => 1f - Mathf.Clamp01(clean01);

        /// <summary>Local X of the overlay centre so its right edge stays on the car's right edge.</summary>
        public static float CenterX(float clean01) => Mathf.Clamp01(clean01) * 0.5f;
    }
}
