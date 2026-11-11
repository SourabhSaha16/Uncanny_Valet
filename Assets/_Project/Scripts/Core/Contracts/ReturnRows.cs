using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows for patron return timing and the end-of-shift pickups.</summary>
    public static class ReturnRows
    {
        public const string Group = "Returns and late shift";

        public static readonly TuningInfo[] Rows =
        {
            new TuningInfo(TuningKey.ReturnWindowPercent, Group, "Return window (+/- %)", 0f, 50f, 1f,
                "How much sooner or later than planned the director may call a parked patron back, in percent of their stay. Only before the late-shift phase."),
            new TuningInfo(TuningKey.PickupSpacingSeconds, Group, "Pickup spacing (s)", 0f, 15f, 0.5f,
                "Fewest seconds between two patrons reaching the curb, except in a peak. The late-shift pickups obey it too."),
            new TuningInfo(TuningKey.LatePickupStart, Group, "Late-shift pickups start (s before the end)", 0f, 180f, 5f,
                "From here on every parked car's patron is called back early, even outside the return window, so the player is still paid tips. 0 turns it off."),
            new TuningInfo(TuningKey.LastPickupLead, Group, "Last late pickup requested (s before the end)", 0f, 60f, 1f,
                "The late-shift pickups are spread so the last one is requested this long before the end, leaving time to deliver it."),
        };
    }
}
