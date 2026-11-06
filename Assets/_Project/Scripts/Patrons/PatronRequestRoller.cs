namespace UncannyValet.Patrons
{
    /// <summary>Rolls the next arrival gap and patron request from the config, the night plan and a random source.</summary>
    public sealed class PatronRequestRoller
    {
        private readonly PatronConfig _config;
        private readonly PatronPlan _plan;
        private readonly IPatronRandom _random;

        public PatronRequestRoller(PatronConfig config, PatronPlan plan, IPatronRandom random)
        {
            _config = config; _plan = plan; _random = random;
        }

        public float NextGap() =>
            _random.Range(_config.ArrivalIntervalRange.x, _config.ArrivalIntervalRange.y) * _plan.ArrivalGapScale;

        public PatronRequest Roll()
        {
            var stay = _random.Range(_config.StayRange.x, _config.StayRange.y);
            var impatient = _random.Value() < _config.ImpatientChance;
            _plan.RollTraits(_random, out var ev, out var dirty);
            return new PatronRequest(stay, _config.BaseTip, impatient,
                _config.ParkWindowMinutes, _config.PickupWindowMinutes, ev, dirty);
        }
    }
}
