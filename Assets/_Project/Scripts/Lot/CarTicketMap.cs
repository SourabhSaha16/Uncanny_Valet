using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Lot
{
    /// <summary>Pure book-keeping of which ticket belongs to which car and how much of its pickup timer is used.</summary>
    public sealed class CarTicketMap
    {
        private readonly Dictionary<CarId, TicketId> _tickets = new Dictionary<CarId, TicketId>();
        private readonly Dictionary<CarId, float> _pickupElapsed = new Dictionary<CarId, float>();

        public void Opened(CarId car, TicketId ticket) => _tickets[car] = ticket;

        /// <summary>The pickup timer moved on (0 = just started, 1 = ran out).</summary>
        public void PickupProgress(CarId car, float elapsed01) => _pickupElapsed[car] = elapsed01;

        public void Closed(CarId car)
        {
            _tickets.Remove(car);
            _pickupElapsed.Remove(car);
        }

        public TicketId TicketOf(CarId car) => _tickets.TryGetValue(car, out var t) ? t : TicketId.None;

        /// <summary>Share 0..1 of the car's pickup timer used so far; 0 before the timer has reported.</summary>
        public float PickupElapsed(CarId car) => _pickupElapsed.TryGetValue(car, out var e) ? e : 0f;
    }
}
