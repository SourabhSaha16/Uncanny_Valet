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

        // Road traffic (Traffic module owns). A speed multiplier for ambient cars (how many are kept on screen: see the end of this list).
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

        // Valet powers (Powers module; appended so earlier key numbers stay put).
        /// <summary>Power meter fill for one Perfect Park (meter 0..100).</summary>
        PowerFillPerfectPark,
        /// <summary>Power meter fill for one perfect drop-off.</summary>
        PowerFillPerfectDropOff,
        /// <summary>Power meter fill per second of sliding.</summary>
        PowerFillDriftPerSecond,
        /// <summary>Most meter one slide can add.</summary>
        PowerFillDriftCap,
        /// <summary>Meter value that unlocks level 1 (speed boost).</summary>
        PowerLevel1Threshold,
        /// <summary>Meter value that unlocks level 2 (freeze time).</summary>
        PowerLevel2Threshold,
        /// <summary>Meter value that unlocks level 3 (Park Smash); also the full bar.</summary>
        PowerLevel3Threshold,
        /// <summary>Seconds the speed boost lasts.</summary>
        PowerSpeedBoostSeconds,
        /// <summary>Walk speed multiplier while the speed boost runs.</summary>
        PowerWalkMultiplier,
        /// <summary>Seconds freeze time lasts.</summary>
        PowerFreezeSeconds,
        /// <summary>World time scale during freeze time: 0 = stopped, 0.3 = slow.</summary>
        PowerFreezeTimeScale,
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
        // Pickup emotions (UI owns). docs/design.md A8.
        /// <summary>Share (0..1) of the pickup patience used up from which the patron is angry (red).</summary>
        EmotionAngryFrom,
        /// <summary>Share (0..1) of the pickup patience used up from which the patron will not tip any more.</summary>
        EmotionWillNotTipFrom,
        // Arrival idle guard (Director owns). Issue 70.
        /// <summary>Longest, in seconds, the arrival queue may stay empty before a car is sent whatever the pacing says.</summary>
        ArrivalIdleLimit,
        /// <summary>No car is forced in when fewer seconds than this are left in the shift.</summary>
        ArrivalIdleCutoff,
        // Patron return timing and end-of-shift pickups (Director owns). Issues 12, 13 and 73.
        /// <summary>How far, in percent of the planned stay, the director may move a patron's return either way.</summary>
        ReturnWindowPercent,
        /// <summary>Fewest seconds between two patrons reaching the curb.</summary>
        PickupSpacingSeconds,
        /// <summary>From this many seconds before the end of the shift every open ticket's return is brought forward.</summary>
        LatePickupStart,
        /// <summary>The last late-shift pickup is requested this many seconds before the end, so there is time to deliver it.</summary>
        LastPickupLead,
        // Ambient road traffic density (Traffic module owns). Issue 75.
        /// <summary>Fewest ambient cars kept on screen; below it a car is sent in at once.</summary>
        TrafficMinOnScreen,
        /// <summary>Ambient cars on screen that spawning aims for (0 = no ambient traffic).</summary>
        TrafficTargetOnScreen,
        // Speed boost feel (Powers seeds, Valet and Pedestrians read). Issue 41.
        /// <summary>Seconds a pedestrian stumbles when the boosted valet runs through them.</summary>
        PowerStumbleSeconds,
        /// <summary>Sparkles per second in the boosted valet's trail.</summary>
        PowerTrailPerSecond,
        // Patron stay start: when a queued car's clock starts (Lot and Patrons own). Issue 78.
        /// <summary>World units a resting car must be from the nearest queue slot centre for the patron's stay to start.</summary>
        QueueClearDistance,
        /// <summary>Game minutes after arrival at which a car still in the queue gets its stay started anyway.</summary>
        QueueMaxWaitMinutes,
    }
}
