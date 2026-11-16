using System;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Pure rule (issue 144): spreads patron returns by moving each rolled stay by a random fraction of itself, so cars that
    /// parked together do not all ask for pickup at once. The draw is seeded from the car id, so one car always gets the same offset.
    /// </summary>
    public static class StayJitter
    {
        /// <summary>
        /// The stay plus an offset in [-fraction, +fraction] of itself, never below <paramref name="floorMinutes"/>
        /// (a stay already at or below the floor is returned unchanged).
        /// </summary>
        public static float Apply(float stayMinutes, float fraction, float floorMinutes, int seed)
        {
            if (fraction <= 0f || stayMinutes <= floorMinutes) return stayMinutes;
            var unit = new SeededPatronRandom(seed).Value();
            var jittered = stayMinutes * (1f + fraction * (unit * 2f - 1f));
            return Math.Max(jittered, floorMinutes);
        }
    }
}
