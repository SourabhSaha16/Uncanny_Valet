using System;

namespace UncannyValet.Powers
{
    /// <summary>The power meter rule: a value from 0 to the level 3 threshold that unlocks levels 0 to 3. Spending resets it.</summary>
    public sealed class PowerMeter
    {
        private readonly PowerSettings _settings;

        public PowerMeter(PowerSettings settings) { _settings = settings; }

        /// <summary>Current meter value, 0 to the full bar.</summary>
        public float Value { get; private set; }

        /// <summary>Highest unlocked level, 0 to 3.</summary>
        public int Level => LevelAt(Value);

        /// <summary>Bar fill, 0 to 1.</summary>
        public float Fill01 => Full > 0f ? Math.Min(Value / Full, 1f) : 0f;

        private float Full => _settings.Level3Threshold;

        /// <summary>Adds meter (clamped to the full bar). Returns true when the value changed.</summary>
        public bool Add(float amount)
        {
            if (amount <= 0f || Value >= Full) return false;
            Value = Math.Min(Value + amount, Full);
            return true;
        }

        /// <summary>Empties the meter. Returns true when it was not already empty.</summary>
        public bool Reset()
        {
            if (Value <= 0f) return false;
            Value = 0f;
            return true;
        }

        private int LevelAt(float value)
        {
            if (value >= _settings.Level3Threshold) return 3;
            if (value >= _settings.Level2Threshold) return 2;
            return value >= _settings.Level1Threshold ? 1 : 0;
        }
    }
}
