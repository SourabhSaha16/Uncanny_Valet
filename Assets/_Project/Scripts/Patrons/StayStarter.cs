using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Decides when a patron's stay clock starts (issue 78): the first time the car rests clear of the arrival queue
    /// (<see cref="CarClearedQueue"/>), when it is parked in a bay, or when it has waited in the queue for the tuned
    /// maximum. Leaving a car in the queue therefore cannot be used to hand it back fast. Publishes
    /// <see cref="StayStarted"/> once per ticket.
    /// </summary>
    public sealed class StayStarter : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IGameClock _clock;
        private readonly TicketBook _book;
        private readonly List<Ticket> _scratch = new List<Ticket>();

        public StayStarter(IEventBus bus, IGameClock clock, TicketBook book)
        {
            _bus = bus; _clock = clock; _book = book;
            _bus.Subscribe<CarArrived>(OnArrived);
            _bus.Subscribe<CarClearedQueue>(OnCleared);
            _bus.Subscribe<CarParked>(OnParked);
            _bus.Subscribe<ShiftTick>(OnTick);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<CarArrived>(OnArrived);
            _bus.Unsubscribe<CarClearedQueue>(OnCleared);
            _bus.Unsubscribe<CarParked>(OnParked);
            _bus.Unsubscribe<ShiftTick>(OnTick);
        }

        private void OnArrived(CarArrived e)
        {
            if (_book.TryGetByCar(e.Car, out var t)) t.MarkArrived(_clock.ElapsedMinutes);
        }

        private void OnCleared(CarClearedQueue e) => Start(e.Car);

        private void OnParked(CarParked e) => Start(e.Car);

        private void Start(CarId car)
        {
            if (_book.TryGetByCar(car, out var t)) Start(t, _clock.ElapsedMinutes);
        }

        private void Start(Ticket t, float minute)
        {
            if (t.StartStay(minute)) _bus.Publish(new StayStarted(t.TicketId, t.CarId, minute));
        }

        private void OnTick(ShiftTick e)
        {
            _book.CopyTo(_scratch);
            var wait = _book.Tuning.QueueWaitMinutes;
            foreach (var t in _scratch)
                if (t.QueueWaitOver(e.ElapsedMinutes, wait)) Start(t, e.ElapsedMinutes);
        }
    }
}
