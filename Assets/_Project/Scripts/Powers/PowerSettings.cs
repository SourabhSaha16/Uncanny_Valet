using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Powers
{
    /// <summary>The live power numbers: start from the config, then follow the tuning store (F2 panel).</summary>
    public sealed class PowerSettings
    {
        public PowerSettings(PowerMeterConfig config)
        {
            FillPerfectPark = config.FillPerfectPark;
            FillPerfectDropOff = config.FillPerfectDropOff;
            FillDriftPerSecond = config.FillDriftPerSecond;
            FillDriftCap = config.FillDriftCap;
            Level1Threshold = config.Level1Threshold;
            Level2Threshold = config.Level2Threshold;
            Level3Threshold = config.Level3Threshold;
            SpeedBoostSeconds = config.SpeedBoostSeconds;
            WalkMultiplier = config.WalkMultiplier;
            StumbleSeconds = config.StumbleSeconds;
            TrailPerSecond = config.TrailPerSecond;
            FreezeSeconds = config.FreezeSeconds;
            FreezeTimeScale = config.FreezeTimeScale;
        }

        public float FillPerfectPark { get; private set; }
        public float FillPerfectDropOff { get; private set; }
        public float FillDriftPerSecond { get; private set; }
        public float FillDriftCap { get; private set; }
        public float Level1Threshold { get; private set; }
        public float Level2Threshold { get; private set; }

        /// <summary>Meter value that unlocks level 3; also the full bar.</summary>
        public float Level3Threshold { get; private set; }
        public float SpeedBoostSeconds { get; private set; }
        public float WalkMultiplier { get; private set; }
        public float StumbleSeconds { get; private set; }
        public float TrailPerSecond { get; private set; }
        public float FreezeSeconds { get; private set; }
        public float FreezeTimeScale { get; private set; }

        /// <summary>Takes every power key already in the store.</summary>
        public void Load(ITuning store)
        {
            foreach (var info in PowerRows.Rows)
                if (store.TryGet(info.Key, out var value)) Apply(info.Key, value);
        }

        /// <summary>Sets the number behind a key; returns false for keys that are not power keys.</summary>
        public bool Apply(TuningKey key, float value)
        {
            switch (key)
            {
                case TuningKey.PowerFillPerfectPark: FillPerfectPark = value; return true;
                case TuningKey.PowerFillPerfectDropOff: FillPerfectDropOff = value; return true;
                case TuningKey.PowerFillDriftPerSecond: FillDriftPerSecond = value; return true;
                case TuningKey.PowerFillDriftCap: FillDriftCap = value; return true;
                case TuningKey.PowerLevel1Threshold: Level1Threshold = value; return true;
                case TuningKey.PowerLevel2Threshold: Level2Threshold = value; return true;
                case TuningKey.PowerLevel3Threshold: Level3Threshold = value; return true;
                case TuningKey.PowerSpeedBoostSeconds: SpeedBoostSeconds = value; return true;
                case TuningKey.PowerWalkMultiplier: WalkMultiplier = value; return true;
                case TuningKey.PowerStumbleSeconds: StumbleSeconds = value; return true;
                case TuningKey.PowerTrailPerSecond: TrailPerSecond = value; return true;
                case TuningKey.PowerFreezeSeconds: FreezeSeconds = value; return true;
                case TuningKey.PowerFreezeTimeScale: FreezeTimeScale = value; return true;
                default: return false;
            }
        }
    }
}
