using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

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

        public CarBook(int regularSpots, int chargingBays)
        {
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

        public void Traits(CarId car, bool isEv, bool isDirty)
        {
            if (!_cars.TryGetValue(car, out var r)) return;
            r.IsEv = isEv; r.IsDirty = isDirty;
        }

        public void Stay(CarId car, float stayMinutes)
        {
            if (_cars.TryGetValue(car, out var r)) r.StayMinutes = stayMinutes;
        }

        public void Arrived(CarId car) { if (_cars.TryGetValue(car, out var r)) r.Arrived = true; }

        public void Driven(CarId car, bool driven) { if (_cars.TryGetValue(car, out var r)) r.Driven = driven; }

        public void Parked(CarId car, int spotId, float nowMinutes)
        {
            if (!_cars.TryGetValue(car, out var r)) return;
            r.SpotId = spotId;
            r.AwaitingPark = false;
            if (float.IsNaN(r.ParkedAtMinute)) r.ParkedAtMinute = nowMinutes;
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

        public void CleanChanged(CarId car, float clean01) { if (_cars.TryGetValue(car, out var r)) r.Clean01 = clean01; }

        public void Cleaned(CarId car)
        {
            if (!_cars.TryGetValue(car, out var r)) return;
            r.WashDone = true;
            r.Clean01 = 1f;
        }

        public void FullyCharged(CarId car) { if (_cars.TryGetValue(car, out var r)) r.ChargeDone = true; }

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

        private void Leave(CarRecord r)
        {
            if (!r.InStation) return;
            r.InStation = false;
            (r.Station == StationKind.Wash ? _washOccupants : _bayOccupants).Remove(r);
        }
    }
}
