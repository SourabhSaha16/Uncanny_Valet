using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure rule for a single-occupancy station: which cars hold it, and whether a given car is kept out.
    /// A car already inside is never blocked, so it can always drive out.
    /// </summary>
    public sealed class StationOccupancy
    {
        private readonly HashSet<CarId> _holders = new HashSet<CarId>();

        /// <summary>True while at least one car holds the station.</summary>
        public bool IsOccupied => _holders.Count > 0;

        /// <summary>A car settled in the station. Returns false if it was already counted.</summary>
        public bool Admit(CarId car) => _holders.Add(car);

        /// <summary>A holding car left. Returns false if it was not a holder.</summary>
        public bool Release(CarId car) => _holders.Remove(car);

        /// <summary>True when the station is taken and <paramref name="car"/> is not one of its holders.</summary>
        public bool BlocksEntryOf(CarId car) => IsOccupied && !_holders.Contains(car);
    }
}
