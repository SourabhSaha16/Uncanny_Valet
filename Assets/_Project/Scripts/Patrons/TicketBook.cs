using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>Storage for open tickets, looked up by ticket or car. Owns the car letters held by those tickets.</summary>
    public sealed class TicketBook
    {
        private readonly LetterPool _letters = new LetterPool();
        private readonly Dictionary<int, Ticket> _byTicket = new Dictionary<int, Ticket>();
        private readonly Dictionary<int, Ticket> _byCar = new Dictionary<int, Ticket>();
        private int _nextId = 1;

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

        /// <summary>Creates a ticket with the lowest free letter and stores it.</summary>
        public Ticket Add(CarId car, PatronRequest request)
        {
            var ticket = new Ticket(new TicketId(_nextId++), car, request, _letters.Acquire());
            _byTicket[ticket.TicketId.Value] = ticket;
            _byCar[car.Value] = ticket;
            return ticket;
        }

        /// <summary>Forgets a ticket and frees its letter.</summary>
        public void Remove(Ticket t)
        {
            _byTicket.Remove(t.TicketId.Value);
            _byCar.Remove(t.CarId.Value);
            _letters.Release(t.Letter);
        }

        /// <summary>Makes every letter free again (start of a night); call only when no ticket is open.</summary>
        public void ResetLetters() { if (_byTicket.Count == 0) _letters.Reset(); }

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
