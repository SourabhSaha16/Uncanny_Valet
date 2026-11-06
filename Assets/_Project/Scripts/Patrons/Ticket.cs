using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>One patron visit. Plain data plus its state; the registry drives the transitions.</summary>
    public class Ticket
    {
        public TicketId TicketId { get; }
        public CarId CarId { get; }
        public PatronRequest Request { get; }
        public TicketState State { get; private set; } = TicketState.AwaitingPark;
        public float ParkedMinute { get; private set; }
        public float PickupRequestMinute { get; private set; }

        public float StayMinutes => Request.StayMinutes;

        public Ticket(TicketId ticketId, CarId carId, PatronRequest request)
        {
            TicketId = ticketId; CarId = carId; Request = request;
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
    }
}
