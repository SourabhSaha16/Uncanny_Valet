using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Panel rows that are not director numbers: the clock (always shown) and the advanced shift, patron, tip, crowd and traffic rows.</summary>
    public static class GameRows
    {
        public static readonly TuningInfo[] Clock =
        {
            new TuningInfo(TuningKey.GameMinutesPerSecond, "Clock", "Game minutes per second", 0.5f, 6f, 0.1f,
                "How fast patron timers run against real time.", false),
            new TuningInfo(TuningKey.ShiftLengthSeconds, "Clock", "Shift length (s, next shift)", 60, 600, 10,
                "Real seconds in a shift. Applies when the next shift starts.", false),
            new TuningInfo(TuningKey.PickupWindowMinutes, "Clock", "Pickup patience (game min)", 5, 120, 1,
                "Game minutes a patron waits at the curb before the tip drops to $0. Applies to waiting patrons too.", false),
            new TuningInfo(TuningKey.ImpatientWindowScale, "Clock", "How much shorter impatient patrons wait (x)", 0.3f, 1f, 0.05f,
                "Impatient patrons wait this fraction of the pickup patience (1 = same as everyone).", false),
        };

        public static readonly TuningInfo[] Rest =
        {
            new TuningInfo(TuningKey.PlanMaxWaitingCars, "Shift plan", "Most cars waiting at the curb", 1, 3, 1),
            new TuningInfo(TuningKey.PlanArrivalGapScale, "Shift plan", "Arrival gap scale (lower = busier)", 0.2f, 6f, 0.05f),
            new TuningInfo(TuningKey.PlanActiveSpots, "Shift plan", "Open parking spots", 2, 6, 1),
            new TuningInfo(TuningKey.PlanEvChance, "Shift plan", "Chance a car is an EV", 0f, 1f, 0.05f),
            new TuningInfo(TuningKey.PlanDirtyChance, "Shift plan", "Chance a car is dirty", 0f, 1f, 0.05f),
            new TuningInfo(TuningKey.PlanLateEvChance, "Shift plan", "Chance a car is an EV at shift end", 0f, 1f, 0.05f),
            new TuningInfo(TuningKey.PlanLateEvStart, "Shift plan", "EV surge starts at (fraction of shift)", 0f, 1f, 0.05f),
            new TuningInfo(TuningKey.StayMinMinutes, "Patrons", "Shortest stay (game min)", 5, 120, 1),
            new TuningInfo(TuningKey.StayMaxMinutes, "Patrons", "Longest stay (game min)", 5, 180, 1),
            new TuningInfo(TuningKey.ArrivalGapMinMinutes, "Patrons", "Old timer: shortest gap (game min)", 1, 60, 1),
            new TuningInfo(TuningKey.ArrivalGapMaxMinutes, "Patrons", "Old timer: longest gap (game min)", 1, 90, 1),
            new TuningInfo(TuningKey.ImpatientChance, "Patrons", "Chance a patron is impatient", 0f, 1f, 0.05f),
            new TuningInfo(TuningKey.ChargeMinutes, "Patrons", "EV charge time (game min)", 2, 60, 1),
            new TuningInfo(TuningKey.WashMinutes, "Patrons", "Wash time (game min)", 2, 60, 1),
            new TuningInfo(TuningKey.BaseTip, "Tips", "Base tip", 1, 200, 1),
            new TuningInfo(TuningKey.ChargeBonus, "Tips", "Charge bonus", 0, 200, 1),
            new TuningInfo(TuningKey.CleanBonus, "Tips", "Clean bonus", 0, 200, 1),
            new TuningInfo(TuningKey.SpeedWindowMinutes, "Tips", "Speed bonus window (game min)", 1, 60, 1),
            new TuningInfo(TuningKey.PedestrianCount, "Pedestrians", "Ambient pedestrians (walking in/out)", 0, 10, 1),
            new TuningInfo(TuningKey.TrafficAmbientPerMinute, "Traffic", "Ambient cars per minute (0 = off)", 0, 40, 1),
            new TuningInfo(TuningKey.TrafficSpeedScale, "Traffic", "Ambient car speed scale", 0.2f, 2f, 0.05f),
            new TuningInfo(TuningKey.TrafficEntrySpeed, "Traffic", "Entry car speed (units/s)", 0.5f, 12f, 0.1f),
            new TuningInfo(TuningKey.TrafficExitSpeed, "Traffic", "Exit car speed (units/s)", 0.5f, 12f, 0.1f),
        };
    }
}
