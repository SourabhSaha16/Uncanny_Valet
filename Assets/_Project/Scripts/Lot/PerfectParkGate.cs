using System.Collections.Generic;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure repeat rules for Perfect Park: a car must have moved at least the minimum distance since it last came to rest
    /// (nudging a parked car earns nothing), and the same car earns at most one Perfect Park per cooldown.
    /// </summary>
    public sealed class PerfectParkGate
    {
        private struct Memory
        {
            public Vector2 RestAt;
            public float LastAward;
            public bool Awarded;
        }

        private readonly PerfectTuning _tuning;
        private readonly Dictionary<CarId, Memory> _cars = new Dictionary<CarId, Memory>();

        public PerfectParkGate(PerfectTuning tuning) { _tuning = tuning; }

        /// <summary>
        /// Records that a car came to rest. Returns true when it should be rewarded: <paramref name="aligned"/> is the
        /// geometric verdict, the rest of the rule is the travel distance and the cooldown.
        /// </summary>
        public bool Rest(CarId car, float time, Vector2 position, bool aligned)
        {
            var known = _cars.TryGetValue(car, out var m);
            var moved = !known || (position - m.RestAt).magnitude >= _tuning.ParkMinTravel;
            var cooled = !m.Awarded || time - m.LastAward >= _tuning.ParkCooldown;
            var award = aligned && moved && cooled;
            m.RestAt = position;
            if (award)
            {
                m.LastAward = time;
                m.Awarded = true;
            }
            _cars[car] = m;
            return award;
        }

        /// <summary>Drops a car's memory (its ticket ended).</summary>
        public void Forget(CarId car) => _cars.Remove(car);
    }
}
