using System;
using System.Collections.Generic;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Pure rule: the first free queue slot. A slot is taken when a car physically stands in it (occupied) or when a car
    /// that is ahead on the route has it reserved. Slot 0 is nearest the curb; a taken slot also cuts off every slot
    /// behind it (<see cref="ArrivalReach"/>), so a car never stops behind an empty slot it could have used.
    /// </summary>
    public sealed class QueueSlotChooser
    {
        private readonly List<bool> _blocked = new List<bool>();

        /// <summary>
        /// Returns the lowest reachable free slot among the first <paramref name="usable"/> slots, or
        /// <see cref="ArrivalGate.NoSlot"/> when none is.
        /// </summary>
        /// <param name="occupied">Slots with a car physically in them.</param>
        /// <param name="reservedAhead">Slots reserved by cars that are ahead of the asking car.</param>
        public int Choose(IReadOnlyList<bool> occupied, IReadOnlyList<bool> reservedAhead, int usable)
        {
            _blocked.Clear();
            var count = Math.Min(usable, occupied.Count);
            for (var i = 0; i < count; i++)
                _blocked.Add(occupied[i] || (i < reservedAhead.Count && reservedAhead[i]));
            ArrivalReach.BlockBehind(_blocked);
            for (var i = 0; i < count; i++)
                if (!_blocked[i]) return i;
            return ArrivalGate.NoSlot;
        }
    }
}
