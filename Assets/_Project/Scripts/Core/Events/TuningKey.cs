namespace UncannyValet.Core.Events
{
    /// <summary>Every pacing number that can be changed while the game runs (debug panel). The owner of each number reads it from ITuning.</summary>
    public enum TuningKey
    {
        // Night plan (Night module owns; edits apply to the current and later nights).
        PlanMaxWaitingCars,
        PlanArrivalGapScale,
        PlanActiveSpots,
        PlanEvChance,
        PlanDirtyChance,
        /// <summary>EV chance reached at the end of the night (ramps from PlanEvChance).</summary>
        PlanLateEvChance,
        /// <summary>Fraction 0..1 of the night after which the EV chance starts ramping.</summary>
        PlanLateEvStart,

        // Patrons.
        ParkWindowMinutes,
        PickupWindowMinutes,
        StayMinMinutes,
        StayMaxMinutes,
        ArrivalGapMinMinutes,
        ArrivalGapMaxMinutes,
        ImpatientChance,
        ChargeMinutes,
        WashMinutes,

        // Clock.
        GameMinutesPerSecond,
        NightLengthMinutes,

        // Tips.
        BaseTip,
        ChargeBonus,
        CleanBonus,
        SpeedWindowMinutes,

        // Boss orders.
        OrderGapMinMinutes,
        OrderGapMaxMinutes,
        MaxOpenOrders,
        MaxStrikes,
    }
}
