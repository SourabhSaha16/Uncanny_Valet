using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Events
{
    // Parking events, grouped in one file by domain.

    /// <summary>
    /// A car came to rest in a bay, centred and aligned (published by Lot). Ticket is TicketId.None when the car has no
    /// ticket. Patrons adds the Perfect Park bonus to that ticket; HUD, audio, camera and results react to it.
    /// </summary>
    public readonly struct PerfectPark : IGameEvent
    {
        public readonly CarId Car;
        public readonly TicketId Ticket;
        public PerfectPark(CarId car, TicketId ticket) { Car = car; Ticket = ticket; }
    }

    /// <summary>
    /// A car came to rest completely inside a bay (published by Lot alongside CarParked, before any PerfectPark). A car
    /// that only touches the lines or sticks out never raises it. The power bar adds a little for it; the bay border flashes green.
    /// </summary>
    public readonly struct CarParkedInside : IGameEvent
    {
        public readonly CarId Car;
        public readonly int SpotId;
        public CarParkedInside(CarId car, int spotId) { Car = car; SpotId = spotId; }
    }
}
