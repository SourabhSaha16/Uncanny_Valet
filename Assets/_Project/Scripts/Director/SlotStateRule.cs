using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>Pure rule: the state of a station slot from the car in it (spec section 2, station states).</summary>
    public static class SlotStateRule
    {
        /// <summary>Working, Blocked or Squatted for an occupied slot. The car's needs are passed in as plain flags.</summary>
        public static StationSlotState Occupied(StationKind kind, bool isDirty, bool washDone, bool isEv, bool chargeDone)
        {
            var needsKind = kind == StationKind.Wash ? isDirty : isEv;
            if (!needsKind) return StationSlotState.Squatted;
            var done = kind == StationKind.Wash ? washDone : chargeDone;
            return done ? StationSlotState.Blocked : StationSlotState.Working;
        }

        /// <summary>
        /// Free or Reserved for an empty slot: Reserved while a car that needs this station is being driven and
        /// not every such car is already matched to an earlier free slot.
        /// </summary>
        public static StationSlotState Empty(int freeSlotsBefore, int drivenCarsNeedingIt)
            => freeSlotsBefore < drivenCarsNeedingIt ? StationSlotState.Reserved : StationSlotState.Free;
    }
}
