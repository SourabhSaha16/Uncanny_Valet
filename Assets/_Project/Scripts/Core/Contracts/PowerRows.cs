using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows of the power bar and the valet powers, in designer language.</summary>
    public static class PowerRows
    {
        public const string BarGroup = "Power bar";
        public const string Group = "Valet powers";
        public const string DeveloperGroup = "Developer: owned powers";

        public static readonly TuningInfo[] Rows =
        {
            new TuningInfo(TuningKey.PowerFillPerfectPark, BarGroup, "Power bar: Perfect Park adds", 0f, 100f, 1f,
                "How much of the power bar (0 to 100) one Perfect Park fills. A third of the bar is one bolt."),
            new TuningInfo(TuningKey.PowerFillPerfectDropOff, BarGroup, "Power bar: perfect drop-off adds", 0f, 100f, 1f,
                "How much of the power bar a perfect delivery at the curb fills."),
            new TuningInfo(TuningKey.PowerFillDriftPerSecond, BarGroup, "Power bar: drifting adds per second", 0f, 5f, 0.1f,
                "Power bar gained for every second the car slides."),
            new TuningInfo(TuningKey.PowerFillDriftCap, BarGroup, "Power bar: most one drift can add", 0f, 20f, 0.5f,
                "One long slide stops adding power bar after this much."),
            new TuningInfo(TuningKey.PowerFreezeSeconds, Group, "Freeze time lasts (s)", 1f, 20f, 0.5f,
                "How long the world stands still."),
            new TuningInfo(TuningKey.PowerFreezeTimeScale, Group, "Freeze time speed (x)", 0f, 1f, 0.05f,
                "World speed during freeze time. 0 = completely stopped, 0.3 = slow motion."),
        };

        /// <summary>Test switches that give Thor a power for the session. They stand in until the sandbox power tab exists.</summary>
        public static readonly TuningInfo[] DeveloperRows =
        {
            new TuningInfo(TuningKey.DevOwnFreezeTime, DeveloperGroup, "Developer: own Freeze time (0 = no, 1 = yes)", 0f, 1f, 1f,
                "Gives Thor Freeze time for testing, so the power bar and the power key work before the worlds exist. Set it back to 0 to take it away."),
        };
    }
}
