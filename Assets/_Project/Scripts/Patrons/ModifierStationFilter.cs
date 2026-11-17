using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Drops the modifiers a lot cannot serve: a car only arrives needing a service when the lot has a station of the
    /// kind that clears it (no charging bay, no EV; no wash, no mud). Pure given the snapshot of the lot.
    /// </summary>
    public static class ModifierStationFilter
    {
        /// <summary>The ids of <paramref name="set"/> whose clearing station exists in <paramref name="lot"/>; unknown ids are dropped.</summary>
        public static ModifierSet Serviceable(ModifierSet set, IModifierCatalog catalog, in LotSnapshot lot)
        {
            var result = set;
            for (var i = 0; i < set.Count; i++)
            {
                var id = set.At(i);
                if (!catalog.TryGet(id, out var def) || !HasStation(lot, def.ClearedAt)) result = result.Without(id);
            }
            return result;
        }

        /// <summary>True when the lot has at least one slot of the station kind.</summary>
        public static bool HasStation(in LotSnapshot lot, StationKind kind)
        {
            for (var i = 0; i < lot.Slots.Count; i++) if (lot.Slots[i].Kind == kind) return true;
            return false;
        }
    }
}
