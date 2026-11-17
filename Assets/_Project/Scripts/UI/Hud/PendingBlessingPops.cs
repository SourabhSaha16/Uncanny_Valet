using System.Collections.Generic;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Pure pairing of a delivery's blessing with the moment its patron boards. A car is tracked from CarDelivered; the amount
    /// arrives with TicketClosed, together with where the car stands; PatronBoarded then turns it into a pop. A ticket that
    /// closes without a delivery (patron left, shift over) is never tracked, so it never pops. The spot is kept here
    /// because PatronBoarded also sends the car off, so the car may already be gone when the tracker hears of it.
    /// </summary>
    public sealed class PendingBlessingPops
    {
        private struct Held
        {
            public bool HasAmount;
            public int Blessing;
            public int Curse;
            public Vector2 At;
        }

        private readonly Dictionary<CarId, Held> _byCar = new Dictionary<CarId, Held>(8);

        /// <summary>Cars waiting for their patron to board.</summary>
        public int Count => _byCar.Count;

        /// <summary>The car was handed back: its patron will board and a blessing is coming.</summary>
        public void Delivered(CarId car) => _byCar[car] = default;

        /// <summary>The ticket closed with its final blessing; ignored unless the car was delivered.</summary>
        public void Closed(CarId car, int blessing, Vector2 at) => Closed(car, blessing, 0, at);

        /// <summary>
        /// The ticket closed with its final blessing and the curse its late delivery cost (0 when on time); ignored unless the
        /// car was delivered. A curse replaces the blessing pop, so the angry patron's "0" becomes a red "-N".
        /// </summary>
        public void Closed(CarId car, int blessing, int curse, Vector2 at)
        {
            if (_byCar.ContainsKey(car)) _byCar[car] = new Held { HasAmount = true, Blessing = blessing, Curse = curse, At = at };
        }

        /// <summary>The patron boarded: true with the blessing and the car's spot when one was waiting. Always forgets the car.</summary>
        public bool TryBoard(CarId car, out int blessing, out Vector2 at) => TryBoard(car, out blessing, out _, out at);

        /// <summary>Like the other overload, and also the curse the late delivery cost (0 when on time).</summary>
        public bool TryBoard(CarId car, out int blessing, out int curse, out Vector2 at)
        {
            var found = _byCar.TryGetValue(car, out var held) && held.HasAmount;
            _byCar.Remove(car);
            blessing = found ? held.Blessing : 0;
            curse = found ? held.Curse : 0;
            at = found ? held.At : Vector2.zero;
            return found;
        }

        /// <summary>Forgets everything (shift end).</summary>
        public void Clear() => _byCar.Clear();
    }
}
