using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Lot
{
    /// <summary>The drop-off zone. Publishes CarAtCurb once a car settles inside it.</summary>
    public class CurbZone : StopZone
    {
        public void Init(LotConfig config) => Setup(config, config.CurbSize);

        protected override void OnSettled(CarId car) => Bus.Publish(new CarAtCurb(car));

        protected override void OnLeft(CarId car) { }
    }
}
