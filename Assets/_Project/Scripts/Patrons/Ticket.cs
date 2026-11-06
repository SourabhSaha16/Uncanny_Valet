using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>One patron visit. Plain data plus its state; the registry drives the transitions.</summary>
    public sealed class Ticket
    {
        public TicketId TicketId { get; }
        public CarId CarId { get; }
        public PatronRequest Request { get; }

        /// <summary>The display letter held until the ticket ends.</summary>
        public char Letter { get; }

        public TicketState State { get; private set; } = TicketState.AwaitingPark;
        public float ParkedMinute { get; private set; }
        public float PickupRequestMinute { get; private set; }
        public TicketTimer ParkTimer { get; }
        public TicketTimer PickupTimer { get; }

        /// <summary>True while the car sits at the curb (it arrived there before pickup was requested).</summary>
        public bool AtCurb { get; set; }

        /// <summary>True once the pickup window ran out; the tip is then $0 but the car is still owed.</summary>
        public bool PickupExpired => PickupTimer.IsExpired;

        public float StayMinutes => Request.StayMinutes;

        public Ticket(TicketId ticketId, CarId carId, PatronRequest request, char letter)
        {
            TicketId = ticketId; CarId = carId; Request = request; Letter = letter;
            ParkTimer = new TicketTimer(request.ParkWindowMinutes);
            PickupTimer = new TicketTimer(request.PickupWindowMinutes);
        }

        public bool StayEnded(float minute) => State == TicketState.Parked && minute >= ParkedMinute + StayMinutes;

        public void MarkParked(float minute)
        {
            if (State != TicketState.AwaitingPark) return;
            State = TicketState.Parked;
            ParkedMinute = minute;
        }

        public void MarkPickupRequested(float minute)
        {
            if (State != TicketState.Parked) return;
            State = TicketState.PickupRequested;
            PickupRequestMinute = minute;
        }

        public void MarkClosed()
        {
            if (State == TicketState.PickupRequested) State = TicketState.Closed;
        }

        /// <summary>The patron gave up waiting for a spot and took the car.</summary>
        public void MarkAbandoned()
        {
            if (State == TicketState.AwaitingPark) State = TicketState.Closed;
        }
    }
}
