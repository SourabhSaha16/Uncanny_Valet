using System;
using System.Collections.Generic;
using UncannyValet.Core.Events;

namespace UncannyValet.UI
{
    /// <summary>Fills the service-need fields of the board's rows (EV, dirty, charge, wash progress) from events only.</summary>
    public sealed class TicketNeedsTracker : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly SortedDictionary<int, TicketRow> _rows;

        public TicketNeedsTracker(IEventBus bus, SortedDictionary<int, TicketRow> rows)
        {
            _bus = bus;
            _rows = rows;
            _bus.Subscribe<TicketTraitsAssigned>(OnTraits);
            _bus.Subscribe<CarChargeChanged>(OnCharge);
            _bus.Subscribe<CarCleaned>(OnCleaned);
            _bus.Subscribe<CarFullyCharged>(OnFullyCharged);
            _bus.Subscribe<CarParked>(OnParked);
            _bus.Subscribe<CarCleanChanged>(OnCleanChanged);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<TicketTraitsAssigned>(OnTraits);
            _bus.Unsubscribe<CarChargeChanged>(OnCharge);
            _bus.Unsubscribe<CarCleaned>(OnCleaned);
            _bus.Unsubscribe<CarFullyCharged>(OnFullyCharged);
            _bus.Unsubscribe<CarParked>(OnParked);
            _bus.Unsubscribe<CarCleanChanged>(OnCleanChanged);
        }

        /// <summary>The open row of a car, or null.</summary>
        public static TicketRow FindByCar(SortedDictionary<int, TicketRow> rows, int car)
        {
            foreach (var row in rows.Values) if (row.Car == car) return row;
            return null;
        }

        private void OnCleanChanged(CarCleanChanged e) { var row = FindByCar(_rows, e.Car.Value); if (row != null) row.Clean01 = e.Clean01; }

        private void OnTraits(TicketTraitsAssigned e)
        {
            if (!_rows.TryGetValue(e.Ticket.Value, out var row)) return;
            row.IsEv = e.IsEv;
            row.IsDirty = e.IsDirty;
        }

        private void OnCharge(CarChargeChanged e) { var row = FindByCar(_rows, e.Car.Value); if (row != null) row.Charge01 = e.Charge01; }

        private void OnCleaned(CarCleaned e) { var row = FindByCar(_rows, e.Car.Value); if (row == null) return; row.Washed = true; row.ShowWashed = true; }

        private void OnFullyCharged(CarFullyCharged e) { var row = FindByCar(_rows, e.Car.Value); if (row != null) row.ShowCharged = true; }

        /// <summary>Parking ends the short "Washed" / "Charged" moments: the car is simply Parked again.</summary>
        private void OnParked(CarParked e)
        {
            var row = FindByCar(_rows, e.Car.Value);
            if (row == null) return;
            row.ShowWashed = false;
            row.ShowCharged = false;
        }
    }
}
