using System;
using UncannyValet.Core.Events;

namespace UncannyValet.Night
{
    /// <summary>The default seven-night challenge table (design.md section 10). Pure; used to fill an empty config.</summary>
    public static class NightPlanDefaults
    {
        /// <summary>A fresh copy of the table, nights 1 to 7. Unlocks never go away once opened.</summary>
        public static NightPlan[] Create()
        {
            const string everything = "Everything at once: parallel cars, EVs and dirty cars. Move charged EVs out late in the night.";
            const float lateEv = 0.75f;
            const float lateStart = 0.6f;
            return new[]
            {
                new NightPlan(1, 1, 2.0f, 0f, 0f, false, false, "Simple: one car at a time.", 4),
                new NightPlan(2, 3, 1.5f, 0f, 0f, false, false,
                    "Things get messy: several cars waiting, parallel pickups and drop-offs.", 5),
                new NightPlan(3, 3, 1.3f, 0.35f, 0f, true, false,
                    "EV charging unlocked: blue EVs earn a bonus tip when handed back fully charged. Use the charging station; more EVs arrive late in the night.",
                    6, lateEv, lateStart),
                new NightPlan(4, 3, 1.2f, 0.3f, 0.4f, true, true,
                    "Dirty cars: wash them at the car wash (one slot) before parking. Extra tip for clean cars; unwashed cars pay $0. EVs and charging stay on.",
                    6, lateEv, lateStart),
                new NightPlan(5, 3, 1.1f, 0.3f, 0.3f, true, true, everything, 7, lateEv, lateStart),
                new NightPlan(6, 3, 1.0f, 0.3f, 0.3f, true, true, everything, 7, lateEv, lateStart),
                new NightPlan(7, 3, 0.9f, 0.3f, 0.3f, true, true, everything, 8, lateEv, lateStart),
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
