using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Lot
{
    /// <summary>
    /// One parking spot. Publishes CarParked once a car settles and CarLeftSpot when it leaves. A charging
    /// spot also publishes CarEnteredStation / CarLeftStation (StationKind.Charging) for any car.
    /// </summary>
    public class ParkingSpot : StopZone
    {
        private SpotOccupancy _occupancy;
        private bool _charging;
        private PerfectRunner _judge;
        private BayFitRule _fit;
        private readonly CarOutline _outline = new CarOutline();

        public int SpotId { get; private set; }

        /// <param name="judge">Told when a car comes to rest here so it can judge a Perfect Park; null for none.</param>
        public void Init(int spotId, LotConfig config, SpotOccupancy occupancy, bool charging, PerfectRunner judge = null)
        {
            _judge = judge;
            SpotId = spotId;
            _occupancy = occupancy;
            _charging = charging;
            _fit = new BayFitRule(config.SpotSize * 0.5f, config.Perfect.ParkedInsideTolerance);
            Setup(config, config.SpotSize);
        }

        protected override void OnSettled(CarId car)
        {
            if (_occupancy.TryGetCar(SpotId, out var held) && held == car) return;
            if (!_occupancy.Park(SpotId, car)) return;
            Announce(car);
            if (TryGetCar(car, out var parked)) AnnounceInside(parked);
            if (_judge != null && TryGetCar(car, out var drivable)) _judge.Parked(transform, drivable);
        }

        private void Announce(CarId car)
        {
            Bus.Publish(new CarParked(car, SpotId));
            if (_charging) Bus.Publish(new CarEnteredStation(car, StationKind.Charging));
        }

        private void AnnounceInside(IDrivable car)
        {
            var count = _outline.Read(transform, car.Transform);
            if (_fit.IsInside(_outline.Points, count)) Bus.Publish(new CarParkedInside(car.Id, SpotId));
        }

        protected override void OnLeft(CarId car)
        {
            if (!_occupancy.Leave(SpotId, car)) return;
            Bus.Publish(new CarLeftSpot(car, SpotId));
            if (_charging) Bus.Publish(new CarLeftStation(car, StationKind.Charging));
        }
    }
}
