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

        // Difficulty dials (Director module maps each to many underlying keys; designers tune only these). 0..10.
        /// <summary>Dial: how much work the player is kept busy with.</summary>
        DialIntensity,
        /// <summary>Dial: how steeply intensity rises and dips over the shift (0 flat).</summary>
        DialRamp,
        /// <summary>Dial: strength of the end-of-shift surge.</summary>
        DialLateRush,
        /// <summary>Dial: share of cars that need a wash.</summary>
        DialDirtyCars,
        /// <summary>Dial: share of EVs that need a charge.</summary>
        DialEvCars,
        /// <summary>Dial: how often the director sets up squeeze moments.</summary>
        DialDilemmas,
        /// <summary>Dial: how readily the director backs off when the player is overwhelmed.</summary>
        DialMercy,
        /// <summary>Dial: how much good play raises the difficulty.</summary>
        DialSkillReward,

        // Director values only the dials derive (no dial-free meaning): shape of the target curve and base car-mix weights.
        /// <summary>Multiplier on the target workload curve (1 = as authored).</summary>
        DirTargetScale,
        /// <summary>How much of the curve's rise and dip is kept around its average (0 flat, 1 as authored).</summary>
        DirRampShape,
        /// <summary>Extra target workload at the very end of the shift, as a fraction; ramps in from PlanLateEvStart.</summary>
        DirLateTargetBoost,
        DirWeightPlain,
        DirWeightDirty,
        DirWeightEv,
        DirWeightDirtyEv,

        // Car handling (Cars module owns; applies live to every car). docs/redesign/car_handling_spec.md section 2.
        /// <summary>How fast sideways slide dies out at full grip, per second.</summary>
        HandlingLateralFriction,
        /// <summary>Share of the removed sideways speed handed back as forward speed (0 scrub, 1 keep momentum).</summary>
        HandlingForwardGrip,
        /// <summary>Speed at and below which the tyres have full grip.</summary>
        HandlingGripLowSpeed,
        /// <summary>Speed at and above which grip has dropped to HandlingGripAtSpeed.</summary>
        HandlingGripHighSpeed,
        /// <summary>Grip fraction at high speed.</summary>
        HandlingGripAtSpeed,
        /// <summary>Grip multiplier while the handbrake (Shift) is held.</summary>
        HandlingDriftGripScale,
        /// <summary>Speed at which steering reaches full effect.</summary>
        HandlingTurnFullSpeed,
        /// <summary>Tightest turning radius at low speed; sets the maximum turn rate.</summary>
        HandlingMinTurnRadius,
        /// <summary>Sideways speed above which a car counts as sliding (skid marks and tyre sound).</summary>
        HandlingSlideThreshold,
        /// <summary>Speed above which holding the drift button alone counts as sliding.</summary>
        HandlingShiftSlideSpeed,
        /// <summary>Darkest skid mark opacity (a full drift), 0..1.</summary>
        HandlingMarkOpacity,
        /// <summary>Seconds a skid mark takes to fade out.</summary>
        HandlingMarkFadeSeconds,
        /// <summary>Multiplier on every car's top speed (1 = the car's own value).</summary>
        HandlingTopSpeedScale,
        /// <summary>Multiplier on every car's acceleration.</summary>
        HandlingAccelerationScale,
        /// <summary>Multiplier on every car's braking strength.</summary>
        HandlingBrakingScale,
        /// <summary>Multiplier on every car's coasting slow-down (no throttle).</summary>
        HandlingCoastDragScale,
        /// <summary>Multiplier on every car's reverse top speed.</summary>
        HandlingReverseScale,
        /// <summary>Volume of the tyre sounds (chirp, squeal, screech), 0..1 (Audio owns).</summary>
        TyreSoundVolume,
        /// <summary>How bright the tyre sounds are, 0 = dull and round, 1 = sharp (Audio owns).</summary>
        TyreSoundSharpness,

        // Patrons (appended so earlier key numbers stay put).
        /// <summary>Multiplier on the pickup window of impatient patrons (0.3..1; 1 = they wait as long as anyone).</summary>
        ImpatientWindowScale,

        // Perfect Park and Perfect drop-off judging (Lot owns). docs/redesign/car_handling_spec.md section 4, valet_powers.md Q1.
        /// <summary>Largest angle off straight down, in degrees, for a perfect drop-off at the curb.</summary>
        PerfectDropOffAngle,
        /// <summary>A perfect drop-off must come before this share (0..1) of the pickup timer is gone.</summary>
        PerfectDropOffTimerShare,
        /// <summary>Largest angle off the bay's long axis, in degrees, for a Perfect Park.</summary>
        PerfectParkAngle,
        /// <summary>Largest side-to-side offset from the bay centre line for a Perfect Park, in units.</summary>
        PerfectParkSideOffset,
        /// <summary>Largest lengthwise offset from the bay centre for a Perfect Park, in units.</summary>
        PerfectParkLengthOffset,
        /// <summary>Seconds before the same car can earn another Perfect Park.</summary>
        PerfectParkCooldown,
        /// <summary>Units a car must have moved since it last stopped for a Perfect Park to count again.</summary>
        PerfectParkMinTravel,
        /// <summary>Dollars a Perfect Park adds to the car's ticket (before the combo multiplier).</summary>
        PerfectParkBonus,
    }
}
