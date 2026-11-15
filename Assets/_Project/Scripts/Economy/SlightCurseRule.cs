using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Economy
{
    /// <summary>
    /// Pure rule: the slight curse for the driven car hitting any pedestrian, on every lot (it ignores the lot's curse
    /// switch). The same pedestrian cannot slight-curse again until the lockout has passed.
    /// </summary>
    public sealed class SlightCurseRule
    {
        private readonly Dictionary<PedestrianId, float> _last = new Dictionary<PedestrianId, float>();

        /// <summary>True (and the lockout starts) when <paramref name="hit"/> is a slight curse.</summary>
        /// <param name="now">Seconds on any steady clock.</param>
        /// <param name="lockoutSeconds">Seconds after a slight curse during which the same pedestrian cannot curse again.</param>
        public bool TryCurse(in PedestrianHit hit, float now, float lockoutSeconds)
        {
            if (!hit.ByPlayerCar) return false;
            if (_last.TryGetValue(hit.Pedestrian, out var last) && now - last < lockoutSeconds) return false;
            _last[hit.Pedestrian] = now;
            return true;
        }

        /// <summary>Forgets every lockout (a new shift starts).</summary>
        public void Reset() => _last.Clear();
    }
}
