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

    /// <summary>The car was given its display letter (A, B, C) for as long as its ticket is open.</summary>
    public readonly struct CarLetterAssigned : IGameEvent
    {
        public readonly CarId Car;
        public readonly char Letter;
        public CarLetterAssigned(CarId car, char letter) { Car = car; Letter = letter; }
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
}
