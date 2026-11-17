using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Pure rule: the car's speed after it hits a pedestrian. It loses a share of its speed but never drops below a floor (and is never sped up).</summary>
    public static class PedestrianSlowRule
    {
        /// <param name="speed">Speed before the hit.</param>
        /// <param name="slowShare">Share of the speed lost (0..1).</param>
        /// <param name="floorSpeed">The slowdown stops here; a car already slower than this keeps its speed.</param>
        public static float SlowedSpeed(float speed, float slowShare, float floorSpeed)
            => Mathf.Max(speed * (1f - Mathf.Clamp01(slowShare)), Mathf.Min(speed, Mathf.Max(0f, floorSpeed)));
    }
}
