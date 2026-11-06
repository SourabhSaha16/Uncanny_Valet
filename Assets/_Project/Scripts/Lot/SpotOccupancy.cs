using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure record of which car sits in which spot, and which spots are open. A spot is open while it is
    /// within the night's limit (regular spots), while charging is open (charging spots), or while a car
    /// holds it - so a spot never closes under a parked car.
    /// </summary>
    public sealed class SpotOccupancy
    {
        private readonly HashSet<int> _known = new HashSet<int>();
        private readonly Dictionary<int, CarId> _cars = new Dictionary<int, CarId>();
        private readonly int _firstChargingId;
        private bool _chargingOpen;

        /// <param name="spotIds">Every spot id that exists.</param>
        /// <param name="firstChargingId">Ids from here up are charging spots; default: none are.</param>
        public SpotOccupancy(IEnumerable<int> spotIds, int firstChargingId = int.MaxValue)
        {
            _firstChargingId = firstChargingId;
            foreach (var id in spotIds) _known.Add(id);
        }

        /// <summary>Highest regular spot id the night plan opens (spots above it are closed unless held by a car).</summary>
        public int OpenLimit { get; private set; } = int.MaxValue;

        /// <summary>Opens regular spots 1..<paramref name="activeSpots"/> only. 0 (or less) opens them all.</summary>
        public void SetActiveSpots(int activeSpots) => OpenLimit = OpenSpotRule.Limit(activeSpots);

        /// <summary>Opens or closes the charging spots (a charging spot holding a car stays open).</summary>
        public void SetChargingOpen(bool open) => _chargingOpen = open;

        public bool IsCharging(int spotId) => spotId >= _firstChargingId;

        public bool IsOpen(int spotId)
        {
            if (!_known.Contains(spotId)) return false;
            if (_cars.ContainsKey(spotId)) return true;
            return IsCharging(spotId) ? _chargingOpen : spotId <= OpenLimit;
        }

        public bool IsFree(int spotId) => IsOpen(spotId) && !_cars.ContainsKey(spotId);

        public bool TryGetCar(int spotId, out CarId car) => _cars.TryGetValue(spotId, out car);

        /// <summary>Returns false if the spot is unknown, closed or taken by another car.</summary>
        public bool Park(int spotId, CarId car)
        {
            if (!IsOpen(spotId)) return false;
            if (_cars.TryGetValue(spotId, out var current)) return current == car;
            _cars[spotId] = car;
            return true;
        }

        /// <summary>Frees the spot only if that car holds it.</summary>
        public bool Leave(int spotId, CarId car)
        {
            if (!_cars.TryGetValue(spotId, out var current) || current != car) return false;
            _cars.Remove(spotId);
            return true;
        }
    }
}
