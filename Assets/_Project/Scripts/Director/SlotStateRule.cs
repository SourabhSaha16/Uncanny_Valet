using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>Pure rule: the state of a station slot from the car in it (spec section 2, station states).</summary>
    public static class SlotStateRule
    {
        /// <summary>
        /// Working, Blocked or Squatted for an occupied slot. <paramref name="holdsForKind"/>: the car holds a modifier this
        /// station clears; <paramref name="openAtKind"/>: one of them is still open.
        /// </summary>
        public static StationSlotState Occupied(bool holdsForKind, bool openAtKind)
        {
            if (!holdsForKind) return StationSlotState.Squatted;
            return openAtKind ? StationSlotState.Working : StationSlotState.Blocked;
        }

        /// <summary>
        /// Free or Reserved for an empty slot: Reserved while a car that needs this station is being driven and
        /// not every such car is already matched to an earlier free slot.
        /// </summary>
        public static StationSlotState Empty(int freeSlotsBefore, int drivenCarsNeedingIt)
            => freeSlotsBefore < drivenCarsNeedingIt ? StationSlotState.Reserved : StationSlotState.Free;
    }
}
