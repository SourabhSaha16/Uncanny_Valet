using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Events
{
    // Park Smash events, grouped in one file by domain (the allowed exception to one type per file).

    /// <summary>Park Smash parked a waiting car (published by Lot after the car stands in its bay). Charged: an EV in a charging bay. Washed: a dirty car.</summary>
    public readonly struct ParkSmashCarParked : IGameEvent
    {
        public readonly CarId Car;
        public readonly int SpotId;
        public readonly bool Charged;
        public readonly bool Washed;
        public ParkSmashCarParked(CarId car, int spotId, bool charged, bool washed)
        {
            Car = car; SpotId = spotId; Charged = charged; Washed = washed;
        }
    }

    /// <summary>The Lot half of a Park Smash is done (published once per smash, even when nothing was parked).</summary>
    public readonly struct ParkSmashParked : IGameEvent
    {
        public readonly int Parked;
        public readonly int Charged;
        public readonly int Washed;
        public ParkSmashParked(int parked, int charged, int washed) { Parked = parked; Charged = charged; Washed = washed; }
    }

    /// <summary>Park Smash handed a waiting patron their car and paid the full tip (published by Patrons, just after TicketClosed).</summary>
    public readonly struct ParkSmashCarDelivered : IGameEvent
    {
        public readonly TicketId Ticket;
        public readonly CarId Car;
        public readonly int Tip;
        public ParkSmashCarDelivered(TicketId ticket, CarId car, int tip) { Ticket = ticket; Car = car; Tip = tip; }
    }

    /// <summary>A Park Smash finished: both halves are done. TotalTips is the sum of the tips it paid (combo included).</summary>
    public readonly struct ParkSmashCompleted : IGameEvent
    {
        public readonly int Parked;
        public readonly int Charged;
        public readonly int Washed;
        public readonly int Delivered;
        public readonly int TotalTips;
        public ParkSmashCompleted(int parked, int charged, int washed, int delivered, int totalTips)
        {
            Parked = parked; Charged = charged; Washed = washed; Delivered = delivered; TotalTips = totalTips;
        }
    }
}
