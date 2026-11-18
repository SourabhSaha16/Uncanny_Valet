using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Pure choice of where a roaming person walks next: a random spot further along or back along a sidewalk/crosswalk route
    /// (a fraction of the route). A spot inside a keep-out rectangle, or a leg that crosses one, is rejected and another is tried.
    /// Deterministic for a seed; nothing is allocated after construction.
    /// </summary>
    public sealed class WanderPicker
    {
        private readonly float _minLegShare;
        private readonly int _attempts;
        private readonly ILegBlocker _keepOut;
        private readonly List<Vector2> _leg = new List<Vector2>(12);
        private int _serial;

        /// <param name="minLegShare">Shortest leg, as a share of the route (0..1).</param>
        /// <param name="attempts">How many random spots to try before giving up.</param>
        public WanderPicker(float minLegShare, int attempts, ILegBlocker keepOut, int seed)
        {
            _minLegShare = Mathf.Clamp01(minLegShare);
            _attempts = Mathf.Max(1, attempts);
            _keepOut = keepOut;
            _serial = seed;
        }

        /// <summary>Picks the next spot on <paramref name="route"/>; false when every attempt was too short or hit a keep-out rectangle.</summary>
        public bool TryPick(IReadOnlyList<Vector2> route, float from, out float to)
        {
            to = from;
            for (var i = 0; i < _attempts; i++)
            {
                var roll = PedestrianPlan.Hash01(++_serial);
                if (Mathf.Abs(roll - from) < _minLegShare) continue;
                RoamPath.Slice(route, from, roll, _leg);
                if (_keepOut.Crosses(_leg)) continue;
                to = roll;
                return true;
            }
            return false;
        }
    }
}
