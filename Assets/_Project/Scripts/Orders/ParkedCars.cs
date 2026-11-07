using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Orders
{
    /// <summary>
    /// Which cars are parked right now and which letter each shows. A car is a candidate for an order only
    /// while it is parked, has a letter, has no pickup request and has no open order.
    /// </summary>
    public sealed class ParkedCars
    {
        private readonly Dictionary<CarId, int> _spots = new Dictionary<CarId, int>();
        private readonly Dictionary<CarId, char> _letters = new Dictionary<CarId, char>();
        private readonly HashSet<CarId> _pickupRequested = new HashSet<CarId>();

        public void SetLetter(CarId car, char letter) => _letters[car] = letter;

        public void Park(CarId car, int spotId) => _spots[car] = spotId;

        /// <summary>Forgets the spot only if the car was still recorded in that one.</summary>
        public void LeaveSpot(CarId car, int spotId)
        {
            if (_spots.TryGetValue(car, out var current) && current == spotId) _spots.Remove(car);
        }

        public void RequestPickup(CarId car) => _pickupRequested.Add(car);

        /// <summary>Drops everything known about a car (its ticket closed).</summary>
        public void Forget(CarId car)
        {
            _spots.Remove(car);
            _letters.Remove(car);
            _pickupRequested.Remove(car);
        }

        public void Clear()
        {
            _spots.Clear();
            _letters.Clear();
            _pickupRequested.Clear();
        }

        /// <summary>Adds every eligible car to <paramref name="into"/> (which is not cleared).</summary>
        public void CollectEligible(OrderBook book, List<ParkedCar> into)
        {
            foreach (var pair in _spots)
            {
                if (_pickupRequested.Contains(pair.Key) || book.Has(pair.Key)) continue;
                if (_letters.TryGetValue(pair.Key, out var letter)) into.Add(new ParkedCar(pair.Key, letter, pair.Value));
            }
        }
    }
}
