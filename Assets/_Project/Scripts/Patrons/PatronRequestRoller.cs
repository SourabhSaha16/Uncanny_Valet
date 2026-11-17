using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;

namespace UncannyValet.Patrons
{
    /// <summary>Rolls the next arrival gap and patron request from the live tuning, the shift plan and a random source.</summary>
    public sealed class PatronRequestRoller
    {
        private readonly PatronConfig _config;
        private readonly PatronTuning _tuning;
        private readonly PatronPlan _plan;
        private readonly ModifierRoller _modifiers;
        private readonly IPatronRandom _random;
        private readonly IModifierCatalog _catalog;
        private readonly ILotSnapshotSource _lot;
        private readonly DirtySpacing _spacing;

        public PatronRequestRoller(PatronConfig config, PatronTuning tuning, PatronPlan plan, IPatronRandom random,
            IModifierCatalog catalog, ILotSnapshotSource lot = null)
        {
            _config = config; _tuning = tuning; _plan = plan; _random = random; _catalog = catalog; _lot = lot;
            _spacing = new DirtySpacing(config.CleanCarsBetweenDirty);
            _modifiers = new ModifierRoller(catalog, tuning, plan);
        }

        /// <summary>Starts a new shift: no dirty car is behind the next arrival.</summary>
        public void ResetShift() => _spacing.Reset();

        public float NextGap() => _random.Range(_tuning.GapMin, _tuning.GapMax) * _plan.ArrivalGapScale;

        /// <summary>Rolls one request for a car arriving this far (0..1) into the shift.</summary>
        public PatronRequest Roll(float shiftProgress01)
        {
            var progress = shiftProgress01;
            var stay = _random.Range(_tuning.StayMin, _tuning.StayMax);
            var impatient = _random.Value() < _tuning.ImpatientChance;
            var modifiers = Limit(_modifiers.Roll(_random, progress));
            return new PatronRequest(stay, _config.BaseBlessing, impatient, _tuning.WindowFor(impatient), modifiers);
        }

        /// <summary>Like <see cref="Roll(float)"/> but the car type is dictated (by the director) instead of rolled.</summary>
        public PatronRequest Roll(float shiftProgress01, CarArchetype archetype)
        {
            var stay = _random.Range(_tuning.StayMin, _tuning.StayMax);
            var impatient = _random.Value() < _tuning.ImpatientChance;
            _random.Value(); // the modifier roll Roll() makes, so both paths draw the same number of values
            return new PatronRequest(stay, _config.BaseBlessing, impatient, _tuning.WindowFor(impatient),
                Limit(ArchetypeModifiers.Of(archetype)));
        }

        // A car only needs what the lot can serve (no charging bay, no EV), and dirty cars are kept apart.
        private ModifierSet Limit(ModifierSet set)
        {
            if (_lot != null) set = ModifierStationFilter.Serviceable(set, _catalog, _lot.Capture());
            return _spacing.Apply(set);
        }
    }
}
