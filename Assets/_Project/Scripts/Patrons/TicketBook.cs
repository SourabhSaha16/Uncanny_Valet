using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>Storage for open tickets, looked up by ticket or car. Owns the tag colours held by those tickets.</summary>
    public sealed class TicketBook
    {
        private readonly TagPool _tags = new TagPool();
        private readonly Dictionary<int, Ticket> _byTicket = new Dictionary<int, Ticket>();
        private readonly Dictionary<int, Ticket> _byCar = new Dictionary<int, Ticket>();
        private int _nextId = 1;

        public TicketBook(ServiceTuning tuning) { Tuning = tuning; }

        /// <summary>Service numbers given to tickets created from now on.</summary>
        public ServiceTuning Tuning { get; set; }

        public int Count => _byTicket.Count;

        /// <summary>Tickets whose car has not been parked yet.</summary>
        public int AwaitingParkCount
        {
            get
            {
                var n = 0;
                foreach (var t in _byTicket.Values) if (t.State == TicketState.AwaitingPark) n++;
                return n;
            }
        }

        /// <summary>Creates a ticket with the lowest free tag colour and stores it.</summary>
        public Ticket Add(CarId car, PatronRequest request)
        {
            var ticket = new Ticket(new TicketId(_nextId++), car, request, _tags.Acquire(), Tuning);
            _byTicket[ticket.TicketId.Value] = ticket;
            _byCar[car.Value] = ticket;
            return ticket;
        }

        /// <summary>Forgets a ticket and frees its tag.</summary>
        public void Remove(Ticket t)
        {
            _byTicket.Remove(t.TicketId.Value);
            _byCar.Remove(t.CarId.Value);
            _tags.Release(t.Tag);
        }

        /// <summary>Makes every tag free again (start of a shift); call only when no ticket is open.</summary>
        public void ResetTags() { if (_byTicket.Count == 0) _tags.Reset(); }

        public bool TryGet(TicketId id, out Ticket ticket) => _byTicket.TryGetValue(id.Value, out ticket);

        public bool TryGetByCar(CarId car, out Ticket ticket) => _byCar.TryGetValue(car.Value, out ticket);

        /// <summary>Copies the open tickets into a list, so callers can close tickets while iterating.</summary>
        public void CopyTo(List<Ticket> into)
        {
            into.Clear();
            into.AddRange(_byTicket.Values);
        }
    }
}
