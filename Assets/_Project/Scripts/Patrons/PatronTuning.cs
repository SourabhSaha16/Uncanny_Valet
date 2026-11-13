using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;

namespace UncannyValet.Patrons
{
    /// <summary>The live patron numbers: each read asks ITuning, and falls back to the config if a key was never seeded.</summary>
    public sealed class PatronTuning
    {
        private readonly ITuning _tuning;
        private readonly PatronConfig _config;

        public PatronTuning(ITuning tuning, PatronConfig config) { _tuning = tuning; _config = config; }

        public float PickupWindow => Read(TuningKey.PickupWindowMinutes, _config.PickupWindowMinutes);
        public float ImpatientWindowScale => Read(TuningKey.ImpatientWindowScale, _config.ImpatientWindowScale);

        /// <summary>The pickup window a patron gets: the live window, scaled down when impatient.</summary>
        public float WindowFor(bool impatient) => impatient ? PickupWindow * ImpatientWindowScale : PickupWindow;
        public float StayMin => Read(TuningKey.StayMinMinutes, _config.StayRange.x);
        public float StayMax => Read(TuningKey.StayMaxMinutes, _config.StayRange.y);
        public float GapMin => Read(TuningKey.ArrivalGapMinMinutes, _config.ArrivalIntervalRange.x);
        public float GapMax => Read(TuningKey.ArrivalGapMaxMinutes, _config.ArrivalIntervalRange.y);
        public float LateStayLead => _config.LateStayLeadMinutes;
        public float LateStayFloor => _config.LateStayFloorMinutes;

        /// <summary>Game minutes that pass per real second on the shift clock.</summary>
        public float MinutesPerSecond => Read(TuningKey.GameMinutesPerSecond, 1f);

        public float ImpatientChance => Read(TuningKey.ImpatientChance, _config.ImpatientChance);

        /// <summary>The queue wait for tickets created from now on.</summary>
        public ServiceTuning Service => new ServiceTuning(Read(TuningKey.QueueMaxWaitMinutes, _config.QueueWaitMinutes));

        /// <summary>The live clear time of a modifier in game minutes (the definition's own value until its key is seeded).</summary>
        public float ClearMinutes(ServiceModifierDefinition def) => Read(def.ClearMinutesKey, def.ClearMinutes);

        /// <summary>The live arrival chance of a modifier (0 until the lot seeds its key).</summary>
        public float Chance(ServiceModifierDefinition def) => Read(def.ChanceKey, 0f);

        private float Read(TuningKey key, float fallback) => _tuning.TryGet(key, out var v) ? v : fallback;
    }
}
