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
}
