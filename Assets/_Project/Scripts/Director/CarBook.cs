using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;

namespace UncannyValet.Director
{
    /// <summary>
    /// Pure bookkeeping for the open tickets' cars and which of them sit in the stations. Each method is one
    /// transition; unknown cars are ignored (a car already gone, or one the tracker never saw).
    /// </summary>
    internal sealed class CarBook
    {
        private readonly Dictionary<CarId, CarRecord> _cars = new Dictionary<CarId, CarRecord>();
        private readonly List<CarRecord> _washOccupants = new List<CarRecord>();
        private readonly List<CarRecord> _bayOccupants = new List<CarRecord>();
        private readonly IModifierCatalog _catalog;

        public CarBook(IModifierCatalog catalog, int regularSpots, int chargingBays)
        {
            _catalog = catalog;
            RegularSpots = regularSpots;
            ChargingBays = chargingBays;
        }

        public int RegularSpots { get; }
        public int ChargingBays { get; }

        /// <summary>Regular spots the plan opens (0 in the plan means all).</summary>
        public int ActiveSpots { get; private set; }
        public bool ChargingOpen { get; private set; } = true;
        public bool WashOpen { get; private set; } = true;

        public Dictionary<CarId, CarRecord>.ValueCollection Records => _cars.Values;
        public IReadOnlyList<CarRecord> WashOccupants => _washOccupants;
        public IReadOnlyList<CarRecord> BayOccupants => _bayOccupants;

        public void ApplyPlan(int activeSpots, bool chargingOpen, bool washOpen)
        {
            ActiveSpots = activeSpots;
            ChargingOpen = chargingOpen;
            WashOpen = washOpen;
        }

        public void Clear()
        {
            _cars.Clear();
            _washOccupants.Clear();
            _bayOccupants.Clear();
        }

        public void Spawned(CarId car) => _cars[car] = new CarRecord { Car = car };

        public void Modifiers(CarId car, ModifierSet held)
        {
            if (!_cars.TryGetValue(car, out var r)) return;
            r.Held = held;
            for (var i = 0; i < held.Count; i++) r.Progress[(int)held.At(i)] = StartProgress(held.At(i));
            Refresh(r);
        }

        public void Stay(CarId car, float stayMinutes)
        {
            if (_cars.TryGetValue(car, out var r)) r.StayMinutes = stayMinutes;
        }

        public void ReturnShifted(CarId car, float deltaMinutes)
        {
            if (_cars.TryGetValue(car, out var r)) r.ShiftedMinutes += deltaMinutes;
        }

        public void Arrived(CarId car)
        {
            if (_cars.TryGetValue(car, out var r)) r.Arrived = true;
        }

        public void StayStarted(CarId car, float minute)
        {
            if (_cars.TryGetValue(car, out var r) && float.IsNaN(r.StayStartedAtMinute)) r.StayStartedAtMinute = minute;
        }

        public void Driven(CarId car, bool driven) { if (_cars.TryGetValue(car, out var r)) r.Driven = driven; }

        public void Parked(CarId car, int spotId)
        {
            if (!_cars.TryGetValue(car, out var r)) return;
            r.SpotId = spotId;
            r.AwaitingPark = false;
        }

        public void LeftSpot(CarId car, int spotId)
        {
            if (_cars.TryGetValue(car, out var r) && r.SpotId == spotId) r.SpotId = CarRecord.NoSpot;
        }

        public void StationEntered(CarId car, StationKind kind)
        {
            if (!_cars.TryGetValue(car, out var r)) return;
            Leave(r);
            r.InStation = true;
            r.Station = kind;
            (kind == StationKind.Wash ? _washOccupants : _bayOccupants).Add(r);
        }

        public void StationLeft(CarId car, StationKind kind)
        {
            if (_cars.TryGetValue(car, out var r) && r.InStation && r.Station == kind) Leave(r);
        }

        public void Progress(CarId car, ModifierId modifier, float progress01)
        {
            if (!_cars.TryGetValue(car, out var r) || !r.Held.Contains(modifier)) return;
            r.Progress[(int)modifier] = progress01;
            Refresh(r);
        }

        public void Cleared(CarId car, ModifierId modifier)
        {
            if (!_cars.TryGetValue(car, out var r) || !r.Held.Contains(modifier)) return;
            r.Done = r.Done.With(modifier);
            r.Progress[(int)modifier] = 1f;
            Refresh(r);
        }

        public void PickupRequested(CarId car) { if (_cars.TryGetValue(car, out var r)) r.PickupRequested = true; }

        /// <summary>Records pickup progress; the pickup timer is the only ticket timer.</summary>
        public void TimerChanged(CarId car, float elapsed01)
        {
            if (_cars.TryGetValue(car, out var r)) r.PickupElapsed01 = elapsed01;
        }

        public void Closed(CarId car)
        {
            if (!_cars.TryGetValue(car, out var r)) return;
            Leave(r);
            _cars.Remove(car);
        }

        private float StartProgress(ModifierId id) => _catalog.TryGet(id, out var def) ? def.StartProgress01 : 0f;

        // Rebuilds the per-station view of the car's modifier list.
        private void Refresh(CarRecord r)
        {
            r.HeldStations = 0;
            for (var i = 0; i < r.LeftAtStation.Length; i++) r.LeftAtStation[i] = 0f;
            for (var i = 0; i < r.Held.Count; i++)
            {
                var id = r.Held.At(i);
                if (!_catalog.TryGet(id, out var def)) continue;
                r.HeldStations |= 1 << (int)def.ClearedAt;
                if (!r.Done.Contains(id)) r.LeftAtStation[(int)def.ClearedAt] += 1f - r.Progress[(int)id];
            }
        }

        private void Leave(CarRecord r)
        {
            if (!r.InStation) return;
            r.InStation = false;
            (r.Station == StationKind.Wash ? _washOccupants : _bayOccupants).Remove(r);
        }
    }
}
