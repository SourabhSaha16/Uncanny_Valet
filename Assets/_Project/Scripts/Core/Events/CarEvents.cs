using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Events
{
    // Car events, grouped in one file by domain (the one allowed exception to one type per file).

    /// <summary>A car was created at the arrival point for a ticket.</summary>
    public readonly struct CarSpawned : IGameEvent
    {
        public readonly CarId Car;
        public readonly TicketId Ticket;
        public CarSpawned(CarId car, TicketId ticket) { Car = car; Ticket = ticket; }
    }

    /// <summary>The car was given its key-tag colour (an index into TagPalette) for as long as its ticket is open.</summary>
    public readonly struct CarTagAssigned : IGameEvent
    {
        public readonly CarId Car;
        public readonly int Tag;
        public CarTagAssigned(CarId car, int tag) { Car = car; Tag = tag; }
    }

    /// <summary>The car was given its licence plate (for example "KT 4"), unique among open tickets, until its ticket ends.</summary>
    public readonly struct CarPlateAssigned : IGameEvent
    {
        public readonly CarId Car;
        public readonly string Plate;
        public CarPlateAssigned(CarId car, string plate) { Car = car; Plate = plate; }
    }

    /// <summary>An arriving car finished driving in and stopped; it is now on the lot like any other car.</summary>
    public readonly struct CarArrived : IGameEvent
    {
        public readonly CarId Car;
        public CarArrived(CarId car) { Car = car; }
    }

    /// <summary>The valet got into the driver's seat.</summary>
    public readonly struct CarEntered : IGameEvent
    {
        public readonly CarId Car;
        public CarEntered(CarId car) { Car = car; }
    }

    /// <summary>The valet got out of the car.</summary>
    public readonly struct CarExited : IGameEvent
    {
        public readonly CarId Car;
        public CarExited(CarId car) { Car = car; }
    }

    /// <summary>A car is stopped inside a parking spot with nobody in it.</summary>
    public readonly struct CarParked : IGameEvent
    {
        public readonly CarId Car;
        public readonly int SpotId;
        public CarParked(CarId car, int spotId) { Car = car; SpotId = spotId; }
    }

    /// <summary>A parked car left its spot.</summary>
    public readonly struct CarLeftSpot : IGameEvent
    {
        public readonly CarId Car;
        public readonly int SpotId;
        public CarLeftSpot(CarId car, int spotId) { Car = car; SpotId = spotId; }
    }

    /// <summary>A car is stopped in the curb zone with nobody in it.</summary>
    public readonly struct CarAtCurb : IGameEvent
    {
        public readonly CarId Car;
        public CarAtCurb(CarId car) { Car = car; }
    }

    /// <summary>The patron drove the car away; it is about to be despawned.</summary>
    public readonly struct CarDeparted : IGameEvent
    {
        public readonly CarId Car;
        public CarDeparted(CarId car) { Car = car; }
    }

    /// <summary>
    /// Two cars bumped into each other (published by Cars, once per pair at most every 0.25 s). WorldX / WorldY locate
    /// the contact; Strength01 is the impact from 0 (a nudge) to 1 (a hard hit).
    /// </summary>
    public readonly struct CarsCollided : IGameEvent
    {
        public readonly float WorldX;
        public readonly float WorldY;
        public readonly float Strength01;
        public CarsCollided(float worldX, float worldY, float strength01) { WorldX = worldX; WorldY = worldY; Strength01 = strength01; }
    }

    /// <summary>
    /// An empty car resting in the curb zone is (Wrong = true) or is no longer facing the wrong way. Drives the
    /// "please park it properly" hint over the patron. Also published with Wrong = false when the car leaves.
    /// </summary>
    public readonly struct CurbFacingChanged : IGameEvent
    {
        public readonly CarId Car;
        public readonly bool Wrong;
        public CurbFacingChanged(CarId car, bool wrong) { Car = car; Wrong = wrong; }
    }
}
