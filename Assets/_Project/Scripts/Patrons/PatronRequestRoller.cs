using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>Rolls the next arrival gap and patron request from the live tuning, the shift plan and a random source.</summary>
    public sealed class PatronRequestRoller
    {
        private readonly PatronConfig _config;
        private readonly PatronTuning _tuning;
        private readonly PatronPlan _plan;
        private readonly IPatronRandom _random;

        public PatronRequestRoller(PatronConfig config, PatronTuning tuning, PatronPlan plan, IPatronRandom random)
        {
            _config = config; _tuning = tuning; _plan = plan; _random = random;
        }

        public float NextGap() => _random.Range(_tuning.GapMin, _tuning.GapMax) * _plan.ArrivalGapScale;

        /// <summary>Rolls one request for a car arriving this far (0..1) into the shift.</summary>
        public PatronRequest Roll(float shiftProgress01)
        {
            var progress = shiftProgress01;
            var stay = _random.Range(_tuning.StayMin, _tuning.StayMax);
            var impatient = _random.Value() < _tuning.ImpatientChance;
            _plan.RollTraits(_random, progress, out var ev, out var dirty);
            return new PatronRequest(stay, _config.BaseTip, impatient, _tuning.WindowFor(impatient), ev, dirty);
        }

        /// <summary>Like <see cref="Roll(float)"/> but the car type is dictated (by the director) instead of rolled.</summary>
        public PatronRequest Roll(float shiftProgress01, CarArchetype archetype)
        {
            var stay = _random.Range(_tuning.StayMin, _tuning.StayMax);
            var impatient = _random.Value() < _tuning.ImpatientChance;
            _random.Value(); // the trait roll Roll() makes, so both paths draw the same number of values
            ArchetypeTraits.Of(archetype, out var ev, out var dirty);
            return new PatronRequest(stay, _config.BaseTip, impatient, _tuning.WindowFor(impatient), ev, dirty);
        }
    }
}
