using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows for how the director paces arrivals and how it counts the player's workload.</summary>
    public static class DirectorPacingRows
    {
        private const string Pacing = "Pacing";
        private const string Costs = "Workload costs";

        public static readonly TuningInfo[] PacingRows =
        {
            new TuningInfo(TuningKey.DirEnabled, Pacing, "Director on (off = old fixed arrival timer)", 0, 1, 1),
            new TuningInfo(TuningKey.DirTickSeconds, Pacing, "How often the director re-checks the lot (s)", 0.1f, 2f, 0.1f),
            new TuningInfo(TuningKey.DirTargetScale, Pacing, "Target workload (x the authored curve)", 0.2f, 2.5f, 0.05f,
                "Scales the whole target-workload curve. Higher keeps you busier."),
            new TuningInfo(TuningKey.DirRampShape, Pacing, "How much the workload curve rises and dips (0 = flat)", 0f, 2f, 0.05f,
                "0 flattens the curve to its average, 1 is as authored, above 1 exaggerates the peaks and breathers."),
            new TuningInfo(TuningKey.DirLateTargetBoost, Pacing, "Extra target workload at shift end (fraction)", -0.5f, 1f, 0.05f,
                "Added to the target, ramping in from where the EV surge starts. Negative eases the finale."),
            new TuningInfo(TuningKey.DirBand, Pacing, "Wiggle room around the target workload (+/-)", 0.02f, 0.6f, 0.01f),
            new TuningInfo(TuningKey.DirIdleWorkload, Pacing, "You count as idle below this workload", 0f, 1f, 0.05f),
            new TuningInfo(TuningKey.DirIdleSeconds, Pacing, "Idle this long, then a car is sent at once (s)", 1, 30, 1),
            new TuningInfo(TuningKey.DirUnderFactor, Pacing, "How much faster cars arrive when you are under-worked", 0.2f, 1f, 0.05f,
                "Multiplier on the wait between cars while you are below target. 0.6 means 40 percent sooner; lower is faster."),
            new TuningInfo(TuningKey.DirOverFactor, Pacing, "How much slower cars arrive when you are overwhelmed", 1f, 3f, 0.05f,
                "Multiplier on the wait between cars while you are above target. 1.5 means 50 percent later; higher is slower."),
            new TuningInfo(TuningKey.DirFactorSlew, Pacing, "How quickly the arrival pace adapts (per s)", 0.05f, 3f, 0.05f),
            new TuningInfo(TuningKey.DirBudgetBlend, Pacing, "How strongly the arrival budget pulls the pace", 0f, 1f, 0.05f,
                "0 ignores the budget; 1 spreads the remaining cars evenly over the remaining time."),
            new TuningInfo(TuningKey.DirMinInterval, Pacing, "Shortest wait between cars (s)", 1, 30, 1),
            new TuningInfo(TuningKey.DirMaxInterval, Pacing, "Longest wait between cars (s)", 10, 90, 1),
            new TuningInfo(TuningKey.DirArrivalBudget, Pacing, "Cars the shift is meant to bring", 1, 40, 1),
            new TuningInfo(TuningKey.DirBudgetSlack, Pacing, "Extra cars allowed beyond the budget", 0, 10, 1,
                "Budget plus this is a hard cap on arrivals."),
        };

        public static readonly TuningInfo[] CostRows =
        {
            new TuningInfo(TuningKey.DirHorizonSeconds, Costs, "Seconds of owed work that count as fully busy", 10, 90, 1),
            new TuningInfo(TuningKey.DirCostPark, Costs, "Work to park a car (s)", 0, 30, 0.5f),
            new TuningInfo(TuningKey.DirCostPickup, Costs, "Work to deliver a pickup (s)", 0, 30, 0.5f),
            new TuningInfo(TuningKey.DirCostWash, Costs, "Work for a wash trip, plus wash time left (s)", 0, 30, 0.5f),
            new TuningInfo(TuningKey.DirCostCharge, Costs, "Work for a charge trip (s)", 0, 30, 0.5f),
            new TuningInfo(TuningKey.DirCostEvict, Costs, "Work to move a car out of a station (s)", 0, 30, 0.5f),
            new TuningInfo(TuningKey.DirCostPickupSoon, Costs, "Work counted for a pickup that is about to land (s)", 0, 30, 0.5f),
            new TuningInfo(TuningKey.DirPickupSoonSeconds, Costs, "A pickup counts as about to land within (s)", 0, 60, 1),
        };
    }
}
