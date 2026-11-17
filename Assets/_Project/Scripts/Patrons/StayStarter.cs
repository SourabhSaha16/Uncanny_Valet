using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Decides when a patron's stay clock starts (issues 126 and 139): at the later of the patron walking into the diner
    /// (<see cref="PatronEnteredDiner"/>) and the valet driving the car out of the pickup spot (<see cref="CarLeftPickup"/>),
    /// with no fallback. Publishes <see cref="StayStarted"/> once per ticket.
    /// </summary>
    public sealed class StayStarter : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IGameClock _clock;
        private readonly TicketBook _book;
        private readonly StayStartRule _rule = new StayStartRule();

        public StayStarter(IEventBus bus, IGameClock clock, TicketBook book)
        {
            _bus = bus; _clock = clock; _book = book;
            _bus.Subscribe<PatronEnteredDiner>(OnEntered);
            _bus.Subscribe<CarLeftPickup>(OnLeft);
            _bus.Subscribe<TicketClosed>(OnClosed);
            _bus.Subscribe<ShiftStarted>(OnShiftEdge);
            _bus.Subscribe<ShiftEnded>(OnShiftEdge);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<PatronEnteredDiner>(OnEntered);
            _bus.Unsubscribe<CarLeftPickup>(OnLeft);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
            _bus.Unsubscribe<ShiftStarted>(OnShiftEdge);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEdge);
        }

        private void OnEntered(PatronEnteredDiner e)
        {
            if (_book.TryGetByCar(e.Car, out _) && _rule.PatronEntered(e.Car)) Start(e.Car);
        }

        private void OnLeft(CarLeftPickup e)
        {
            if (_book.TryGetByCar(e.Car, out _) && _rule.CarLeft(e.Car)) Start(e.Car);
        }

        private void OnClosed(TicketClosed e) => _rule.Forget(e.Car);

        private void OnShiftEdge<T>(T e) => _rule.Clear();

        private void Start(CarId car)
        {
            if (!_book.TryGetByCar(car, out var t)) return;
            var minute = _clock.ElapsedMinutes;
            if (t.StartStay(minute)) _bus.Publish(new StayStarted(t.TicketId, t.CarId, minute));
        }
    }
}
