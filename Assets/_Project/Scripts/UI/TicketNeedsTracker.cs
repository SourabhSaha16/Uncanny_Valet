using System;
using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;
using UncannyValet.UI.Kit;

namespace UncannyValet.UI
{
    /// <summary>Fills the service-modifier fields of the board's rows (the list, each modifier's progress and cleared state) from events only.</summary>
    public sealed class TicketNeedsTracker : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly SortedDictionary<int, TicketRow> _rows;
        private readonly IModifierCatalog _catalog;

        public TicketNeedsTracker(IEventBus bus, SortedDictionary<int, TicketRow> rows, IModifierCatalog catalog)
        {
            _bus = bus;
            _rows = rows;
            _catalog = catalog;
            _bus.Subscribe<TicketModifiersAssigned>(OnModifiers);
            _bus.Subscribe<ModifierProgressChanged>(OnProgress);
            _bus.Subscribe<ModifierCleared>(OnCleared);
            _bus.Subscribe<CarParked>(OnParked);
            _bus.Subscribe<CarDamaged>(OnDamaged);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<TicketModifiersAssigned>(OnModifiers);
            _bus.Unsubscribe<ModifierProgressChanged>(OnProgress);
            _bus.Unsubscribe<ModifierCleared>(OnCleared);
            _bus.Unsubscribe<CarParked>(OnParked);
            _bus.Unsubscribe<CarDamaged>(OnDamaged);
        }

        private void OnDamaged(CarDamaged e) { var row = FindByCar(_rows, e.Car.Value); if (row != null) row.Damage01 = e.Damage01; }

        /// <summary>The open row of a car, or null.</summary>
        public static TicketRow FindByCar(SortedDictionary<int, TicketRow> rows, int car)
        {
            foreach (var row in rows.Values) if (row.Car == car) return row;
            return null;
        }

        private void OnModifiers(TicketModifiersAssigned e)
        {
            if (!_rows.TryGetValue(e.Ticket.Value, out var row)) return;
            row.Modifiers.Clear();
            for (var i = 0; i < e.Modifiers.Count; i++)
                if (_catalog.TryGet(e.Modifiers.At(i), out var def)) row.Modifiers.Add(Describe(def));
        }

        private static RowModifier Describe(ServiceModifierDefinition def) => new RowModifier
        {
            Id = def.Id, ClearedAt = def.ClearedAt, Icon = ModifierIcons.ToPixel(def.Icon),
            ActiveWord = def.ActiveWord, DoneWord = def.DoneWord, Progress01 = def.StartProgress01,
        };

        private void OnProgress(ModifierProgressChanged e)
        {
            var modifier = FindByCar(_rows, e.Car.Value)?.Find(e.Modifier);
            if (modifier != null) modifier.Progress01 = e.Progress01;
        }

        private void OnCleared(ModifierCleared e)
        {
            var modifier = FindByCar(_rows, e.Car.Value)?.Find(e.Modifier);
            if (modifier == null) return;
            modifier.IsCleared = true;
            modifier.ShowDone = true;
        }

        /// <summary>Parking ends the short "Washed" / "Charged" moments: the car is simply Parked again.</summary>
        private void OnParked(CarParked e)
        {
            var row = FindByCar(_rows, e.Car.Value);
            if (row == null) return;
            for (var i = 0; i < row.Modifiers.Count; i++) row.Modifiers[i].ShowDone = false;
        }
    }
}
