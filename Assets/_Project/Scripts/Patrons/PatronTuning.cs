using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>The live patron numbers: each read asks ITuning, and falls back to the config if a key was never seeded.</summary>
    public sealed class PatronTuning
    {
        private readonly ITuning _tuning;
        private readonly PatronConfig _config;

        public PatronTuning(ITuning tuning, PatronConfig config) { _tuning = tuning; _config = config; }

        public float PickupWindow => Read(TuningKey.PickupWindowMinutes, _config.PickupWindowMinutes);
        public float StayMin => Read(TuningKey.StayMinMinutes, _config.StayRange.x);
        public float StayMax => Read(TuningKey.StayMaxMinutes, _config.StayRange.y);
        public float GapMin => Read(TuningKey.ArrivalGapMinMinutes, _config.ArrivalIntervalRange.x);
        public float GapMax => Read(TuningKey.ArrivalGapMaxMinutes, _config.ArrivalIntervalRange.y);
        public float ImpatientChance => Read(TuningKey.ImpatientChance, _config.ImpatientChance);

        /// <summary>Charge and wash numbers for tickets created from now on.</summary>
        public ServiceTuning Service => new ServiceTuning(_config.Tuning.EvStartCharge,
            Read(TuningKey.ChargeMinutes, _config.ChargeMinutes), Read(TuningKey.WashMinutes, _config.WashMinutes));

        private float Read(TuningKey key, float fallback) => _tuning.TryGet(key, out var v) ? v : fallback;
    }
}
