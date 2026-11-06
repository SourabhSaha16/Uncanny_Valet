using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Lot
{
    /// <summary>A charging bay or the wash slot. Publishes CarEnteredStation / CarLeftStation for its kind.</summary>
    public class StationZone : StopZone
    {
        public StationKind Kind { get; private set; }

        public void Init(StationKind kind, LotConfig config)
        {
            Kind = kind;
            Setup(config, config.StationSize);
        }

        protected override void OnSettled(CarId car) => Bus.Publish(new CarEnteredStation(car, Kind));

        protected override void OnLeft(CarId car) => Bus.Publish(new CarLeftStation(car, Kind));
    }
}
