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

        /// <summary>Game minute the stay clock started (see <see cref="StartStay"/>); meaningless until <see cref="StayStarted"/>.</summary>
        public float StayStartMinute { get; private set; }

        /// <summary>True once the stay clock started: the car first rested clear of the queue, was parked, or waited out the queue limit.</summary>
        public bool StayStarted { get; private set; }

        /// <summary>Game minute the car arrived in the queue; meaningless until <see cref="HasArrived"/>.</summary>
        public float ArrivedMinute { get; private set; }

        /// <summary>True once the car finished driving in.</summary>
        public bool HasArrived { get; private set; }

        /// <summary>Game minute the stay ends and the patron wants the car, wherever it is; infinity while the stay has not started.</summary>
        public float ReturnMinute => StayStarted ? StayStartMinute + StayMinutes + StayShiftMinutes : float.PositiveInfinity;
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

        /// <summary>True for any open ticket that has not asked for the car yet (queued, parked, washing or charging) once the stay is over.</summary>
        public bool StayEnded(float minute) => StayStarted && IsBeforePickup && minute >= ReturnMinute;

        /// <summary>
        /// Moves the end of the stay (the director's return nudge). Only a patron still inside the diner can be moved:
        /// stay started, not yet asked for the car, not summoned, stay not over, whatever the car is doing. The new end
        /// is never before <paramref name="nowMinute"/>. Returns the delta really applied, or 0 when nothing was moved.
        /// </summary>
        public float ShiftStay(float deltaMinutes, float nowMinute)
        {
            if (!StayStarted || !IsBeforePickup || Summoned || StayEnded(nowMinute)) return 0f;
            var end = ReturnMinute;
            var applied = System.Math.Max(end + deltaMinutes, nowMinute) - end;
            StayShiftMinutes += applied;
            return applied;
        }

        /// <summary>The patron has not asked for the car yet.</summary>
        public bool IsBeforePickup => State == TicketState.AwaitingPark || State == TicketState.Parked;

        /// <summary>
        /// Starts the stay clock; only the first call counts (a car moved back into the queue does not restart it).
        /// Returns true when it started now.
        /// </summary>
        public bool StartStay(float minute)
        {
            if (StayStarted || !IsBeforePickup) return false;
            StayStarted = true;
            StayStartMinute = minute;
            return true;
        }

        /// <summary>The car finished driving in; remembered for the queue wait limit. Only the first call counts.</summary>
        public void MarkArrived(float minute)
        {
            if (HasArrived) return;
            HasArrived = true;
            ArrivedMinute = minute;
        }

        /// <summary>True when the car has sat in the queue for <paramref name="maxWaitMinutes"/> without the stay starting.</summary>
        public bool QueueWaitOver(float minute, float maxWaitMinutes)
            => !StayStarted && HasArrived && IsBeforePickup && minute >= ArrivedMinute + maxWaitMinutes;

        public void MarkParked()
        {
            if (State == TicketState.AwaitingPark) State = TicketState.Parked;
        }

        public void MarkSummoned(float minute)
        {
            if (Summoned) return;
            Summoned = true;
            SummonedMinute = minute;
        }

        public void MarkPickupRequested(float minute)
        {
            if (!IsBeforePickup) return;
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
