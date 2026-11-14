using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Patrons
{
    /// <summary>Pure maths: which queue slot a standing car is in.</summary>
    public static class QueueSlotLocator
    {
        /// <summary>No slot is near the point.</summary>
        public const int None = -1;

        /// <summary>The slot whose centre is nearest <paramref name="point"/> and within <paramref name="tolerance"/>, or <see cref="None"/>.</summary>
        public static int SlotAt(IReadOnlyList<Vector2> slots, Vector2 point, float tolerance)
        {
            var best = None;
            var bestDistance = tolerance;
            for (var i = 0; i < slots.Count; i++)
            {
                var d = Vector2.Distance(slots[i], point);
                if (d > bestDistance) continue;
                bestDistance = d;
                best = i;
            }
            return best;
        }
    }
}
