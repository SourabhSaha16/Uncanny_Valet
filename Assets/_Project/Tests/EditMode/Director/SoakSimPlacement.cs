using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Tests.Director
{
    // Where the bot parks a car and how it gets there.
    internal sealed partial class SoakSim
    {
        private enum Where { Spot, Wash, Bay }

        private readonly struct Destination
        {
            public readonly Where Where;
            public readonly int Index;
            public Destination(Where where, int index) { Where = where; Index = index; }
        }

        private Destination? DestinationOf(SoakCar car)
        {
            if (car.NeedsWash && WashSlot == null) return new Destination(Where.Wash, 0);
            var bay = FreeBay();
            if (car.NeedsCharge && bay >= 0) return new Destination(Where.Bay, bay);
            var spot = FreeSpot();
            if (spot > 0) return new Destination(Where.Spot, spot);
            if (bay >= 0) return new Destination(Where.Bay, bay);
            if (WashSlot == null) return new Destination(Where.Wash, 0);
            return null;
        }

        private int FreeBay()
        {
            for (var i = 0; i < BaySlots.Length; i++) if (BaySlots[i] == null) return i;
            return -1;
        }

        private int FreeSpot()
        {
            for (var i = 1; i < Spots.Length; i++) if (Spots[i] == null) return i;
            return 0;
        }

        private void Place(SoakCar car)
        {
            var dest = DestinationOf(car);
            if (!dest.HasValue) { car.State = CarLotState.WaitingToPark; return; }
            var d = dest.Value;
            if (car.ParkedAt <= 0f) car.ParkedAt = Now;
            switch (d.Where)
            {
                case Where.Spot:
                    Spots[d.Index] = car;
                    car.Spot = d.Index;
                    car.State = CarLotState.Parked;
                    _bus.Publish(new CarParked(car.Id, d.Index));
                    break;
                case Where.Wash:
                    WashSlot = car;
                    car.InWash = true;
                    car.WashLeft = WashSeconds;
                    car.State = CarLotState.InStation;
                    _bus.Publish(new CarEnteredStation(car.Id, StationKind.Wash));
                    break;
                default:
                    BaySlots[d.Index] = car;
                    car.Bay = d.Index;
                    car.ChargeLeft = ChargeSeconds;
                    car.State = CarLotState.InStation;
                    _bus.Publish(new CarEnteredStation(car.Id, StationKind.Charging));
                    break;
            }
        }
    }
}
