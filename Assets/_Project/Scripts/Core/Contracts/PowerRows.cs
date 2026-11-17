using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows of the power bar and the valet powers, in designer language.</summary>
    public static class PowerRows
    {
        public const string BarGroup = "Power bar";
        public const string DeveloperGroup = "Developer: owned powers";

        /// <summary>The power bar rows, then every power's rows grouped by power (Freeze time also holds its world speed row).</summary>
        public static readonly TuningInfo[] Rows = BuildRows();

        /// <summary>Test switches that give Thor a power (and its upgrade) for the session. They stand in until the sandbox power tab exists.</summary>
        public static readonly TuningInfo[] DeveloperRows = BuildDeveloperRows();

        /// <summary>The developer switch of a power (None has none).</summary>
        public static bool TryGetDeveloperKey(StoryPower power, out TuningKey key)
        {
            key = default;
            switch (power)
            {
                case StoryPower.ThunderStride: key = TuningKey.DevOwnThunderStride; return true;
                case StoryPower.StormWind: key = TuningKey.DevOwnStormWind; return true;
                case StoryPower.FreezeTime: key = TuningKey.DevOwnFreezeTime; return true;
                case StoryPower.MjolnirRecall: key = TuningKey.DevOwnMjolnirRecall; return true;
                default: return false;
            }
        }

        private static TuningInfo[] BuildRows()
        {
            var rows = new List<TuningInfo>
            {
                new TuningInfo(TuningKey.PowerFillPerfectPark, BarGroup, "Power bar: Perfect Park adds", 0f, 100f, 1f,
                    "How much of the power bar (0 to 100) one Perfect Park fills. A third of the bar is one bolt."),
                new TuningInfo(TuningKey.PowerFillPerfectDropOff, BarGroup, "Power bar: perfect drop-off adds", 0f, 100f, 1f,
                    "How much of the power bar a perfect delivery at the curb fills."),
                new TuningInfo(TuningKey.PowerFillDriftPerSecond, BarGroup, "Power bar: drifting adds per second", 0f, 5f, 0.1f,
                    "Power bar gained for every second the car slides."),
                new TuningInfo(TuningKey.PowerFillDriftCap, BarGroup, "Power bar: most one drift can add", 0f, 20f, 0.5f,
                    "One long slide stops adding power bar after this much."),
            };
            var powers = PowerLevelRows.All;
            for (var i = 0; i < powers.Count; i++)
            {
                PowerLevelRows.AddRows(powers[i], rows);
                if (powers[i] != StoryPower.FreezeTime) continue;
                rows.Add(new TuningInfo(TuningKey.PowerFreezeTimeScale, PowerLevelRows.GroupOf(powers[i]), "Freeze time: world speed (x)", 0f, 1f, 0.05f,
                    "World speed during freeze time. 0 = completely stopped, 0.3 = slow motion."));
            }
            return rows.ToArray();
        }

        private static TuningInfo[] BuildDeveloperRows()
        {
            var rows = new List<TuningInfo>();
            var powers = PowerLevelRows.All;
            for (var i = 0; i < powers.Count; i++)
            {
                TryGetDeveloperKey(powers[i], out var key);
                var name = PowerLevelRows.GroupOf(powers[i]);
                rows.Add(new TuningInfo(key, DeveloperGroup, "Developer: own " + name + " (0 = no, 1 = earned, 2 = upgraded)", 0f, 2f, 1f,
                    "Gives Thor " + name + " for testing, so the power bar and the power keys work before the worlds exist. Set it back to 0 to take it away."));
            }
            return rows.ToArray();
        }
    }
}
