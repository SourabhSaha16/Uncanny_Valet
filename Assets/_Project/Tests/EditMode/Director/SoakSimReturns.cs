using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Tests.Director
{
    // The fake Patrons module's side of the return nudge, and the log of when pickups were requested.
    internal sealed partial class SoakSim
    {
        /// <summary>Shift seconds at which each pickup was requested, in order.</summary>
        public List<float> RequestTimes { get; } = new List<float>();
        /// <summary>For each request, whether the car was sitting in a station.</summary>
        public List<bool> RequestFromStation { get; } = new List<bool>();

        private void RecordRequest(SoakCar car)
        {
            RequestTimes.Add(Now);
            RequestFromStation.Add(car.State == CarLotState.InStation);
        }

        // A patron can only be moved while inside the diner: dropped off, not asked for the car yet.
        private void OnReturnShift(ReturnShiftRequested e)
        {
            foreach (var car in _cars)
            {
                if (car.Id.Value != e.Car.Value) continue;
                if (car.State == CarLotState.Arriving || car.State == CarLotState.PickupRequested) return;
                var applied = System.Math.Max(car.StayLeft + e.DeltaMinutes, 0f) - car.StayLeft;
                car.StayLeft += applied;
                car.Shifted += applied;
                _bus.Publish(new PatronReturnShifted(new TicketId(car.Id.Value), car.Id, applied));
                return;
            }
        }
    }
}
