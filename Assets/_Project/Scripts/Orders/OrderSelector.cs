using System;
using System.Collections.Generic;

namespace UncannyValet.Orders
{
    /// <summary>Chooses which parked car to order and which free spot to send it to.</summary>
    public sealed class OrderSelector
    {
        private readonly IOrderRandom _random;

        public OrderSelector(IOrderRandom random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <summary>
        /// Picks a random car and a random free spot. Returns false (no order this time) when there is no
        /// candidate car or no free spot.
        /// </summary>
        public bool TryPick(IReadOnlyList<ParkedCar> cars, IReadOnlyList<int> freeSpots, out OrderPick pick)
        {
            pick = default;
            if (cars.Count == 0 || freeSpots.Count == 0) return false;
            var car = cars[_random.Next(cars.Count)];
            pick = new OrderPick(car, freeSpots[_random.Next(freeSpots.Count)]);
            return true;
        }
    }
}
