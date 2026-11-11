using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Executes the director's <see cref="ReturnShiftRequested"/>: moves the end of a parked patron's stay through
    /// <see cref="Ticket.ShiftStay"/> and answers with <see cref="PatronReturnShifted"/>. It owns no rule about when
    /// to move a return; the director decides, and a patron who already left the diner is refused here.
    /// </summary>
    public sealed class ReturnShifter : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IGameClock _clock;
        private readonly TicketBook _book;

        public ReturnShifter(IEventBus bus, IGameClock clock, TicketBook book)
        {
            _bus = bus; _clock = clock; _book = book;
            _bus.Subscribe<ReturnShiftRequested>(OnRequested);
        }

        public void Dispose() => _bus.Unsubscribe<ReturnShiftRequested>(OnRequested);

        private void OnRequested(ReturnShiftRequested e)
        {
            if (!_book.TryGetByCar(e.Car, out var t)) return;
            var applied = t.ShiftStay(e.DeltaMinutes, _clock.ElapsedMinutes);
            if (applied != 0f) _bus.Publish(new PatronReturnShifted(t.TicketId, t.CarId, applied));
        }
    }
}
