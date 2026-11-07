using System;
using System.Collections.Generic;
using System.Text;
using UncannyValet.Core.Events;

namespace UncannyValet.UI
{
    /// <summary>Tracks open tickets, letter, spot and timers from events only. Text comes from TicketRowFormatter.</summary>
    public sealed class TicketBoard : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly SortedDictionary<int, TicketRow> _rows = new SortedDictionary<int, TicketRow>();
        private readonly Dictionary<int, char> _letters = new Dictionary<int, char>();

        public int Count => _rows.Count;
        public int PickupCount { get; private set; }

        public TicketBoard(IEventBus bus)
        {
            _bus = bus;
            _bus.Subscribe<TicketOpened>(OnOpened);
            _bus.Subscribe<CarLetterAssigned>(OnLetter);
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
        }

        public void Dispose()
        {
            _bus.Unsubscribe<TicketOpened>(OnOpened);
            _bus.Unsubscribe<CarLetterAssigned>(OnLetter);
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
        }

        public void AppendLines(StringBuilder text, int maxLines)
        {
            var shown = 0;
            foreach (var pair in _rows)
            {
                if (shown++ >= maxLines)
                {
                    text.Append("  +").Append(_rows.Count - maxLines).AppendLine(" more");
                    break;
                }
                TicketRowFormatter.Append(text, pair.Value);
                text.AppendLine();
            }
        }

        /// <summary>
        /// Writes one line per pending pickup (at most maxLines, then "+N more"), separated by newlines
        /// with none at the end. Returns the number of lines written. Allocation-free apart from the builder.
        /// </summary>
        public int AppendPickups(StringBuilder text, int maxLines)
        {
            var lines = 0;
            var hidden = 0;
            foreach (var pair in _rows)
            {
                var row = pair.Value;
                if (row.State != TicketRowState.PickupRequested) continue;
                if (lines >= maxLines) { hidden++; continue; }
                if (lines++ > 0) text.Append('\n');
                PickupBannerFormatter.AppendPickup(text, row.Letter, row.Spot);
            }
            if (hidden == 0) return lines;
            text.Append('\n');
            PickupBannerFormatter.AppendOverflow(text, hidden);
            return lines + 1;
        }

        private void OnOpened(TicketOpened e)
        {
            var row = new TicketRow { Car = e.Car.Value };
            if (_letters.TryGetValue(e.Car.Value, out var letter)) row.Letter = letter;
            _rows[e.Ticket.Value] = row;
        }

        private void OnTraits(TicketTraitsAssigned e)
        {
            if (!_rows.TryGetValue(e.Ticket.Value, out var row)) return;
            row.IsEv = e.IsEv;
            row.IsDirty = e.IsDirty;
        }

        private void OnCharge(CarChargeChanged e)
        {
            var row = FindByCar(e.Car.Value);
            if (row != null) row.Charge01 = e.Charge01;
        }

        private void OnCleaned(CarCleaned e)
        {
            var row = FindByCar(e.Car.Value);
            if (row != null) row.Washed = true;
        }

        private void OnLetter(CarLetterAssigned e)
        {
            _letters[e.Car.Value] = e.Letter;
            var row = FindByCar(e.Car.Value);
            if (row != null) row.Letter = e.Letter;
        }

        private void OnParked(CarParked e)
        {
            var row = FindByCar(e.Car.Value);
            if (row == null) return;
            row.Spot = e.SpotId;
            if (row.State == TicketRowState.AwaitingPark) row.State = TicketRowState.Parked;
        }

        private void OnLeftSpot(CarLeftSpot e)
        {
            var row = FindByCar(e.Car.Value);
            if (row != null && row.Spot == e.SpotId) row.Spot = 0;
        }

        private void OnPickup(PickupRequested e)
        {
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
            if (!_rows.TryGetValue(e.Ticket.Value, out var row)) return;
            if (row.State == TicketRowState.PickupRequested && PickupCount > 0) PickupCount--;
            _rows.Remove(e.Ticket.Value);
            _letters.Remove(row.Car);
        }

        private TicketRow FindByCar(int car)
        {
            foreach (var row in _rows.Values) if (row.Car == car) return row;
            return null;
        }
    }
}
