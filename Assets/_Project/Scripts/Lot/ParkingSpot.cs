using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Lot
{
    /// <summary>One parking spot. Publishes CarParked once a car settles and CarLeftSpot when it leaves.</summary>
    public class ParkingSpot : StopZone
    {
        private SpotOccupancy _occupancy;

        public int SpotId { get; private set; }

        public void Init(int spotId, LotConfig config, SpotOccupancy occupancy)
        {
            SpotId = spotId;
            _occupancy = occupancy;
            Setup(config, config.SpotSize);
        }

        protected override void OnSettled(IDrivable car)
        {
            if (_occupancy.Park(SpotId, car.Id)) Bus.Publish(new CarParked(car.Id, SpotId));
        }

        protected override void OnLeft(IDrivable car)
        {
            if (_occupancy.Leave(SpotId, car.Id)) Bus.Publish(new CarLeftSpot(car.Id, SpotId));
        }
    }
}
