using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Pure pairing of a delivery's tip with the moment its patron boards. A car is tracked from CarDelivered; the amount
    /// arrives with TicketClosed; PatronBoarded then turns it into a pop. A ticket that closes without a delivery
    /// (patron left, shift over) is never tracked, so it never pops.
    /// </summary>
    public sealed class PendingTipPops
    {
        private const int NoAmount = int.MinValue;

        private readonly Dictionary<CarId, int> _byCar = new Dictionary<CarId, int>(8);

        /// <summary>Cars waiting for their patron to board.</summary>
        public int Count => _byCar.Count;

        /// <summary>The car was handed back: its patron will board and a tip is coming.</summary>
        public void Delivered(CarId car) => _byCar[car] = NoAmount;

        /// <summary>The ticket closed with its final tip; ignored unless the car was delivered.</summary>
        public void Closed(CarId car, int tip)
        {
            if (_byCar.ContainsKey(car)) _byCar[car] = tip;
        }

        /// <summary>The patron boarded: true with the tip when one was waiting. Always forgets the car.</summary>
        public bool TryBoard(CarId car, out int tip)
        {
            var found = _byCar.TryGetValue(car, out tip) && tip != NoAmount;
            _byCar.Remove(car);
            if (!found) tip = 0;
            return found;
        }

        /// <summary>Forgets a car whose patron will never board (it left or was despawned).</summary>
        public void Drop(CarId car) => _byCar.Remove(car);

        /// <summary>Forgets everything (shift end).</summary>
        public void Clear() => _byCar.Clear();
    }
}
