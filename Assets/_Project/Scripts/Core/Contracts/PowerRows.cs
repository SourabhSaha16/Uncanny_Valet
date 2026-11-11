using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows of the valet powers (docs/redesign/valet_powers.md), in designer language.</summary>
    public static class PowerRows
    {
        public const string Group = "Valet powers";

        public static readonly TuningInfo[] Rows =
        {
            new TuningInfo(TuningKey.PowerFillPerfectPark, Group, "Meter per Perfect Park", 0f, 100f, 1f,
                "How much of the power meter (0 to 100) one Perfect Park fills."),
            new TuningInfo(TuningKey.PowerFillPerfectDropOff, Group, "Meter per perfect drop-off", 0f, 100f, 1f,
                "How much meter a perfect delivery at the curb fills."),
            new TuningInfo(TuningKey.PowerFillDriftPerSecond, Group, "Meter per second of drifting", 0f, 5f, 0.1f,
                "Meter gained for every second the car slides."),
            new TuningInfo(TuningKey.PowerFillDriftCap, Group, "Most meter from one drift", 0f, 20f, 0.5f,
                "One long slide stops adding meter after this much."),
            new TuningInfo(TuningKey.PowerLevel1Threshold, Group, "Speed boost unlocks at", 1f, 100f, 1f,
                "Meter value that unlocks level 1, the speed boost."),
            new TuningInfo(TuningKey.PowerLevel2Threshold, Group, "Freeze time unlocks at", 1f, 150f, 1f,
                "Meter value that unlocks level 2, freeze time."),
            new TuningInfo(TuningKey.PowerLevel3Threshold, Group, "Park Smash unlocks at (full bar)", 1f, 200f, 1f,
                "Meter value that unlocks level 3, Park Smash. This is also the full bar."),
            new TuningInfo(TuningKey.PowerSpeedBoostSeconds, Group, "Speed boost lasts (s)", 1f, 20f, 0.5f,
                "How long the valet runs faster."),
            new TuningInfo(TuningKey.PowerWalkMultiplier, Group, "Speed boost walk speed (x)", 1f, 3f, 0.05f,
                "How much faster the valet walks during the boost."),
            new TuningInfo(TuningKey.PowerFreezeSeconds, Group, "Freeze time lasts (s)", 1f, 20f, 0.5f,
                "How long the world stands still."),
            new TuningInfo(TuningKey.PowerFreezeTimeScale, Group, "Freeze time speed (x)", 0f, 1f, 0.05f,
                "World speed during freeze time. 0 = completely stopped, 0.3 = slow motion."),
        };
    }
}
