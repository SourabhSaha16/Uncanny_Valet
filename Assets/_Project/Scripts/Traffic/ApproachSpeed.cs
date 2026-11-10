using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>Pure rule: the fastest speed that still lets a car slow to a target speed by a given point.</summary>
    public static class ApproachSpeed
    {
        /// <summary>Speed cap with <paramref name="remaining"/> distance left, braking at <paramref name="brake"/> down to <paramref name="endSpeed"/>.</summary>
        public static float Cap(float remaining, float endSpeed, float brake) =>
            Mathf.Sqrt(endSpeed * endSpeed + 2f * brake * Mathf.Max(0f, remaining));
    }
}
