using System;
using System.Collections.Generic;
using System.Text;
using UncannyValet.Core.Events;
using UncannyValet.UI.Hud;

namespace UncannyValet.UI
{
    /// <summary>Tracks open tickets, tag colour, licence plate, spot and timers from events only. The HUD reads the rows.</summary>
    public sealed class TicketBoard : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly SortedDictionary<int, TicketRow> _rows = new SortedDictionary<int, TicketRow>();
        private readonly Dictionary<int, int> _tags = new Dictionary<int, int>();
        private readonly Dictionary<int, string> _plates = new Dictionary<int, string>();
        private readonly HashSet<int> _arrived = new HashSet<int>();

        public int Count => _rows.Count;
        public int PickupCount { get; private set; }

        public TicketBoard(IEventBus bus)
        {
            _bus = bus;
            _bus.Subscribe<TicketOpened>(OnOpened);
            _bus.Subscribe<CarTagAssigned>(OnTag);
            _bus.Subscribe<CarPlateAssigned>(OnPlate);
            _bus.Subscribe<CarParked>(OnParked);
            _bus.Subscribe<CarLeftSpot>(OnLeftSpot);
            _bus.Subscribe<PickupRequested>(OnPickup);
            _bus.Subscribe<TicketTimerChanged>(OnTimerChanged);
            _bus.Subscribe<TicketTimerExpired>(OnTimerExpired);
            _bus.Subscribe<TicketTimerStopped>(OnTimerStopped);
            _bus.Subscribe<TicketClosed>(OnClosed);
            _bus.Subscribe<TicketTraitsAssigned>(OnTraits);
            _bus.Subscribe<CarChargeChanged>(OnCharge);
            _bus.Subscribe<CarCleaned>(OnCleaned);
            _bus.Subscribe<CarCleanChanged>(OnCleanChanged);
            _bus.Subscribe<CarArrived>(OnArrived);
            _bus.Subscribe<CarDeparted>(OnDeparted);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<TicketOpened>(OnOpened);
            _bus.Unsubscribe<CarTagAssigned>(OnTag);
            _bus.Unsubscribe<CarPlateAssigned>(OnPlate);
            _bus.Unsubscribe<CarParked>(OnParked);
            _bus.Unsubscribe<CarLeftSpot>(OnLeftSpot);
            _bus.Unsubscribe<PickupRequested>(OnPickup);
            _bus.Unsubscribe<TicketTimerChanged>(OnTimerChanged);
            _bus.Unsubscribe<TicketTimerExpired>(OnTimerExpired);
            _bus.Unsubscribe<TicketTimerStopped>(OnTimerStopped);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
            _bus.Unsubscribe<TicketTraitsAssigned>(OnTraits);
            _bus.Unsubscribe<CarChargeChanged>(OnCharge);
            _bus.Unsubscribe<CarCleaned>(OnCleaned);
            _bus.Unsubscribe<CarCleanChanged>(OnCleanChanged);
            _bus.Unsubscribe<CarArrived>(OnArrived);
            _bus.Unsubscribe<CarDeparted>(OnDeparted);
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

        /// <summary>
        /// Writes one line per pending pickup (at most maxLines, then "+N more") and returns the line count; see
        /// <see cref="PickupBannerFormatter.AppendAll"/>. <paramref name="tags"/> receives each line's tag colour.
        /// </summary>
        public int AppendPickups(StringBuilder text, int maxLines, List<int> tags = null)
            => PickupBannerFormatter.AppendAll(text, _rows.Values, maxLines, tags);

        private void OnOpened(TicketOpened e)
        {
            Version++;
            var row = new TicketRow { Car = e.Car.Value };
            if (_tags.TryGetValue(e.Car.Value, out var tag)) row.Tag = tag;
            if (_plates.TryGetValue(e.Car.Value, out var plate)) row.Plate = plate;
            row.Arrived = _arrived.Contains(e.Car.Value);
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

        private void OnCleanChanged(CarCleanChanged e) { var row = FindByCar(e.Car.Value); if (row != null) row.Clean01 = e.Clean01; }

        private void OnTraits(TicketTraitsAssigned e)
        {
            if (!_rows.TryGetValue(e.Ticket.Value, out var row)) return;
            row.IsEv = e.IsEv;
            row.IsDirty = e.IsDirty;
        }

        private void OnCharge(CarChargeChanged e) { var row = FindByCar(e.Car.Value); if (row != null) row.Charge01 = e.Charge01; }

        private void OnCleaned(CarCleaned e) { var row = FindByCar(e.Car.Value); if (row != null) row.Washed = true; }

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
            var row = FindByCar(e.Car.Value);
            if (row == null) return;
            row.Spot = e.SpotId;
            if (row.State == TicketRowState.AwaitingPark) row.State = TicketRowState.Parked;
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
            PickupCount++;
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
            if (row.State == TicketRowState.PickupRequested && PickupCount > 0) PickupCount--;
            _rows.Remove(e.Ticket.Value);
            _tags.Remove(row.Car);
            _plates.Remove(row.Car);
            _arrived.Remove(row.Car);
        }

        private TicketRow FindByCar(int car)
        {
            foreach (var row in _rows.Values) if (row.Car == car) return row;
            return null;
        }
    }
}
