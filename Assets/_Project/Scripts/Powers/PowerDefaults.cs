using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;

namespace UncannyValet.Powers
{
    /// <summary>
    /// The shipped starting numbers of every power level: they fill a new <see cref="PowerDefinition"/> asset and stand in
    /// when a config has no definition for a power. Placeholders until the power's own task (88 to 92) tunes them.
    /// </summary>
    public static class PowerDefaults
    {
        /// <summary>The numbers of a power level (1 or 2); all zero for None or another level.</summary>
        public static PowerLevelValues Values(StoryPower power, int level)
        {
            var upgraded = level == 2;
            if (level < 1 || level > 2) return default;
            switch (power)
            {
                case StoryPower.ThunderStride:
                    return upgraded ? ThunderStrideNumbers(2.2f, 8f) : ThunderStrideNumbers(1.5f, 5f);
                case StoryPower.StormWind:
                    return upgraded ? new PowerLevelValues(8f, 2f, 10f, 8f, 4f) : new PowerLevelValues(10f, 1.5f, 8f, 6f, 3f);
                case StoryPower.FreezeTime:
                    return upgraded ? new PowerLevelValues(16f, 8f) : new PowerLevelValues(20f, 6f);
                case StoryPower.MjolnirRecall:
                    return upgraded ? new PowerLevelValues(9f, 1f, 14f) : new PowerLevelValues(12f, 1f, 10f);
                default:
                    return default;
            }
        }

        // Cooldown 4 s; dash time 0.2 s; car boost top speed x1.5 and acceleration x2 (issue 88); only the boost time and dash distance differ per level.
        private static PowerLevelValues ThunderStrideNumbers(float boostSeconds, float dashDistance) =>
            new PowerLevelValues(4f, boostSeconds, dashDistance, 1.5f, 2f, 0.2f);

        /// <summary>The display name of a power.</summary>
        public static string DisplayName(StoryPower power) => PowerLevelRows.GroupOf(power);

        /// <summary>The one-line description of a power (shown on the reward card).</summary>
        public static string Description(StoryPower power)
        {
            switch (power)
            {
                case StoryPower.ThunderStride: return "Dash forward on foot in a flash of lightning, or boost your car.";
                case StoryPower.StormWind: return "Blow a gust that clears a lane of cars and people.";
                case StoryPower.FreezeTime: return "Stop the world for a few seconds while you keep moving.";
                case StoryPower.MjolnirRecall: return "Call the hammer back to your hand.";
                default: return string.Empty;
            }
        }
    }
}
