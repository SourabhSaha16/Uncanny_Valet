using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Powers
{
    /// <summary>The live power numbers: start from the config, then follow the tuning store (F2 panel).</summary>
    public sealed class PowerSettings
    {
        private readonly PowerLevelValues[] _levels = new PowerLevelValues[PowerTuningKeys.PowerCount * PowerTuningKeys.Levels];

        public PowerSettings(PowerBarConfig config)
        {
            FillPerfectPark = config.FillPerfectPark;
            FillPerfectDropOff = config.FillPerfectDropOff;
            FillDriftPerSecond = config.FillDriftPerSecond;
            FillDriftCap = config.FillDriftCap;
            FreezeTimeScale = config.FreezeTimeScale;
            for (var p = 1; p <= PowerTuningKeys.PowerCount; p++)
                for (var level = 1; level <= PowerTuningKeys.Levels; level++)
                    _levels[IndexOf(p, level)] = config.ValuesFor((StoryPower)p, level);
        }

        public float FillPerfectPark { get; private set; }
        public float FillPerfectDropOff { get; private set; }
        public float FillDriftPerSecond { get; private set; }
        public float FillDriftCap { get; private set; }
        public float FreezeTimeScale { get; private set; }

        /// <summary>The live numbers of a power level (1 or 2); false for None or another level.</summary>
        public bool TryGetValues(StoryPower power, int level, out PowerLevelValues values)
        {
            values = default;
            if (!PowerTuningKeys.TryGetKey(power, level, PowerField.Cooldown, out _)) return false;
            values = _levels[IndexOf((int)power, level)];
            return true;
        }

        /// <summary>Takes every power key already in the store.</summary>
        public void Load(ITuning store)
        {
            foreach (var info in PowerRows.Rows)
                if (store.TryGet(info.Key, out var value)) Apply(info.Key, value);
            for (var key = 0; key < PowerTuningKeys.KeyCount; key++)
            {
                var level = (TuningKey)((int)TuningKey.PowerThunderStrideL1Cooldown + key);
                if (store.TryGet(level, out var number)) Apply(level, number);
            }
        }

        /// <summary>Sets the number behind a key; returns false for keys that are not power keys.</summary>
        public bool Apply(TuningKey key, float value)
        {
            if (PowerTuningKeys.TryGetSlot(key, out var power, out var level, out var field))
            {
                _levels[IndexOf((int)power, level)].Set(field, value);
                return true;
            }
            switch (key)
            {
                case TuningKey.PowerFillPerfectPark: FillPerfectPark = value; return true;
                case TuningKey.PowerFillPerfectDropOff: FillPerfectDropOff = value; return true;
                case TuningKey.PowerFillDriftPerSecond: FillDriftPerSecond = value; return true;
                case TuningKey.PowerFillDriftCap: FillDriftCap = value; return true;
                case TuningKey.PowerFreezeTimeScale: FreezeTimeScale = value; return true;
                default: return false;
            }
        }

        private static int IndexOf(int power, int level) => (power - 1) * PowerTuningKeys.Levels + (level - 1);
    }
}
