using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// The rule for "a car is in mud": the car's centre or any of its four wheel points is inside a puddle. The wheel points are the
    /// body's corners pulled in to <see cref="WheelInset"/> of the half size, so the rim of a puddle counts only when a tyre touches it.
    /// </summary>
    public static class CarMudProbe
    {
        /// <summary>Share of the body's half length and half width at which the wheels sit.</summary>
        public const float WheelInset = 0.8f;

        /// <summary>True when the centre or a wheel point is inside the patches. No allocation.</summary>
        public static bool IsInMud(IMudPatches patches, Vector2 position, Vector2 up, Vector2 size)
        {
            if (patches == null) return false;
            if (patches.IsInside(position)) return true;
            var right = new Vector2(up.y, -up.x);
            var along = up * (size.y * 0.5f * WheelInset);
            var across = right * (size.x * 0.5f * WheelInset);
            return patches.IsInside(position + along + across) || patches.IsInside(position + along - across)
                || patches.IsInside(position - along + across) || patches.IsInside(position - along - across);
        }
    }
}
