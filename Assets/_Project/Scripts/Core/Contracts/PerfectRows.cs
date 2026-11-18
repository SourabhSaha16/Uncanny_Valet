using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// Panel rows of Perfect Park and Perfect pickup judging (Lot owns the values), in designer language. The three
    /// Perfect Park judging rows show without the Advanced toggle; the rest are Advanced.
    /// </summary>
    public static class PerfectRows
    {
        public const string Group = "Perfect park and pickup";

        public static readonly TuningInfo[] Rows =
        {
            new TuningInfo(TuningKey.PerfectDropOffAngle, Group, "Perfect pickup: how straight at the curb (degrees)", 1f, 80f, 1f,
                "How far off straight the car may point at the curb and still count as perfect; 15 is fairly strict, 80 accepts any normal hand-back."),
            new TuningInfo(TuningKey.PerfectDropOffTimerShare, Group, "Perfect pickup: deliver within this share of the patron's patience", 0.05f, 1f, 0.05f,
                "The car must reach the curb before this share of the patron's waiting time has run out; 0.5 means in the first half."),
            new TuningInfo(TuningKey.PerfectParkAngle, Group, "Perfect Park: how straight (degrees off the bay lines)", 1f, 90f, 0.5f,
                "How far the car may be twisted from the bay's lines and still be perfect; 90 accepts any angle (the setting for now), 12 is forgiving, 5 is strict.",
                advanced: false),
            new TuningInfo(TuningKey.PerfectParkCooldown, Group, "Perfect Park: wait before the same car can score again (s)", 0f, 30f, 1f,
                "Seconds that must pass before the same car can earn another Perfect Park; 10 means a car can score at most every 10 seconds."),
            new TuningInfo(TuningKey.PerfectParkMinTravel, Group, "Perfect Park: distance to drive between scores", 0f, 10f, 0.5f,
                "How far a car must drive after its last score before it can score again, so nudging a parked car earns nothing; 3 is about one car length and a half."),
        };
    }
}
