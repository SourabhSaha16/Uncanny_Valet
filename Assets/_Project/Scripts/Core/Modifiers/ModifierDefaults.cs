using UncannyValet.Core.Events;

namespace UncannyValet.Core.Modifiers
{
    /// <summary>
    /// The shipped numbers of the two modifiers (the setup tool writes them into the assets; tests build catalogs from
    /// them). Muddy is the old wash need: 15 game minutes, $15. NeedsCharge is the EV need: starts 20% charged,
    /// 20 game minutes to full, $15.
    /// </summary>
    public static class ModifierDefaults
    {
        public const float MuddyClearMinutes = 15f;
        public const float NeedsChargeClearMinutes = 20f;
        public const float NeedsChargeStart01 = 0.2f;
        public const int Bonus = 15;

        /// <summary>The diner's arrival chance of a Muddy car (the old shift plan default).</summary>
        public const float MuddyChance = 0.3f;

        /// <summary>The diner's arrival chance of a car needing a charge (the old shift plan default).</summary>
        public const float NeedsChargeChance = 0.3f;

        /// <summary>The Muddy definition values.</summary>
        public static ServiceModifierDefinition.Values Muddy() => new ServiceModifierDefinition.Values
        {
            Id = ModifierId.Muddy, DisplayName = "Muddy", ClearedAt = StationKind.Wash,
            ClearMinutes = MuddyClearMinutes, StartProgress01 = 0f, Bonus = Bonus,
            ActiveWord = "Washing", DoneWord = "Washed", Icon = ModifierIcon.Mud, Look = ModifierLook.MudOverlay,
            RampsLate = false, ClearMinutesKey = TuningKey.WashMinutes, BonusKey = TuningKey.CleanBonus,
            ChanceKey = TuningKey.ModifierChanceMuddy,
        };

        /// <summary>The NeedsCharge definition values.</summary>
        public static ServiceModifierDefinition.Values NeedsCharge() => new ServiceModifierDefinition.Values
        {
            Id = ModifierId.NeedsCharge, DisplayName = "Needs charge", ClearedAt = StationKind.Charging,
            ClearMinutes = NeedsChargeClearMinutes, StartProgress01 = NeedsChargeStart01, Bonus = Bonus,
            ActiveWord = "Charging", DoneWord = "Charged", Icon = ModifierIcon.Plug, Look = ModifierLook.ElectricBody,
            RampsLate = true, ClearMinutesKey = TuningKey.ChargeMinutes, BonusKey = TuningKey.ChargeBonus,
            ChanceKey = TuningKey.ModifierChanceNeedsCharge,
        };

        /// <summary>A catalog of both shipped modifiers, charge first (the old roll order). For tests and tools; the game reads the assets.</summary>
        public static ModifierCatalog CreateCatalog() => new ModifierCatalog(new[]
        {
            ServiceModifierDefinition.Create(NeedsCharge()), ServiceModifierDefinition.Create(Muddy()),
        });
    }
}
