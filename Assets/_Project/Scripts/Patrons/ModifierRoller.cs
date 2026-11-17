using UncannyValet.Core.Modifiers;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Rolls the modifiers of an arriving car from the lot's chances. One random value is drawn and compared against the
    /// chances in roll order (the catalog's order), so a car holds at most one modifier and each modifier keeps exactly its
    /// own chance. Modifiers that ramp late use the plan's late chance and start. Pure given its inputs.
    /// </summary>
    public sealed class ModifierRoller
    {
        private readonly IModifierCatalog _catalog;
        private readonly PatronTuning _tuning;
        private readonly PatronPlan _plan;

        public ModifierRoller(IModifierCatalog catalog, PatronTuning tuning, PatronPlan plan)
        {
            _catalog = catalog; _tuning = tuning; _plan = plan;
        }

        /// <summary>The modifiers of a car arriving this far (0..1) into the shift.</summary>
        public ModifierSet Roll(IPatronRandom random, float shiftProgress01)
        {
            var r = random.Value();
            var threshold = 0f;
            var all = _catalog.All;
            for (var i = 0; i < all.Count; i++)
            {
                threshold += ChanceOf(all[i], shiftProgress01);
                if (r < threshold) return ModifierSet.Of(all[i].Id);
            }
            return ModifierSet.None;
        }

        private float ChanceOf(ServiceModifierDefinition def, float shiftProgress01)
        {
            var chance = _tuning.Chance(def);
            return def.RampsLate ? LateChanceCurve.Evaluate(chance, _plan.LateChance, _plan.LateStart, shiftProgress01) : chance;
        }
    }
}
