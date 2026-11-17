using System;
using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Economy
{
    /// <summary>
    /// Pure rule: a patron whose pickup timer ran out is angry, and the player pays a curse when the car finally arrives.
    /// The curse is round(overtime seconds / seconds per point), at least 1 once the timer expired, never above the cap.
    /// An on-time pickup costs nothing.
    /// </summary>
    public sealed class OvertimeCurseRule
    {
        private readonly Dictionary<TicketId, float> _expiredAt = new Dictionary<TicketId, float>();

        /// <summary>The curse for a given overtime (0 when the patron was not late).</summary>
        public static int Amount(float overtimeSeconds, float secondsPerPoint, int cap)
        {
            if (overtimeSeconds <= 0f || cap <= 0) return 0;
            var points = secondsPerPoint <= 0f ? cap : (int)Math.Round(overtimeSeconds / secondsPerPoint, MidpointRounding.AwayFromZero);
            return Math.Min(Math.Max(points, 1), cap);
        }

        /// <summary>The ticket's pickup timer ran out at <paramref name="now"/> (seconds on any steady clock).</summary>
        public void MarkExpired(TicketId ticket, float now) => _expiredAt[ticket] = now;

        /// <summary>The curse owed for this ticket delivered at <paramref name="now"/>; the ticket is forgotten. 0 when on time.</summary>
        public int Take(TicketId ticket, float now, float secondsPerPoint, int cap)
        {
            if (!_expiredAt.TryGetValue(ticket, out var at)) return 0;
            _expiredAt.Remove(ticket);
            return Amount(now - at, secondsPerPoint, cap);
        }

        /// <summary>Forgets a ticket that ended without a delivery.</summary>
        public void Forget(TicketId ticket) => _expiredAt.Remove(ticket);

        /// <summary>Forgets every ticket (a new shift starts).</summary>
        public void Reset() => _expiredAt.Clear();
    }
}
