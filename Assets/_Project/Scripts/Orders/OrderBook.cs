using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Orders
{
    /// <summary>The open orders, one per car at most, and the source of order ids for the run.</summary>
    public sealed class OrderBook
    {
        private readonly Dictionary<CarId, Order> _byCar = new Dictionary<CarId, Order>();
        private int _nextId = 1;

        public int Count => _byCar.Count;

        /// <summary>Opens an order for a car and gives it the next id.</summary>
        public Order Open(CarId car, char letter, int targetSpot, float minute)
        {
            var order = new Order(new OrderId(_nextId++), car, letter, targetSpot, minute);
            _byCar[car] = order;
            return order;
        }

        public bool Has(CarId car) => _byCar.ContainsKey(car);

        public bool TryGetByCar(CarId car, out Order order) => _byCar.TryGetValue(car, out order);

        /// <summary>Closes the car's order. Returns false if it had none.</summary>
        public bool Remove(CarId car) => _byCar.Remove(car);

        public bool IsSpotTargeted(int spotId)
        {
            foreach (var order in _byCar.Values)
            {
                if (order.TargetSpot == spotId) return true;
            }
            return false;
        }

        /// <summary>Adds every open order to <paramref name="into"/> (which is not cleared).</summary>
        public void CopyTo(List<Order> into) => into.AddRange(_byCar.Values);

        /// <summary>Drops every open order. Ids keep counting up.</summary>
        public void Clear() => _byCar.Clear();
    }
}
