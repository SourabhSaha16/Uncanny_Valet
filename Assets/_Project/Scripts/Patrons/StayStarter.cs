using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Decides when a patron's stay clock starts (issue 126): the moment the patron walks into the diner
    /// (<see cref="PatronEnteredDiner"/>), not when the car is dropped off, parked or left the queue, and with no fallback.
    /// Publishes <see cref="StayStarted"/> once per ticket.
    /// </summary>
    public sealed class StayStarter : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IGameClock _clock;
        private readonly TicketBook _book;

        public StayStarter(IEventBus bus, IGameClock clock, TicketBook book)
        {
            _bus = bus; _clock = clock; _book = book;
            _bus.Subscribe<PatronEnteredDiner>(OnEntered);
        }

        public void Dispose() => _bus.Unsubscribe<PatronEnteredDiner>(OnEntered);

        private void OnEntered(PatronEnteredDiner e)
        {
            if (!_book.TryGetByCar(e.Car, out var t)) return;
            var minute = _clock.ElapsedMinutes;
            if (t.StartStay(minute)) _bus.Publish(new StayStarted(t.TicketId, t.CarId, minute));
        }
    }
}
