using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows, default values and seeding of the late-pickup curse (a patron picked up after their timer ran out).</summary>
    public static class OvertimeCurseRows
    {
        public const string Group = "Late pickup";
        public const float SecondsPerPoint = 10f;
        public const float Cap = 3f;

        public static readonly TuningInfo[] Rows =
        {
            new TuningInfo(TuningKey.OvertimeCurseSecondsPerPoint, Group, "Late pickup: seconds of extra wait per blessing lost", 1f, 60f, 1f,
                "A patron picked up after their timer ran out costs one blessing for every this many seconds they waited past it (at least 1)."),
            new TuningInfo(TuningKey.OvertimeCurseCap, Group, "Late pickup: most blessings one pickup can cost", 0f, 10f, 1f,
                "The late-pickup curse never costs more than this. 0 switches it off."),
        };

        /// <summary>Seeds every key with its default.</summary>
        public static void Seed(ITuning t)
        {
            t.Seed(TuningKey.OvertimeCurseSecondsPerPoint, SecondsPerPoint);
            t.Seed(TuningKey.OvertimeCurseCap, Cap);
        }
    }
}
