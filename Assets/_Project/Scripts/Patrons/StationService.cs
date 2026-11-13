using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Tracks which station each car is stopped in and advances the modifiers each game minute: a station works on one
    /// open modifier of the car it holds (the first one it clears), so two modifiers of one kind clear one after the other.
    /// </summary>
    public sealed class StationService
    {
        private readonly IEventBus _bus;
        private readonly TicketBook _book;
        private readonly List<Ticket> _scratch = new List<Ticket>();

        public StationService(IEventBus bus, TicketBook book)
        {
            _bus = bus; _book = book;
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

        /// <summary>Publishes the starting progress of every modifier of a new car.</summary>
        public void Announce(Ticket t)
        {
            for (var i = 0; i < t.Modifiers.Count; i++)
                _bus.Publish(new ModifierProgressChanged(t.CarId, t.Modifiers[i].Id, t.Modifiers[i].Progress01));
        }

        private void OnEntered(CarEnteredStation e) => SetStation(e.Car, e.Station, true);

        private void OnLeft(CarLeftStation e) => SetStation(e.Car, e.Station, false);

        private void SetStation(Core.Ids.CarId car, StationKind kind, bool inside)
        {
            if (_book.TryGetByCar(car, out var t)) t.SetInStation(kind, inside);
        }

        private void OnShiftTick(ShiftTick e)
        {
            _book.CopyTo(_scratch);
            foreach (var t in _scratch) Work(t);
        }

        // Per station kind only the first open modifier it clears gets this minute.
        private void Work(Ticket t)
        {
            var served = 0;
            for (var i = 0; i < t.Modifiers.Count; i++)
            {
                var m = t.Modifiers[i];
                var bit = 1 << (int)m.ClearedAt;
                if (m.IsCleared || (served & bit) != 0) continue;
                served |= bit;
                if (m.Advance(t.InStation(m.ClearedAt))) Publish(t, m);
            }
        }

        private void Publish(Ticket t, ModifierInstance m)
        {
            _bus.Publish(new ModifierProgressChanged(t.CarId, m.Id, m.Progress01));
            if (m.IsCleared) _bus.Publish(new ModifierCleared(t.CarId, m.Id));
        }
    }
}
