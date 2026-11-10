using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>Pure helpers the dilemma rules share: the solvability test and station slot counts.</summary>
    public static class DilemmaSolvable
    {
        /// <summary>True with at least one free regular spot, or a pickup due within the window that will free one.</summary>
        public static bool Check(in LotSnapshot lot, DilemmaRuleSettings s)
            => lot.FreeSpots > 0 || lot.SecondsToNextPickup < s.SolvablePickupSeconds;

        /// <summary>How many slots of a station kind the lot has open (any state).</summary>
        public static int SlotsOf(in LotSnapshot lot, StationKind kind)
        {
            var n = 0;
            for (var i = 0; i < lot.Slots.Count; i++) if (lot.Slots[i].Kind == kind) n++;
            return n;
        }

        /// <summary>True when the lot has a slot of this kind and every one is in the given state.</summary>
        public static bool AllIn(in LotSnapshot lot, StationKind kind, StationSlotState state)
        {
            var total = SlotsOf(lot, kind);
            return total > 0 && lot.CountSlots(kind, state) == total;
        }
    }
}
