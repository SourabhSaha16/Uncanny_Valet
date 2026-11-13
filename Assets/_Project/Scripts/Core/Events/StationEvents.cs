using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;

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

    /// <summary>
    /// A modifier of a car moved toward cleared (published by Patrons, also once when the car's modifiers are announced).
    /// Progress01: 0 untouched, 1 cleared.
    /// </summary>
    public readonly struct ModifierProgressChanged : IGameEvent
    {
        public readonly CarId Car;
        public readonly ModifierId Modifier;
        public readonly float Progress01;
        public ModifierProgressChanged(CarId car, ModifierId modifier, float progress01)
        {
            Car = car; Modifier = modifier; Progress01 = progress01;
        }
    }

    /// <summary>A modifier of a car was cleared at its station.</summary>
    public readonly struct ModifierCleared : IGameEvent
    {
        public readonly CarId Car;
        public readonly ModifierId Modifier;
        public ModifierCleared(CarId car, ModifierId modifier) { Car = car; Modifier = modifier; }
    }
}
