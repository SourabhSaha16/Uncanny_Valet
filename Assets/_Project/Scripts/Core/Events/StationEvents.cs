using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Events
{
    // Station and car-service events, grouped in one file by domain.

    /// <summary>A car stopped in a station with nobody in it (published by Lot).</summary>
    public readonly struct CarEnteredStation : IGameEvent
    {
        public readonly CarId Car;
        public readonly StationKind Station;
        public CarEnteredStation(CarId car, StationKind station) { Car = car; Station = station; }
    }

    /// <summary>The car moved out of the station, or was destroyed in it (published by Lot).</summary>
    public readonly struct CarLeftStation : IGameEvent
    {
        public readonly CarId Car;
        public readonly StationKind Station;
        public CarLeftStation(CarId car, StationKind station) { Car = car; Station = station; }
    }

    /// <summary>
    /// A car driven by the valet bumped into a single-occupancy station that is already taken (published by Lot, repeated
    /// while the car keeps touching it). WorldX / WorldY locate the station for the "OCCUPIED" hint.
    /// </summary>
    public readonly struct StationEntryRefused : IGameEvent
    {
        public readonly StationKind Station;
        public readonly float WorldX;
        public readonly float WorldY;
        public StationEntryRefused(StationKind station, float worldX, float worldY) { Station = station; WorldX = worldX; WorldY = worldY; }
    }

    /// <summary>An EV's charge changed (published by Patrons). Charge01: 0 empty, 1 full.</summary>
    public readonly struct CarChargeChanged : IGameEvent
    {
        public readonly CarId Car;
        public readonly float Charge01;
        public CarChargeChanged(CarId car, float charge01) { Car = car; Charge01 = charge01; }
    }

    /// <summary>An EV reached a full charge.</summary>
    public readonly struct CarFullyCharged : IGameEvent
    {
        public readonly CarId Car;
        public CarFullyCharged(CarId car) { Car = car; }
    }

    /// <summary>A dirty car is being washed (published by Patrons). Clean01: 0 filthy, 1 spotless.</summary>
    public readonly struct CarCleanChanged : IGameEvent
    {
        public readonly CarId Car;
        public readonly float Clean01;
        public CarCleanChanged(CarId car, float clean01) { Car = car; Clean01 = clean01; }
    }

    /// <summary>A dirty car finished washing.</summary>
    public readonly struct CarCleaned : IGameEvent
    {
        public readonly CarId Car;
        public CarCleaned(CarId car) { Car = car; }
    }
}
