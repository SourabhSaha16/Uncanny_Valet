using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;
using UncannyValet.UI;
using UncannyValet.UI.Kit;

namespace UncannyValet.Tests
{
    /// <summary>Shared helpers of the tests: modifier sets from the old flags, the shipped catalog, snapshots with modifiers.</summary>
    public static class ModifierTestKit
    {
        /// <summary>The set of a car that needs a charge and/or is Muddy.</summary>
        public static ModifierSet Of(bool needsCharge, bool muddy)
        {
            var set = ModifierSet.None;
            if (needsCharge) set = set.With(ModifierId.NeedsCharge);
            if (muddy) set = set.With(ModifierId.Muddy);
            return set;
        }

        /// <summary>A catalog of the two shipped modifiers (charge first, the diner's roll order).</summary>
        public static ModifierCatalog Catalog() => ModifierDefaults.CreateCatalog();

        /// <summary>A board row modifier for Muddy, as the tracker would describe it.</summary>
        public static RowModifier MuddyRow() => new RowModifier
        {
            Id = ModifierId.Muddy, ClearedAt = StationKind.Wash, Icon = PixelIcon.Mud, ActiveWord = "Washing", DoneWord = "Washed",
        };

        /// <summary>A board row modifier for NeedsCharge, as the tracker would describe it.</summary>
        public static RowModifier ChargeRow(float progress01 = 0.4f) => new RowModifier
        {
            Id = ModifierId.NeedsCharge, ClearedAt = StationKind.Charging, Icon = PixelIcon.Plug, ActiveWord = "Charging",
            DoneWord = "Charged", Progress01 = progress01,
        };

        /// <summary>Gives the row the modifiers of a car that is Muddy and/or needs a charge (muddy first, as the board lists them).</summary>
        public static TicketRow Give(this TicketRow row, bool needsCharge, bool muddy)
        {
            if (muddy) row.Modifiers.Add(MuddyRow());
            if (needsCharge) row.Modifiers.Add(ChargeRow());
            return row;
        }

        /// <summary>Clears one modifier of the row the way the tracker does: cleared, its done word showing.</summary>
        public static void Clear(this TicketRow row, ModifierId id)
        {
            var m = row.Find(id);
            m.IsCleared = true;
            m.ShowDone = true;
            m.Progress01 = 1f;
        }

        /// <summary>The per-station service-left array of a car holding these modifiers (nothing cleared, shipped start progress).</summary>
        public static float[] Left(ModifierSet held, ModifierSet done, float muddyProgress = 0f, float chargeProgress = ModifierDefaults.NeedsChargeStart01)
        {
            var left = new float[CarSnapshot.StationKindCount];
            if (held.Contains(ModifierId.Muddy) && !done.Contains(ModifierId.Muddy)) left[(int)StationKind.Wash] = 1f - muddyProgress;
            if (held.Contains(ModifierId.NeedsCharge) && !done.Contains(ModifierId.NeedsCharge)) left[(int)StationKind.Charging] = 1f - chargeProgress;
            return left;
        }
    }
}
