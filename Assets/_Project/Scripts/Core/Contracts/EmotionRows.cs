using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows of the pickup emotions: when a waiting patron turns angry and when they stop tipping.</summary>
    public static class EmotionRows
    {
        public const string Group = "Patron mood";

        public static readonly TuningInfo[] Rows =
        {
            new TuningInfo(TuningKey.EmotionAngryFrom, Group, "Patron gets angry at (share of patience used)", 0.1f, 1f, 0.01f,
                "From here the plate and the mood bubble flash red and the CAR SLOTS row says Cust. Angry."),
            new TuningInfo(TuningKey.EmotionWillNotBlessFrom, Group, "Patron will not bless at (share of patience used)", 0.1f, 1f, 0.01f,
                "From here the patron has given up: dark red, Unblessed. At 1 this is when the patience runs out."),
        };
    }
}
