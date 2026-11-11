using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows of Perfect Park and Perfect drop-off judging (Lot owns the values).</summary>
    public static class PerfectRows
    {
        public const string Group = "Perfect park and drop-off";

        public static readonly TuningInfo[] Rows =
        {
            new TuningInfo(TuningKey.PerfectDropOffAngle, Group, "Perfect drop-off: angle (degrees)", 1f, 80f, 1f,
                "How far off straight down the car may point at the curb and still count as perfect. The normal hand-back rule is 80."),
            new TuningInfo(TuningKey.PerfectDropOffTimerShare, Group, "Perfect drop-off: patron patience used", 0.05f, 1f, 0.05f,
                "The car must arrive before this share of the patron's pickup timer is gone. 0.5 = before half."),
            new TuningInfo(TuningKey.PerfectParkAngle, Group, "Perfect park: angle (degrees)", 1f, 20f, 0.5f,
                "How far off the bay's lines the car may point, nose in or tail in."),
            new TuningInfo(TuningKey.PerfectParkSideOffset, Group, "Perfect park: side offset (units)", 0.05f, 0.8f, 0.05f,
                "How far left or right of the bay's centre line the car may rest. The bay has 0.8 of slack each side."),
            new TuningInfo(TuningKey.PerfectParkLengthOffset, Group, "Perfect park: lengthwise offset (units)", 0.1f, 1.5f, 0.05f,
                "How far forward or back of the bay's centre the car may rest."),
            new TuningInfo(TuningKey.PerfectParkCooldown, Group, "Perfect park: repeat wait (s)", 0f, 30f, 1f,
                "Seconds before the same car can earn another Perfect Park."),
            new TuningInfo(TuningKey.PerfectParkMinTravel, Group, "Perfect park: distance between parks", 0f, 10f, 0.5f,
                "How far a car must have driven since it last stopped, so nudging a parked car earns nothing."),
            new TuningInfo(TuningKey.PerfectParkBonus, Group, "Perfect park: bonus ($)", 0f, 10f, 1f,
                "Dollars added to the car's ticket, then multiplied by the combo like the rest of the tip."),
        };
    }
}
