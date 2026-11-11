using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure rule for one car: has it come to rest clear of the arrival queue? True once, on the tick the car has been
    /// resting (empty and slow) for the settle time while at least the clear distance from the nearest queue slot
    /// centre. Any move, a driver, or a spot nearer the queue restarts the count.
    /// </summary>
    public sealed class QueueClearRule
    {
        private readonly StopTimer _rest;

        public QueueClearRule(float settleSeconds) { _rest = new StopTimer(settleSeconds); }

        /// <summary>True once the rule has fired for this car; it never fires again.</summary>
        public bool IsDone => _rest.IsDone;

        /// <summary>Advances time. Returns true only on the tick the car is judged clear of the queue.</summary>
        /// <param name="resting">No driver and speed below the stopped threshold.</param>
        public bool Tick(float deltaSeconds, float distanceToQueue, float clearDistance, bool resting)
            => _rest.Tick(deltaSeconds, resting && distanceToQueue >= clearDistance);

        /// <summary>Distance from a point to the nearest slot centre; infinity when there are no slots.</summary>
        public static float DistanceToNearest(IReadOnlyList<Vector2> slots, Vector2 point)
        {
            var best = float.PositiveInfinity;
            for (var i = 0; i < slots.Count; i++)
            {
                var d = Vector2.Distance(slots[i], point);
                if (d < best) best = d;
            }
            return best;
        }
    }
}
