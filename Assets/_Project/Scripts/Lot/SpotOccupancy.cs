using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Lot
{
    /// <summary>Pure record of which car sits in which spot.</summary>
    public sealed class SpotOccupancy
    {
        private readonly HashSet<int> _known = new HashSet<int>();
        private readonly Dictionary<int, CarId> _cars = new Dictionary<int, CarId>();

        public SpotOccupancy(IEnumerable<int> spotIds)
        {
            foreach (var id in spotIds) _known.Add(id);
        }

        public bool IsFree(int spotId) => _known.Contains(spotId) && !_cars.ContainsKey(spotId);

        public bool TryGetCar(int spotId, out CarId car) => _cars.TryGetValue(spotId, out car);

        /// <summary>Returns false if the spot is unknown or taken by another car.</summary>
        public bool Park(int spotId, CarId car)
        {
            if (!_known.Contains(spotId)) return false;
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
