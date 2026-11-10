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

        // Director: dilemma rule multipliers and thresholds.
        DirDilemmaWashBlocked,
        DirDilemmaWashBusy,
        DirDilemmaBaysBlocked,
        DirDilemmaBaySquatted,
        DirDilemmaLastBay,
        DirDilemmaLotTight,
        DirDilemmaChain,
        DirLotTightSpots,
        DirSolvablePickupSeconds,

        // Director: dilemma budget.
        DirDilemmaCooldown,
        DirDilemmaBlackout,
        DirHeadroomGap,

        // Director: archetype picker.
        DirLoadUnderHeavy,
        DirLoadOverHeavy,
        DirLoadOverPlain,
        DirStreakLimit,
        DirMixTolerance,
        DirAssist,
        DirAssistDilemmaScale,

        // Pedestrians (Pedestrians module owns). How many ambient people walk around; patrons are separate.
        PedestrianCount,

        // Road traffic (Traffic module owns). Ambient cars per minute (0 = off) and a speed multiplier.
        TrafficAmbientPerMinute,
        TrafficSpeedScale,
        // Patron cars: speed of cars driving in (lane cruise, the turn into the queue scales with it) and of delivered
        // cars driving off (curb route, the lane after merging scales with it), in units per second.
        TrafficEntrySpeed,
        TrafficExitSpeed,
    }
}
