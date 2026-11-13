using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// The Advanced panel rows of every story power's per-level numbers, grouped by power. Only the fields a power
    /// uses get a row (the other keys stay reserved); a new power field is a new entry in <see cref="FieldName"/>.
    /// </summary>
    public static class PowerLevelRows
    {
        private const float CooldownMax = 60f;
        private const float DurationMax = 20f;
        private const float DistanceMax = 30f;
        private const float PushMax = 30f;
        private const float WidthMax = 15f;

        private static readonly StoryPower[] Powers =
            { StoryPower.ThunderStride, StoryPower.StormWind, StoryPower.FreezeTime, StoryPower.MjolnirRecall };

        /// <summary>The group heading (the power's display name).</summary>
        public static string GroupOf(StoryPower power)
        {
            switch (power)
            {
                case StoryPower.ThunderStride: return "Thunder stride";
                case StoryPower.StormWind: return "Storm wind";
                case StoryPower.FreezeTime: return "Freeze time";
                default: return "Mjolnir recall";
            }
        }

        /// <summary>The plain-language name of a power's field (null when the power does not use it).</summary>
        public static string FieldName(StoryPower power, PowerField field)
        {
            if (field == PowerField.Cooldown) return "cooldown (s)";
            switch (power)
            {
                case StoryPower.ThunderStride:
                    return field == PowerField.Duration ? "dash time (s)" : field == PowerField.Distance ? "dash distance" : null;
                case StoryPower.StormWind:
                    return field == PowerField.Duration ? "gust time (s)" : field == PowerField.Distance ? "gust range"
                        : field == PowerField.PushStrength ? "push strength" : field == PowerField.Width ? "gust width" : null;
                case StoryPower.FreezeTime:
                    return field == PowerField.Duration ? "duration (s)" : null;
                default:
                    return field == PowerField.Duration ? "flight time (s)" : field == PowerField.Distance ? "recall range" : null;
            }
        }

        /// <summary>The rows of one power, level 1 then level 2, field by field.</summary>
        public static void AddRows(StoryPower power, List<TuningInfo> rows)
        {
            for (var level = 1; level <= PowerTuningKeys.Levels; level++)
                for (var f = 0; f < PowerTuningKeys.FieldCount; f++)
                {
                    var field = (PowerField)f;
                    var name = FieldName(power, field);
                    if (name == null || !PowerTuningKeys.TryGetKey(power, level, field, out var key)) continue;
                    rows.Add(new TuningInfo(key, GroupOf(power), GroupOf(power) + " level " + level + ": " + name,
                        0f, MaxOf(field), StepOf(field), "Level 1 is the power as earned; level 2 is after its upgrade."));
                }
        }

        /// <summary>The four story powers, in the doc's table order.</summary>
        public static IReadOnlyList<StoryPower> All => Powers;

        private static float MaxOf(PowerField field)
        {
            switch (field)
            {
                case PowerField.Cooldown: return CooldownMax;
                case PowerField.Duration: return DurationMax;
                case PowerField.Distance: return DistanceMax;
                case PowerField.PushStrength: return PushMax;
                default: return WidthMax;
            }
        }

        private static float StepOf(PowerField field) => field == PowerField.Duration ? 0.1f : 0.5f;
    }
}
