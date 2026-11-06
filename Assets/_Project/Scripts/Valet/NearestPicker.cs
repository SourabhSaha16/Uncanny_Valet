using System;
using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>Pure helper: picks the closest eligible position within range.</summary>
    public static class NearestPicker
    {
        /// <summary>Index of the nearest eligible position within maxDistance, or -1 if none.</summary>
        public static int Pick(Vector2 origin, IReadOnlyList<Vector2> positions, float maxDistance,
            Func<int, bool> isEligible = null)
        {
            var best = -1;
            var bestSqr = maxDistance * maxDistance;
            for (var i = 0; i < positions.Count; i++)
            {
                if (isEligible != null && !isEligible(i)) continue;
                var sqr = (positions[i] - origin).sqrMagnitude;
                if (sqr > bestSqr) continue;
                best = i;
                bestSqr = sqr;
            }
            return best;
        }
    }
}
