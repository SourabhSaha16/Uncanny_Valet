using System;
using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Pure rule (issue 139): a patron's stay clock starts at the later of two moments, the patron walking into the diner and the
    /// valet driving the car out of the pickup spot. Two flags per car; neither alone starts the clock.
    /// </summary>
    public sealed class StayStartRule
    {
        [Flags]
        private enum Moment { None = 0, PatronInside = 1, CarLeft = 2, Both = PatronInside | CarLeft }

        private readonly Dictionary<CarId, Moment> _seen = new Dictionary<CarId, Moment>();

        /// <summary>The patron entered the diner. True when this completes the pair (the clock starts now).</summary>
        public bool PatronEntered(CarId car) => Mark(car, Moment.PatronInside);

        /// <summary>The car left the pickup spot. True when this completes the pair (the clock starts now).</summary>
        public bool CarLeft(CarId car) => Mark(car, Moment.CarLeft);

        /// <summary>Drops the car's flags (ticket closed).</summary>
        public void Forget(CarId car) => _seen.Remove(car);

        /// <summary>Drops every flag (shift edge).</summary>
        public void Clear() => _seen.Clear();

        /// <summary>Number of cars with exactly one flag set (for leak checks).</summary>
        public int Pending => _seen.Count;

        private bool Mark(CarId car, Moment moment)
        {
            _seen.TryGetValue(car, out var now);
            now |= moment;
            if (now != Moment.Both) { _seen[car] = now; return false; }
            _seen.Remove(car);
            return true;
        }
    }
}
