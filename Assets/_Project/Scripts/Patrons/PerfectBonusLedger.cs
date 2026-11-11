using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Counts the Perfect Parks of each ticket until it is paid. Records one per <see cref="PerfectPark"/> event,
    /// hands the count over on payout and forgets the ticket when it closes.
    /// </summary>
    public sealed class PerfectBonusLedger
    {
        private readonly IEventBus _bus;
        private readonly Dictionary<TicketId, int> _counts = new Dictionary<TicketId, int>();

        public PerfectBonusLedger(IEventBus bus)
        {
            _bus = bus;
            _bus.Subscribe<PerfectPark>(OnPerfectPark);
        }

        public void Dispose() => _bus.Unsubscribe<PerfectPark>(OnPerfectPark);

        /// <summary>Perfect Parks waiting on the ticket, without paying them out.</summary>
        public int Count(TicketId ticket) => _counts.TryGetValue(ticket, out var n) ? n : 0;

        /// <summary>Returns the Perfect Parks of the ticket and forgets them.</summary>
        public int Take(TicketId ticket)
        {
            var n = Count(ticket);
            _counts.Remove(ticket);
            return n;
        }

        /// <summary>Forgets the ticket (it closed without a payout).</summary>
        public void Clear(TicketId ticket) => _counts.Remove(ticket);

        private void OnPerfectPark(PerfectPark e)
        {
            if (e.Ticket == TicketId.None) return;
            _counts[e.Ticket] = Count(e.Ticket) + 1;
        }
    }
}
