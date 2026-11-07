using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Remembers which arrival slot each still-unparked ticket was sent to. A car that is still driving in has
    /// not reached the slot yet, so the physics scan cannot see it; the reservation keeps the slot taken.
    /// </summary>
    public sealed class ArrivalReservations
    {
        private readonly Dictionary<TicketId, int> _slots = new Dictionary<TicketId, int>();
        private readonly List<TicketId> _scratch = new List<TicketId>();
        private readonly List<bool> _blocked = new List<bool>();

        /// <summary>Marks a slot as taken by a ticket until that ticket leaves AwaitingPark.</summary>
        public void Reserve(TicketId ticket, int slot) => _slots[ticket] = slot;

        /// <summary>
        /// Returns <paramref name="scanned"/> with every reserved slot also marked blocked. Reservations whose
        /// ticket has been parked, closed or lost are dropped. The result is valid until the next call.
        /// </summary>
        public IReadOnlyList<bool> Merge(IReadOnlyList<bool> scanned, TicketRegistry registry)
        {
            _blocked.Clear();
            for (var i = 0; i < scanned.Count; i++) _blocked.Add(scanned[i]);
            _scratch.Clear();
            foreach (var pair in _slots)
            {
                var waiting = registry.TryGet(pair.Key, out var t) && t.State == TicketState.AwaitingPark;
                if (!waiting) _scratch.Add(pair.Key);
                else if (pair.Value < _blocked.Count) _blocked[pair.Value] = true;
            }
            foreach (var id in _scratch) _slots.Remove(id);
            return _blocked;
        }
    }
}
