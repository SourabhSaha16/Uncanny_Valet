using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Lot
{
    /// <summary>
    /// A charging bay or the wash slot. Publishes CarEnteredStation / CarLeftStation for its kind. A station with a
    /// <see cref="StationBlocker"/> holds one car at a time.
    /// </summary>
    public class StationZone : StopZone
    {
        private StationBlocker _blocker;

        public StationKind Kind { get; private set; }

        public void Init(StationKind kind, LotConfig config) => Init(kind, config, config.SpotSize);

        /// <summary>Same, for a station of its own <paramref name="size"/> instead of the config's spot size.</summary>
        public void Init(StationKind kind, LotConfig config, UnityEngine.Vector2 size)
        {
            Kind = kind;
            Setup(config, size);
        }

        /// <summary>Makes the station single-occupancy: while a car holds it, other cars are kept out.</summary>
        public void UseBlocker(StationBlocker blocker) => _blocker = blocker;

        protected override void OnSettled(CarId car)
        {
            if (_blocker != null && TryGetCar(car, out var drivable)) _blocker.Admit(drivable);
            Bus.Publish(new CarEnteredStation(car, Kind));
        }

        protected override void OnLeft(CarId car)
        {
            if (_blocker != null) _blocker.Release(car);
            Bus.Publish(new CarLeftStation(car, Kind));
        }
    }
}
