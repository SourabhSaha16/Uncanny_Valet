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

        public bool IsEv => Request.IsEv;
        public bool IsDirty => Request.IsDirty;
        public ChargeRule Charge { get; }
        public WashRule Wash { get; }

        /// <summary>A valet is driving the car.</summary>
        public bool Driven { get; set; }
        public bool InChargingBay { get; set; }
        public bool InWashBay { get; set; }

        public bool InStation => InChargingBay || InWashBay;
        public bool ChargedEv => IsEv && Charge.IsFull;
        public bool CleanedDirty => IsDirty && Wash.IsClean;
        public bool UncleanedDirty => IsDirty && !Wash.IsClean;

        public Ticket(TicketId ticketId, CarId carId, PatronRequest request, char letter, ServiceTuning tuning)
        {
            TicketId = ticketId; CarId = carId; Request = request; Letter = letter;
            ParkTimer = new TicketTimer(request.ParkWindowMinutes);
            PickupTimer = new TicketTimer(request.PickupWindowMinutes);
            Charge = new ChargeRule(tuning.EvStartCharge, tuning.ChargeMinutes);
            Wash = new WashRule(tuning.WashMinutes);
        }

        /// <summary>The park timer runs only while nobody drives the car and it is not stopped in a station.</summary>
        public void SyncParkTimer(float minute)
        {
            if (Driven || InStation) ParkTimer.Pause(minute);
            else ParkTimer.Resume(minute);
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

        /// <summary>The night ended with this ticket open. Returns false if it was already closed.</summary>
        public bool MarkForceClosed()
        {
            if (State == TicketState.Closed) return false;
            State = TicketState.Closed;
            return true;
        }

        /// <summary>The patron gave up waiting for a spot and took the car.</summary>
        public void MarkAbandoned()
        {
            if (State == TicketState.AwaitingPark) State = TicketState.Closed;
        }
    }
}
