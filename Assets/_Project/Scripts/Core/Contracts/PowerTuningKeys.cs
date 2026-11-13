using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Maps (power, level, field) to its TuningKey. The 40 keys are contiguous in the enum and appended at its end.</summary>
    public static class PowerTuningKeys
    {
        /// <summary>Levels per power: base and upgraded.</summary>
        public const int Levels = 2;
        /// <summary>Powers with keys (Thunder stride, Storm wind, Freeze time, Mjolnir recall).</summary>
        public const int PowerCount = 4;
        /// <summary>Fields per level.</summary>
        public const int FieldCount = 5;
        /// <summary>Keys in all.</summary>
        public const int KeyCount = PowerCount * Levels * FieldCount;

        private const TuningKey First = TuningKey.PowerThunderStrideL1Cooldown;

        /// <summary>The key of a field; false for None, a level outside 1 to 2, or a field outside the enum.</summary>
        public static bool TryGetKey(StoryPower power, int level, PowerField field, out TuningKey key)
        {
            key = default;
            var power0 = (int)power - 1;
            var fieldIndex = (int)field;
            if (power0 < 0 || power0 >= PowerCount || level < 1 || level > Levels || fieldIndex < 0 || fieldIndex >= FieldCount) return false;
            key = (TuningKey)((int)First + (power0 * Levels + (level - 1)) * FieldCount + fieldIndex);
            return true;
        }

        /// <summary>The power, level and field behind a key; false for any other key.</summary>
        public static bool TryGetSlot(TuningKey key, out StoryPower power, out int level, out PowerField field)
        {
            power = StoryPower.None; level = 0; field = PowerField.Cooldown;
            var index = (int)key - (int)First;
            if (index < 0 || index >= KeyCount) return false;
            field = (PowerField)(index % FieldCount);
            level = index / FieldCount % Levels + 1;
            power = (StoryPower)(index / (FieldCount * Levels) + 1);
            return true;
        }
    }
}
