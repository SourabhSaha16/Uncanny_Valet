using System;
using System.Collections.Generic;

namespace UncannyValet.Patrons
{
    /// <summary>Pure rule: which queue slot a new patron car takes, if any.</summary>
    public static class ArrivalGate
    {
        public const int NoSlot = -1;

        /// <summary>
        /// Returns the first clear slot among the first <paramref name="maxWaiting"/> slots, or
        /// <see cref="NoSlot"/> when the waiting cap is reached or every usable slot is blocked.
        /// </summary>
        public static int FirstFreeSlot(int waitingTickets, int maxWaiting, IReadOnlyList<bool> slotBlocked)
        {
            if (waitingTickets >= maxWaiting) return NoSlot;
            var usable = Math.Min(maxWaiting, slotBlocked.Count);
            for (var i = 0; i < usable; i++)
                if (!slotBlocked[i]) return i;
            return NoSlot;
        }
    }
}
