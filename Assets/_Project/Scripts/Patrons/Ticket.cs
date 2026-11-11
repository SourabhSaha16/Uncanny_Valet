using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>One patron visit. Plain data plus its state; the registry drives the transitions.</summary>
    public sealed class Ticket
    {
        public TicketId TicketId { get; }
        public CarId CarId { get; }
        public PatronRequest Request { get; }

        /// <summary>The key-tag colour index (see TagPalette) held until the ticket ends.</summary>
        public int Tag { get; }

        /// <summary>The licence plate (for example "KT 4"), unique among open tickets; held until the ticket ends.</summary>
        public string Plate { get; }

        public TicketState State { get; private set; } = TicketState.AwaitingPark;
        public float ParkedMinute { get; private set; }
        public float PickupRequestMinute { get; private set; }
        public TicketTimer PickupTimer { get; }

        /// <summary>True while the car sits at the curb (it arrived there before pickup was requested).</summary>
        public bool AtCurb { get; set; }

        /// <summary>True once the stay ended and the patron was sent to walk to the curb (see PatronSummoned).</summary>
        public bool Summoned { get; private set; }

        /// <summary>Game minute at which the patron was sent to the curb.</summary>
        public float SummonedMinute { get; private set; }

        /// <summary>True once the pickup window ran out; the tip is then $0 but the car is still owed.</summary>
        public bool PickupExpired => PickupTimer.IsExpired;

        public float StayMinutes => Request.StayMinutes;

        public bool IsEv => Request.IsEv;
        public bool IsDirty => Request.IsDirty;
        public ChargeRule Charge { get; }
        public WashRule Wash { get; }

        /// <summary>A valet is driving the car.</summary>
        public bool Driven { get; set; }
        /// <summary>An EV is parked in a charging spot.</summary>
        public bool InChargingBay { get; set; }
        public bool InWashBay { get; set; }
        public bool ChargedEv => IsEv && Charge.IsFull;
        public bool CleanedDirty => IsDirty && Wash.IsClean;
        public bool UncleanedDirty => IsDirty && !Wash.IsClean;

        public Ticket(TicketId ticketId, CarId carId, PatronRequest request, int tag, string plate, ServiceTuning tuning)
        {
            TicketId = ticketId; CarId = carId; Request = request; Tag = tag; Plate = plate;
            PickupTimer = new TicketTimer(request.PickupWindowMinutes);
            Charge = new ChargeRule(tuning.EvStartCharge, tuning.ChargeMinutes);
            Wash = new WashRule(tuning.WashMinutes);
        }

        /// <summary>Game minutes the director has moved the end of the stay by; negative means earlier.</summary>
        public float StayShiftMinutes { get; private set; }

        public bool StayEnded(float minute)
            => State == TicketState.Parked && minute >= ParkedMinute + StayMinutes + StayShiftMinutes;

        /// <summary>
        /// Moves the end of the stay (the director's return nudge). Only a patron still inside the diner can be moved:
        /// parked, not summoned, stay not over. The new end is never before <paramref name="nowMinute"/>.
        /// Returns the delta really applied, or 0 when nothing was moved.
        /// </summary>
        public float ShiftStay(float deltaMinutes, float nowMinute)
        {
            if (State != TicketState.Parked || Summoned || StayEnded(nowMinute)) return 0f;
            var end = ParkedMinute + StayMinutes + StayShiftMinutes;
            var applied = System.Math.Max(end + deltaMinutes, nowMinute) - end;
            StayShiftMinutes += applied;
            return applied;
        }

        public void MarkParked(float minute)
        {
            if (State != TicketState.AwaitingPark) return;
            State = TicketState.Parked;
            ParkedMinute = minute;
        }

        public void MarkSummoned(float minute)
        {
            if (Summoned) return;
            Summoned = true;
            SummonedMinute = minute;
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

        /// <summary>The shift ended with this ticket open. Returns false if it was already closed.</summary>
        public bool MarkForceClosed()
        {
            if (State == TicketState.Closed) return false;
            State = TicketState.Closed;
            return true;
        }
    }
}
