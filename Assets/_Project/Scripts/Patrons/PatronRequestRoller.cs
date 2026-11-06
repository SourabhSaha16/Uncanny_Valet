namespace UncannyValet.Patrons
{
    /// <summary>Rolls the next arrival gap and patron request from the live tuning, the night plan and a random source.</summary>
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

        /// <summary>Rolls one request for a car arriving at this minute of the night.</summary>
        public PatronRequest Roll(float minuteOfNight)
        {
            var length = _tuning.NightLength;
            var progress = length > 0f ? minuteOfNight / length : 0f;
            var stay = _random.Range(_tuning.StayMin, _tuning.StayMax);
            var impatient = _random.Value() < _tuning.ImpatientChance;
            _plan.RollTraits(_random, progress, out var ev, out var dirty);
            return new PatronRequest(stay, _config.BaseTip, impatient, _tuning.ParkWindow, _tuning.PickupWindow, ev, dirty);
        }
    }
}
