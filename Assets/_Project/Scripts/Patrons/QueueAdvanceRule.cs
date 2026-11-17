using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Pure rule for the queue moving up (issue 127). Slot 0 is the valet pickup spot; later slots lie behind it. A standing
    /// car drives up one slot when the slot ahead of it is free, so when the pickup car is driven away the car behind moves
    /// into the pickup spot and the rest follow one by one as each slot clears.
    /// </summary>
    public static class QueueAdvanceRule
    {
        /// <summary>The pickup spot's slot index.</summary>
        public const int PickupSlot = 0;

        /// <summary>Appends the moves for this moment to <paramref name="into"/>.</summary>
        /// <param name="standing">Per slot, the standing idle car in it, or <see cref="CarId.None"/>.</param>
        /// <param name="occupied">Per slot, true while something physically is in it or a car is already driving into it.</param>
        public static void Moves(IReadOnlyList<CarId> standing, IReadOnlyList<bool> occupied, List<QueueMove> into)
        {
            var count = standing.Count < occupied.Count ? standing.Count : occupied.Count;
            var claimed = 0UL;
            for (var slot = PickupSlot + 1; slot < count; slot++)
            {
                if (standing[slot] == CarId.None) continue;
                var ahead = slot - 1;
                if (occupied[ahead] || (claimed & (1UL << ahead)) != 0UL) continue;
                claimed |= 1UL << ahead;
                into.Add(new QueueMove(standing[slot], ahead));
            }
        }
    }
}
