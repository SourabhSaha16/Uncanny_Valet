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

    /// <summary>The traits of a new patron car, published right after TicketOpened.</summary>
    public readonly struct TicketTraitsAssigned : IGameEvent
    {
        public readonly TicketId Ticket;
        public readonly CarId Car;
        public readonly bool IsEv;
        public readonly bool IsDirty;
        public TicketTraitsAssigned(TicketId ticket, CarId car, bool isEv, bool isDirty)
        {
            Ticket = ticket; Car = car; IsEv = isEv; IsDirty = isDirty;
        }
    }

    /// <summary>How a ticket's tip was made up, published just before TicketClosed.</summary>
    public readonly struct TipExplained : IGameEvent
    {
        public readonly TicketId Ticket;
        public readonly CarId Car;
        public readonly TipBreakdown Breakdown;
        public TipExplained(TicketId ticket, CarId car, TipBreakdown breakdown)
        {
            Ticket = ticket; Car = car; Breakdown = breakdown;
        }
    }

    /// <summary>The patron wants the car back at the curb.</summary>
    public readonly struct PickupRequested : IGameEvent
    {
        public readonly TicketId Ticket;
        public readonly CarId Car;
        public PickupRequested(TicketId ticket, CarId car) { Ticket = ticket; Car = car; }
    }

    /// <summary>Progress of a ticket timer, published once per game minute while it runs.</summary>
    public readonly struct TicketTimerChanged : IGameEvent
    {
        public readonly TicketId Ticket;
        public readonly CarId Car;
        public readonly TimerKind Kind;
        /// <summary>0 = just started, 1 = out of time.</summary>
        public readonly float Elapsed01;
        public TicketTimerChanged(TicketId ticket, CarId car, TimerKind kind, float elapsed01)
        {
            Ticket = ticket; Car = car; Kind = kind; Elapsed01 = elapsed01;
        }
    }

    /// <summary>A ticket timer ran out. Park: the car is about to vanish. Pickup: the tip will be $0.</summary>
    public readonly struct TicketTimerExpired : IGameEvent
    {
        public readonly TicketId Ticket;
        public readonly CarId Car;
        public readonly TimerKind Kind;
        public TicketTimerExpired(TicketId ticket, CarId car, TimerKind kind)
        {
            Ticket = ticket; Car = car; Kind = kind;
        }
    }

    /// <summary>A ticket timer stopped without expiring (the car was parked, or the ticket closed).</summary>
    public readonly struct TicketTimerStopped : IGameEvent
    {
        public readonly TicketId Ticket;
        public readonly CarId Car;
        public TicketTimerStopped(TicketId ticket, CarId car) { Ticket = ticket; Car = car; }
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
