namespace UncannyValet.Core.Events
{
    /// <summary>Every pacing number that can be changed while the game runs (debug panel). The owner of each number reads it from ITuning.</summary>
    public enum TuningKey
    {
        // Shift plan (Night module owns; edits apply to the current and later shifts).
        PlanMaxWaitingCars,
        PlanArrivalGapScale,
        PlanActiveSpots,
        PlanEvChance,
        PlanDirtyChance,
        /// <summary>EV chance reached at the end of the shift (ramps from PlanEvChance).</summary>
        PlanLateEvChance,
        /// <summary>Fraction 0..1 of the shift after which the EV chance starts ramping.</summary>
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
        /// <summary>Real seconds in a shift. Read when a shift starts.</summary>
        ShiftLengthSeconds,

        // Tips.
        BaseTip,
        ChargeBonus,
        CleanBonus,
        SpeedWindowMinutes,

        // Patron Director (Director module owns). 1 = on, 0 = the old fixed arrival timer.
        DirEnabled,
        DirTickSeconds,
        /// <summary>0 = a fresh random seed every shift; otherwise the seed plus the shift number.</summary>
        DirSeed,

        // Director: workload cost table (seconds of player work).
        DirHorizonSeconds,
        DirCostPark,
        DirCostPickup,
        DirCostWash,
        DirCostCharge,
        DirCostEvict,
        DirCostPickupSoon,
        DirPickupSoonSeconds,

        // Director: pacing.
        DirBand,
        DirFormInfluence,
        DirIdleWorkload,
        DirIdleSeconds,
        DirUnderFactor,
        DirOverFactor,
        DirFactorSlew,
        DirBudgetBlend,
        DirMinInterval,
        DirMaxInterval,
        DirArrivalBudget,
        DirBudgetSlack,

        // Director: relief.
        DirReliefWorkload,
        DirReliefPressure,
        DirReliefAngryCount,
        DirReliefAngryWindow,
        DirReliefHold,
        DirReliefPlainCars,
        DirReliefCooldown,

        // Director: form.
        DirFormUpdate,
        DirFormMaxStep,
        DirFormWindow,
        DirFormWarmup,
        DirFormTipWeight,
        DirFormAngryWeight,
        DirFormComboWeight,
        DirFormAngryFull,
    }
}
