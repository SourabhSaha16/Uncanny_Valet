using System;
using System.Collections.Generic;
using System.Text;
using UncannyValet.Core.Events;

namespace UncannyValet.UI
{
    /// <summary>Tracks open tickets, their state and parking spot from events only.</summary>
    public sealed class TicketBoard : IDisposable
    {
        private sealed class Row
        {
            public int Car;
            public string State = "Awaiting park";
            public int Spot;
        }

        private readonly IEventBus _bus;
        private readonly SortedDictionary<int, Row> _rows = new SortedDictionary<int, Row>();

        public int Count => _rows.Count;
        public int PickupCount { get; private set; }

        public TicketBoard(IEventBus bus)
        {
            _bus = bus;
            _bus.Subscribe<TicketOpened>(OnOpened);
            _bus.Subscribe<CarParked>(OnParked);
            _bus.Subscribe<CarLeftSpot>(OnLeftSpot);
            _bus.Subscribe<PickupRequested>(OnPickup);
            _bus.Subscribe<TicketClosed>(OnClosed);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<TicketOpened>(OnOpened);
            _bus.Unsubscribe<CarParked>(OnParked);
            _bus.Unsubscribe<CarLeftSpot>(OnLeftSpot);
            _bus.Unsubscribe<PickupRequested>(OnPickup);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
        }

        public void AppendLines(StringBuilder text)
        {
            foreach (var pair in _rows)
            {
                var r = pair.Value;
                var spot = r.Spot != 0 ? (r.State == "Parked" ? " in spot " + r.Spot : " (spot " + r.Spot + ")") : "";
                text.AppendLine($"  #{pair.Key} Car#{r.Car}: {r.State}{spot}");
            }
        }

        private void OnOpened(TicketOpened e) => _rows[e.Ticket.Value] = new Row { Car = e.Car.Value };

        private void OnParked(CarParked e)
        {
            var r = FindByCar(e.Car.Value);
            if (r == null) return;
            r.Spot = e.SpotId;
            if (r.State == "Awaiting park") r.State = "Parked";
        }

        private void OnLeftSpot(CarLeftSpot e)
        {
            var r = FindByCar(e.Car.Value);
            if (r != null && r.Spot == e.SpotId) r.Spot = 0;
        }

        private void OnPickup(PickupRequested e)
        {
            if (!_rows.TryGetValue(e.Ticket.Value, out var r)) return;
            r.State = "Pickup requested";
            PickupCount++;
        }

        private void OnClosed(TicketClosed e)
        {
            if (!_rows.TryGetValue(e.Ticket.Value, out var r)) return;
            if (r.State == "Pickup requested" && PickupCount > 0) PickupCount--;
            _rows.Remove(e.Ticket.Value);
        }

        private Row FindByCar(int car)
        {
            foreach (var r in _rows.Values) if (r.Car == car) return r;
            return null;
        }
    }
}
