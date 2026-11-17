using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// Maps (power, level, field) to its TuningKey. The 48 keys are contiguous at the end of the enum: 40 for the first five
    /// fields (power, level, field order) then 8 for <see cref="PowerField.Secondary"/> (power, level order).
    /// </summary>
    public static class PowerTuningKeys
    {
        /// <summary>Levels per power: base and upgraded.</summary>
        public const int Levels = 2;
        /// <summary>Powers with keys (Thunder stride, Storm wind, Freeze time, Mjolnir recall).</summary>
        public const int PowerCount = 4;
        /// <summary>Fields per level.</summary>
        public const int FieldCount = 6;
        /// <summary>Fields that sit in the first key block (every field before <see cref="PowerField.Secondary"/>).</summary>
        public const int BlockFieldCount = 5;
        /// <summary>Keys in all.</summary>
        public const int KeyCount = PowerCount * Levels * FieldCount;

        private const TuningKey First = TuningKey.PowerThunderStrideL1Cooldown;
        private const int FirstBlockKeys = PowerCount * Levels * BlockFieldCount;

        /// <summary>The key of a field; false for None, a level outside 1 to 2, or a field outside the enum.</summary>
        public static bool TryGetKey(StoryPower power, int level, PowerField field, out TuningKey key)
        {
            key = default;
            var power0 = (int)power - 1;
            var fieldIndex = (int)field;
            if (power0 < 0 || power0 >= PowerCount || level < 1 || level > Levels || fieldIndex < 0 || fieldIndex >= FieldCount) return false;
            var slot = power0 * Levels + (level - 1);
            var offset = fieldIndex < BlockFieldCount ? slot * BlockFieldCount + fieldIndex : FirstBlockKeys + slot;
            key = (TuningKey)((int)First + offset);
            return true;
        }

        /// <summary>The power, level and field behind a key; false for any other key.</summary>
        public static bool TryGetSlot(TuningKey key, out StoryPower power, out int level, out PowerField field)
        {
            power = StoryPower.None; level = 0; field = PowerField.Cooldown;
            var index = (int)key - (int)First;
            if (index < 0 || index >= KeyCount) return false;
            var slot = index < FirstBlockKeys ? index / BlockFieldCount : index - FirstBlockKeys;
            field = index < FirstBlockKeys ? (PowerField)(index % BlockFieldCount) : PowerField.Secondary;
            level = slot % Levels + 1;
            power = (StoryPower)(slot / Levels + 1);
            return true;
        }
    }
}
