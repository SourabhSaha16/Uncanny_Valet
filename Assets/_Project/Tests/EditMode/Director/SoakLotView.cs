using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    /// <summary>Builds the director's lot snapshot from the soak test's fake lot.</summary>
    internal sealed class SoakLotView : ILotSnapshotSource
    {
        private readonly SoakSim _sim;
        private readonly List<CarSnapshot> _cars = new List<CarSnapshot>();
        private readonly List<StationSlot> _slots = new List<StationSlot>();

        public SoakLotView(SoakSim sim) { _sim = sim; }

        public LotSnapshot Capture()
        {
            _cars.Clear();
            _slots.Clear();
            var nextPickup = float.PositiveInfinity;
            foreach (var car in _sim.Cars)
            {
                var toPickup = car.State == CarLotState.Parked || car.State == CarLotState.InStation
                    ? car.StayLeft : float.PositiveInfinity;
                if (toPickup < nextPickup) nextPickup = toPickup;
                _cars.Add(new CarSnapshot(car.Id, car.State, car.Dirty, car.Ev, car.WashDone, car.ChargeDone,
                    car.AwaitingPark, WashLeft01(car), Patience(car), toPickup));
            }
            _slots.Add(SlotOf(StationKind.Wash, _sim.WashSlot));
            foreach (var bay in _sim.BaySlots) _slots.Add(SlotOf(StationKind.Charging, bay));
            return new LotSnapshot(_cars, _slots, FreeSpots(), SoakSim.RegularSpots, nextPickup, _sim.Now, _sim.AngryTimes);
        }

        private static float WashLeft01(SoakCar car)
        {
            if (!car.NeedsWash) return 0f;
            return car.InWash ? car.WashLeft / SoakSim.WashSeconds : 1f;
        }

        private static float Patience(SoakCar car)
            => car.State == CarLotState.PickupRequested ? 1f - car.PickupWaited / SoakSim.PickupWindowSeconds : 1f;

        private static StationSlot SlotOf(StationKind kind, SoakCar occupant)
        {
            if (occupant == null) return new StationSlot(kind, StationSlotState.Free, CarId.None);
            var state = SlotStateRule.Occupied(kind, occupant.Dirty, occupant.WashDone, occupant.Ev, occupant.ChargeDone);
            return new StationSlot(kind, state, occupant.Id);
        }

        private int FreeSpots()
        {
            var free = 0;
            for (var i = 1; i < _sim.Spots.Length; i++) if (_sim.Spots[i] == null) free++;
            return free;
        }
    }
}
