using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Events
{
    // Ticket events, grouped in one file by domain.

    /// <summary>A patron arrived and handed over keys. StayMinutes is game minutes until pickup.</summary>
    public readonly struct TicketOpened : IGameEvent
    {
        public readonly TicketId Ticket;
        public readonly CarId Car;
        public readonly float StayMinutes;
        public TicketOpened(TicketId ticket, CarId car, float stayMinutes)
        {
            Ticket = ticket; Car = car; StayMinutes = stayMinutes;
        }
    }

    /// <summary>The patron wants the car back at the curb.</summary>
    public readonly struct PickupRequested : IGameEvent
    {
        public readonly TicketId Ticket;
        public readonly CarId Car;
        public PickupRequested(TicketId ticket, CarId car) { Ticket = ticket; Car = car; }
    }

    /// <summary>The car was handed back. WaitMinutes is time from pickup request to return.</summary>
    public readonly struct TicketClosed : IGameEvent
    {
        public readonly TicketId Ticket;
        public readonly CarId Car;
        public readonly float WaitMinutes;
        public readonly int Tip;
        public TicketClosed(TicketId ticket, CarId car, float waitMinutes, int tip)
        {
            Ticket = ticket; Car = car; WaitMinutes = waitMinutes; Tip = tip;
        }
    }
}
