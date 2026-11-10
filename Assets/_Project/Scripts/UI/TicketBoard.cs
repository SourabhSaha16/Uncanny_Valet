using System;
using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.UI.Hud;

namespace UncannyValet.UI
{
    /// <summary>Tracks open tickets, tag colour, licence plate, spot and timers from events only. The HUD reads the rows.</summary>
    public sealed class TicketBoard : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly TicketNeedsTracker _needs;
        private readonly SortedDictionary<int, TicketRow> _rows = new SortedDictionary<int, TicketRow>();
        private readonly Dictionary<int, int> _tags = new Dictionary<int, int>();
        private readonly Dictionary<int, string> _plates = new Dictionary<int, string>();
        private readonly HashSet<int> _arrived = new HashSet<int>();
        private readonly HashSet<int> _placed = new HashSet<int>();

        public int Count => _rows.Count;

        public TicketBoard(IEventBus bus)
        {
            _bus = bus;
            _needs = new TicketNeedsTracker(bus, _rows);
            _bus.Subscribe<TicketOpened>(OnOpened);
            _bus.Subscribe<CarTagAssigned>(OnTag);
            _bus.Subscribe<CarPlateAssigned>(OnPlate);
            _bus.Subscribe<CarParked>(OnParked);
            _bus.Subscribe<CarEnteredStation>(OnEnteredStation);
            _bus.Subscribe<CarLeftStation>(OnLeftStation);
            _bus.Subscribe<CarLeftSpot>(OnLeftSpot);
            _bus.Subscribe<PickupRequested>(OnPickup);
            _bus.Subscribe<TicketTimerChanged>(OnTimerChanged);
            _bus.Subscribe<TicketTimerExpired>(OnTimerExpired);
            _bus.Subscribe<TicketTimerStopped>(OnTimerStopped);
            _bus.Subscribe<TicketClosed>(OnClosed);
            _bus.Subscribe<CarArrived>(OnArrived);
            _bus.Subscribe<CarDeparted>(OnDeparted);
        }

        public void Dispose()
        {
            _needs.Dispose();
            _bus.Unsubscribe<TicketOpened>(OnOpened);
            _bus.Unsubscribe<CarTagAssigned>(OnTag);
            _bus.Unsubscribe<CarPlateAssigned>(OnPlate);
            _bus.Unsubscribe<CarParked>(OnParked);
            _bus.Unsubscribe<CarEnteredStation>(OnEnteredStation);
            _bus.Unsubscribe<CarLeftStation>(OnLeftStation);
            _bus.Unsubscribe<CarLeftSpot>(OnLeftSpot);
            _bus.Unsubscribe<PickupRequested>(OnPickup);
            _bus.Unsubscribe<TicketTimerChanged>(OnTimerChanged);
            _bus.Unsubscribe<TicketTimerExpired>(OnTimerExpired);
            _bus.Unsubscribe<TicketTimerStopped>(OnTimerStopped);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
            _bus.Unsubscribe<CarArrived>(OnArrived);
            _bus.Unsubscribe<CarDeparted>(OnDeparted);
        }

        /// <summary>Clears <paramref name="into"/> and fills it with the open tickets the key board lists (cars parked in a spot or station), most urgent first.</summary>
        public void CollectKeyBoardRows(List<TicketRow> into)
        {
            CollectRows(into);
            KeyBoardRows.Filter(into);
        }

        /// <summary>Counts every change that could alter what the HUD shows about parking and pickups.</summary>
        public int Version { get; private set; }

        /// <summary>Clears <paramref name="into"/> and fills it with the open tickets, most urgent first.</summary>
        public void CollectRows(List<TicketRow> into)
        {
            into.Clear();
            foreach (var pair in _rows) into.Add(pair.Value);
            TicketRowOrder.Sort(into);
        }

        private void OnOpened(TicketOpened e)
        {
            Version++;
            var row = new TicketRow { Car = e.Car.Value };
            if (_tags.TryGetValue(e.Car.Value, out var tag)) row.Tag = tag;
            if (_plates.TryGetValue(e.Car.Value, out var plate)) row.Plate = plate;
            row.Arrived = _arrived.Contains(e.Car.Value);
            row.Placed = _placed.Contains(e.Car.Value);
            _rows[e.Ticket.Value] = row;
        }

        private void OnArrived(CarArrived e)
        {
            _arrived.Add(e.Car.Value);
            var row = FindByCar(e.Car.Value);
            if (row != null) row.Arrived = true;
        }

        private void OnDeparted(CarDeparted e)
        {
            _arrived.Remove(e.Car.Value);
            var row = FindByCar(e.Car.Value);
            if (row != null) row.Arrived = false;
        }

        private void OnTag(CarTagAssigned e)
        {
            Version++;
            _tags[e.Car.Value] = e.Tag;
            var row = FindByCar(e.Car.Value);
            if (row != null) row.Tag = e.Tag;
        }

        private void OnPlate(CarPlateAssigned e)
        {
            Version++;
            _plates[e.Car.Value] = e.Plate ?? string.Empty;
            var row = FindByCar(e.Car.Value);
            if (row != null) row.Plate = _plates[e.Car.Value];
        }

        private void OnParked(CarParked e)
        {
            Version++;
            _placed.Add(e.Car.Value);
            var row = FindByCar(e.Car.Value);
            if (row == null) return;
            row.Placed = true;
            row.Spot = e.SpotId;
            if (row.State == TicketRowState.AwaitingPark) row.State = TicketRowState.Parked;
        }

        private void OnEnteredStation(CarEnteredStation e)
        {
            Version++;
            _placed.Add(e.Car.Value);
            var row = FindByCar(e.Car.Value);
            if (row == null) return;
            row.Placed = true;
            row.HasStation = true;
            row.Station = e.Station;
        }

        private void OnLeftStation(CarLeftStation e)
        {
            Version++;
            var row = FindByCar(e.Car.Value);
            if (row != null && row.InStation(e.Station)) row.HasStation = false;
        }

        private void OnLeftSpot(CarLeftSpot e)
        {
            Version++;
            var row = FindByCar(e.Car.Value);
            if (row != null && row.Spot == e.SpotId) row.Spot = 0;
        }

        private void OnPickup(PickupRequested e)
        {
            Version++;
            if (!_rows.TryGetValue(e.Ticket.Value, out var row)) return;
            row.State = TicketRowState.PickupRequested;
            row.HasTimer = false;
            row.Expired = false;
        }

        private void OnTimerChanged(TicketTimerChanged e)
        {
            if (_rows.TryGetValue(e.Ticket.Value, out var row)) row.StartTimer(e.Kind, e.Elapsed01);
        }

        private void OnTimerExpired(TicketTimerExpired e)
        {
            if (!_rows.TryGetValue(e.Ticket.Value, out var row)) return;
            row.StartTimer(e.Kind, 1f);
            row.Expired = true;
        }

        private void OnTimerStopped(TicketTimerStopped e)
        {
            if (_rows.TryGetValue(e.Ticket.Value, out var row)) row.HasTimer = false;
        }

        private void OnClosed(TicketClosed e)
        {
            Version++;
            if (!_rows.TryGetValue(e.Ticket.Value, out var row)) return;
            _rows.Remove(e.Ticket.Value);
            _tags.Remove(row.Car);
            _plates.Remove(row.Car);
            _arrived.Remove(row.Car);
            _placed.Remove(row.Car);
        }

        private TicketRow FindByCar(int car) => TicketNeedsTracker.FindByCar(_rows, car);
    }
}
