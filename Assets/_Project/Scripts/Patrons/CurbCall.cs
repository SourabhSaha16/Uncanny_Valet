using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Decides when a patron whose stay is over actually asks for the car: the patron is first sent to walk to the curb
    /// (PatronSummoned) and the pickup is requested when they report PatronReachedCurb, or after a fallback timeout
    /// when nobody walks. With a timeout of 0 the pickup is requested the moment the stay ends.
    /// </summary>
    public sealed class CurbCall
    {
        private readonly IEventBus _bus;

        public CurbCall(IEventBus bus)
        {
            _bus = bus;
        }

        /// <summary>Game minutes a summoned patron may take before the pickup is requested anyway. 0 = no walk.</summary>
        public float TimeoutMinutes { get; set; }

        /// <summary>Called every tick for a ticket whose stay ended. True when the pickup must be requested now.</summary>
        public bool ShouldRequest(Ticket t, float minute)
        {
            if (TimeoutMinutes <= 0f) return true;
            if (!t.Summoned)
            {
                t.MarkSummoned(minute);
                _bus.Publish(new PatronSummoned(t.TicketId, t.CarId));
                return false;
            }
            return minute >= t.SummonedMinute + TimeoutMinutes;
        }

        /// <summary>True when a patron reaching the curb turns into a pickup request for this ticket.</summary>
        public static bool AcceptsArrival(Ticket t) => t.Summoned && t.State == TicketState.Parked;
    }
}
