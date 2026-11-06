using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

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

        protected override void OnSettled(CarId car)
        {
            if (_occupancy.Park(SpotId, car)) Bus.Publish(new CarParked(car, SpotId));
        }

        protected override void OnLeft(CarId car)
        {
            if (_occupancy.Leave(SpotId, car)) Bus.Publish(new CarLeftSpot(car, SpotId));
        }
    }
}
