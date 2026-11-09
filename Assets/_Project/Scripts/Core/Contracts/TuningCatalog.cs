using System.Collections.Generic;
using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>What the debug panel shows: every key, grouped, with a sensible range.</summary>
    public static class TuningCatalog
    {
        public static readonly IReadOnlyList<TuningInfo> All = new[]
        {
            new TuningInfo(TuningKey.PlanMaxWaitingCars, "Shift plan", "Max cars waiting", 1, 3, 1),
            new TuningInfo(TuningKey.PlanArrivalGapScale, "Shift plan", "Arrival gap scale (lower = busier)", 0.2f, 6f, 0.05f),
            new TuningInfo(TuningKey.PlanActiveSpots, "Shift plan", "Open parking spots", 2, 12, 1),
            new TuningInfo(TuningKey.PlanEvChance, "Shift plan", "EV chance", 0f, 1f, 0.05f),
            new TuningInfo(TuningKey.PlanDirtyChance, "Shift plan", "Dirty car chance", 0f, 1f, 0.05f),
            new TuningInfo(TuningKey.PlanLateEvChance, "Shift plan", "EV chance at shift end", 0f, 1f, 0.05f),
            new TuningInfo(TuningKey.PlanLateEvStart, "Shift plan", "EV surge starts at (fraction of shift)", 0f, 1f, 0.05f),
            new TuningInfo(TuningKey.ParkWindowMinutes, "Patrons", "Park window (game min)", 5, 120, 1),
            new TuningInfo(TuningKey.PickupWindowMinutes, "Patrons", "Pickup window (game min)", 5, 120, 1),
            new TuningInfo(TuningKey.StayMinMinutes, "Patrons", "Stay min (game min)", 5, 120, 1),
            new TuningInfo(TuningKey.StayMaxMinutes, "Patrons", "Stay max (game min)", 5, 180, 1),
            new TuningInfo(TuningKey.ArrivalGapMinMinutes, "Patrons", "Arrival gap min (game min)", 1, 60, 1),
            new TuningInfo(TuningKey.ArrivalGapMaxMinutes, "Patrons", "Arrival gap max (game min)", 1, 90, 1),
            new TuningInfo(TuningKey.ImpatientChance, "Patrons", "Impatient chance", 0f, 1f, 0.05f),
            new TuningInfo(TuningKey.ChargeMinutes, "Patrons", "EV charge time (game min)", 2, 60, 1),
            new TuningInfo(TuningKey.WashMinutes, "Patrons", "Wash time (game min)", 2, 60, 1),
            new TuningInfo(TuningKey.GameMinutesPerSecond, "Clock", "Game minutes per second", 0.5f, 6f, 0.1f),
            new TuningInfo(TuningKey.ShiftLengthSeconds, "Clock", "Shift length (s, next shift)", 60, 600, 10),
            new TuningInfo(TuningKey.BaseTip, "Tips", "Base tip", 1, 200, 1),
            new TuningInfo(TuningKey.ChargeBonus, "Tips", "Charge bonus", 0, 200, 1),
            new TuningInfo(TuningKey.CleanBonus, "Tips", "Clean bonus", 0, 200, 1),
            new TuningInfo(TuningKey.SpeedWindowMinutes, "Tips", "Speed bonus window (game min)", 1, 60, 1),
        };
    }
}
