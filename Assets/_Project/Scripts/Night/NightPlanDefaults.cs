using System;
using UncannyValet.Core.Events;

namespace UncannyValet.Night
{
    /// <summary>The default seven-night challenge table (design.md section 10). Pure; used to fill an empty config.</summary>
    public static class NightPlanDefaults
    {
        /// <summary>A fresh copy of the table, nights 1 to 7.</summary>
        public static NightPlan[] Create()
        {
            const string everything = "Everything at once: parallel cars, EVs and dirty cars.";
            return new[]
            {
                new NightPlan(1, 1, 1.0f, 0f, 0f, false, false, "Simple: one car at a time."),
                new NightPlan(2, 3, 0.7f, 0f, 0f, false, false,
                    "Things get messy: several cars waiting, parallel pickups and drop-offs."),
                new NightPlan(3, 3, 0.7f, 0.4f, 0f, true, false,
                    "EV charging unlocked: blue EVs earn a bonus tip when handed back fully charged. Use the charging station."),
                new NightPlan(4, 3, 0.7f, 0f, 0.4f, false, true,
                    "Dirty cars: wash them at the car wash (one slot) before parking. Extra tip for clean cars; unwashed cars pay $0. No EVs tonight."),
                new NightPlan(5, 3, 0.6f, 0.3f, 0.3f, true, true, everything),
                new NightPlan(6, 3, 0.6f, 0.3f, 0.3f, true, true, everything),
                new NightPlan(7, 3, 0.6f, 0.3f, 0.3f, true, true, everything),
            };
        }

        /// <summary>The default plan for a 1-based night; nights past the last reuse the last plan.</summary>
        public static NightPlan For(int night)
        {
            var table = Create();
            return table[Math.Min(Math.Max(night, 1), table.Length) - 1];
        }
    }
}
