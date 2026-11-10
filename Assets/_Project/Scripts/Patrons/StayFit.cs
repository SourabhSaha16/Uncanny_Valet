using System;

namespace UncannyValet.Patrons
{
    /// <summary>Pure rule: a stay is cut short when the patron would otherwise still be away at the end of the shift.</summary>
    public static class StayFit
    {
        /// <summary>
        /// Returns <paramref name="stay"/>, or less when it would run past the last moment a patron can ask for the car
        /// (<paramref name="remainingMinutes"/> minus <paramref name="leadMinutes"/>), but never below
        /// <paramref name="floorMinutes"/> and never above the rolled stay.
        /// </summary>
        public static float Clamp(float stay, float remainingMinutes, float leadMinutes, float floorMinutes)
            => Math.Min(stay, Math.Max(floorMinutes, remainingMinutes - leadMinutes));
    }
}
