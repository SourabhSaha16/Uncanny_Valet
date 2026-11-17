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
                var counting = car.State == CarLotState.WaitingToPark || car.State == CarLotState.Parked
                    || car.State == CarLotState.InStation;
                var toPickup = counting && car.StayStarted ? car.StayLeft : float.PositiveInfinity;
                if (toPickup < nextPickup) nextPickup = toPickup;
                _cars.Add(new CarSnapshot(car.Id, car.State, car.Held, car.Done, car.AwaitingPark, Patience(car), toPickup,
                    car.PlannedStay, car.Shifted, LeftAtStation(car)));
            }
            _slots.Add(SlotOf(StationKind.Wash, _sim.WashSlot));
            foreach (var bay in _sim.BaySlots) _slots.Add(SlotOf(StationKind.Charging, bay));
            return new LotSnapshot(_cars, _slots, FreeSpots(), SoakSim.RegularSpots, nextPickup, _sim.Now, _sim.AngryTimes);
        }

        private static float[] LeftAtStation(SoakCar car)
        {
            var left = new float[CarSnapshot.StationKindCount];
            if (car.NeedsWash) left[(int)StationKind.Wash] = car.InWash ? car.WashLeft / SoakSim.WashSeconds : 1f;
            if (car.NeedsCharge) left[(int)StationKind.Charging] = 1f;
            return left;
        }

        private static float Patience(SoakCar car)
            => car.State == CarLotState.PickupRequested ? 1f - car.PickupWaited / SoakSim.PickupWindowSeconds : 1f;

        private static StationSlot SlotOf(StationKind kind, SoakCar occupant)
        {
            if (occupant == null) return new StationSlot(kind, StationSlotState.Free, CarId.None);
            var holds = kind == StationKind.Wash ? occupant.Muddy : occupant.Ev;
            var open = kind == StationKind.Wash ? occupant.NeedsWash : occupant.NeedsCharge;
            var state = SlotStateRule.Occupied(holds, open);
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
