using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>Tracks which station each car is stopped in, charges EVs and washes cars each minute, and pauses park timers while washing.</summary>
    public sealed class StationService
    {
        private readonly IEventBus _bus;
        private readonly IGameClock _clock;
        private readonly TicketBook _book;
        private readonly List<Ticket> _scratch = new List<Ticket>();

        public StationService(IEventBus bus, IGameClock clock, TicketBook book)
        {
            _bus = bus; _clock = clock; _book = book;
            _bus.Subscribe<CarEnteredStation>(OnEntered);
            _bus.Subscribe<CarLeftStation>(OnLeft);
            _bus.Subscribe<ShiftTick>(OnShiftTick);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<CarEnteredStation>(OnEntered);
            _bus.Unsubscribe<CarLeftStation>(OnLeft);
            _bus.Unsubscribe<ShiftTick>(OnShiftTick);
        }

        /// <summary>Publishes the starting charge (EV) or clean level (dirty) of a new car.</summary>
        public void Announce(Ticket t)
        {
            if (t.IsEv) _bus.Publish(new CarChargeChanged(t.CarId, t.Charge.Charge01));
            if (t.IsDirty) _bus.Publish(new CarCleanChanged(t.CarId, t.Wash.Clean01));
        }

        private void OnEntered(CarEnteredStation e) => SetBay(e.Car, e.Station, true);

        private void OnLeft(CarLeftStation e) => SetBay(e.Car, e.Station, false);

        private void SetBay(Core.Ids.CarId car, StationKind kind, bool inside)
        {
            if (!_book.TryGetByCar(car, out var t)) return;
            if (kind == StationKind.Charging)
            {
                if (t.IsEv) t.InChargingBay = inside;
                return;
            }
            t.InWashBay = inside;
            t.SyncParkTimer(_clock.ElapsedMinutes);
        }

        private void OnShiftTick(ShiftTick e)
        {
            _book.CopyTo(_scratch);
            foreach (var t in _scratch)
            {
                if (t.IsEv && t.Charge.Advance(t.InChargingBay)) PublishCharge(t);
                if (t.IsDirty && t.Wash.Advance(t.InWashBay)) PublishClean(t);
            }
        }

        private void PublishCharge(Ticket t)
        {
            _bus.Publish(new CarChargeChanged(t.CarId, t.Charge.Charge01));
            if (t.Charge.IsFull) _bus.Publish(new CarFullyCharged(t.CarId));
        }

        private void PublishClean(Ticket t)
        {
            _bus.Publish(new CarCleanChanged(t.CarId, t.Wash.Clean01));
            if (t.Wash.IsClean) _bus.Publish(new CarCleaned(t.CarId));
        }
    }
}
