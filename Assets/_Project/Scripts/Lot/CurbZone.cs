using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Lot
{
    /// <summary>The drop-off zone. Publishes CarAtCurb once a car settles inside it.</summary>
    public class CurbZone : StopZone
    {
        public void Init(LotConfig config) => Setup(config, config.CurbSize);

        protected override void OnSettled(IDrivable car) => Bus.Publish(new CarAtCurb(car.Id));

        protected override void OnLeft(IDrivable car) { }
    }
}
