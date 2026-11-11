using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>The nine top-level difficulty dials, the only rows the panel shows until Advanced is switched on.</summary>
    public static class DifficultyRows
    {
        private const string Group = TuningInfo.DifficultyGroup;
        private const float DialMax = 10f;
        private const float DialStep = 0.5f;

        public static readonly TuningInfo[] Rows =
        {
            Dial(TuningKey.DialIntensity, "Overall Intensity",
                "How much work you are kept busy with: how fast cars arrive and how many wait. Higher is harder."),
            Dial(TuningKey.DialRamp, "Ramp Over the Shift",
                "How steeply the workload climbs from shift start to end. 0 is flat, higher is a steeper climb."),
            Dial(TuningKey.DialLateRush, "Late-Shift Rush",
                "Strength of the end-of-shift surge: more EVs and a tighter squeeze of arrivals. Higher is harder."),
            Dial(TuningKey.DialDirtyCars, "Car Mix: Dirty Cars %",
                "Share of cars that need a wash. Higher means more washing."),
            Dial(TuningKey.DialEvCars, "Car Mix: EV Cars %",
                "Share of electric cars that need a charge. Higher means more charging."),
            Dial(TuningKey.DialDilemmas, "Dilemma Frequency",
                "How often the director sets up squeeze moments such as a blocked wash or squatted bays. Higher is more often."),
            Dial(TuningKey.DialMercy, "Mercy",
                "How quickly and how much the director backs off when you are overwhelmed. Higher means more relief."),
            Dial(TuningKey.DialSkillReward, "Skill Reward",
                "How much good play (tips, combo, no angry patrons) raises the difficulty. Higher pushes good players harder."),
            new TuningInfo(TuningKey.DirSeed, Group, "Randomness Seed", 0, 9999, 1,
                "0 gives a new random shift every time. Any other number replays the same shift.", false),
        };

        private static TuningInfo Dial(TuningKey key, string label, string tooltip)
            => new TuningInfo(key, Group, label, 0f, DialMax, DialStep, tooltip, false);
    }
}
