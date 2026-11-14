using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Economy
{
    /// <summary>
    /// Pure rule: does a pedestrian hit cost a curse? Only the car the valet drives hitting a patron walker does, and only
    /// where the lot's switch is on; the same patron cannot curse again until the lockout has passed.
    /// </summary>
    public sealed class PedestrianCurseRule
    {
        private readonly Dictionary<PedestrianId, float> _lastCurse = new Dictionary<PedestrianId, float>();

        /// <summary>True (and the lockout starts) when <paramref name="hit"/> is a curse.</summary>
        /// <param name="now">Seconds on any steady clock.</param>
        /// <param name="cursesOn">The lot's switch.</param>
        /// <param name="lockoutSeconds">Seconds after a curse during which the same patron cannot curse again.</param>
        public bool TryCurse(in PedestrianHit hit, float now, bool cursesOn, float lockoutSeconds)
        {
            if (!cursesOn || !hit.ByPlayerCar || !hit.IsPatron) return false;
            if (_lastCurse.TryGetValue(hit.Pedestrian, out var last) && now - last < lockoutSeconds) return false;
            _lastCurse[hit.Pedestrian] = now;
            return true;
        }

        /// <summary>Forgets every lockout (a new shift starts).</summary>
        public void Reset() => _lastCurse.Clear();
    }
}
