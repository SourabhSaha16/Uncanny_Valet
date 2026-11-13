namespace UncannyValet.Core.Events
{
    /// <summary>Every pacing number that can be changed while the game runs (debug panel). The owner of each number reads it from ITuning.</summary>
    public enum TuningKey
    {
        // Shift plan (Night module owns; edits apply to the current and later shifts).
        PlanMaxWaitingCars,
        PlanArrivalGapScale,
        PlanActiveSpots,
        /// <summary>Retired: the shift plan's EV chance, now the lot's NeedsCharge chance (ModifierChanceNeedsCharge). Reserved slot, never reuse or remove.</summary>
        RetiredPlanEvChance,
        /// <summary>Retired: the shift plan's wash-need chance, now the lot's Muddy chance (ModifierChanceMuddy). Reserved slot, never reuse or remove.</summary>
        RetiredPlanWashChance,
        /// <summary>Chance of the late-ramping modifier (NeedsCharge) at the end of the shift (ramps from its lot chance).</summary>
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

        // Blessings.
        BaseBlessing,
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
        DirFormBlessingWeight,
        DirFormAngryWeight,
        DirFormFavourWeight,
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
        /// <summary>Retired: the Car Mix dial for the share of cars needing a wash (replaced by the lot's Muddy chance). Reserved slot, never reuse or remove.</summary>
        RetiredDialCarMixWash,
        /// <summary>Retired: the Car Mix dial for the share of EVs (replaced by the lot's NeedsCharge chance). Reserved slot, never reuse or remove.</summary>
        RetiredDialCarMixEv,
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
        DirWeightMuddy,
        DirWeightEv,
        DirWeightMuddyEv,

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
        /// <summary>Power bar fill for one Perfect Park (bar 0..100).</summary>
        PowerFillPerfectPark,
        /// <summary>Power bar fill for one perfect drop-off.</summary>
        PowerFillPerfectDropOff,
        /// <summary>Power bar fill per second of sliding.</summary>
        PowerFillDriftPerSecond,
        /// <summary>Most bar one slide can add.</summary>
        PowerFillDriftCap,
        /// <summary>Retired: the old meter's level 1 threshold. Reserved slot, never reuse or remove.</summary>
        RetiredPowerLevel1Threshold,
        /// <summary>Retired: the old meter's level 2 threshold. Reserved slot, never reuse or remove.</summary>
        RetiredPowerLevel2Threshold,
        /// <summary>Retired: the old meter's level 3 threshold. Reserved slot, never reuse or remove.</summary>
        RetiredPowerLevel3Threshold,
        /// <summary>Retired: seconds the removed speed boost lasted. Reserved slot, never reuse or remove.</summary>
        RetiredPowerSpeedBoostSeconds,
        /// <summary>Retired: walk multiplier of the removed speed boost. Reserved slot, never reuse or remove.</summary>
        RetiredPowerWalkMultiplier,
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
        /// <summary>Retired: the Perfect Park bonus (a perfect park no longer adds blessings, it fills the power bar). Reserved slot, never reuse or remove.</summary>
        PerfectParkBonus,
        // Pickup emotions (UI owns). docs/design.md A8.
        /// <summary>Share (0..1) of the pickup patience used up from which the patron is angry (red).</summary>
        EmotionAngryFrom,
        /// <summary>Share (0..1) of the pickup patience used up from which the patron will not bless any more.</summary>
        EmotionWillNotBlessFrom,
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
        // Retired speed boost feel keys (issue 41, removed with issue 86); the numbers stay reserved.
        /// <summary>Retired: stumble length of the removed speed boost. Reserved slot, never reuse or remove.</summary>
        RetiredPowerStumbleSeconds,
        /// <summary>Retired: sparkle trail of the removed speed boost. Reserved slot, never reuse or remove.</summary>
        RetiredPowerTrailPerSecond,
        // Patron stay start: when a queued car's clock starts (Lot and Patrons own). Issue 78.
        /// <summary>World units a resting car must be from the nearest queue slot centre for the patron's stay to start.</summary>
        QueueClearDistance,
        /// <summary>Game minutes after arrival at which a car still in the queue gets its stay started anyway.</summary>
        QueueMaxWaitMinutes,
        // Car damage (Cars owns). Issue 19.
        /// <summary>Smallest impact force (speed change, units per second) that damages a car.</summary>
        DamageMinForce,
        /// <summary>Damage percent added per unit of impact force above the smallest hit.</summary>
        DamagePercentPerForce,
        // Screen shake on a hard hit (Bootstrap owns). Issue 20.
        /// <summary>World units the camera shakes at the biggest hit.</summary>
        ShakeStrength,
        /// <summary>Seconds a shake lasts.</summary>
        ShakeSeconds,
        /// <summary>Impact force at which the shake reaches its full strength.</summary>
        ShakeFullForce,
        // Developer grant of a power through the F2 panel (Bootstrap owns). Issue 86.
        /// <summary>1 when Freeze time is granted to Thor for testing, 0 when not (the run progress is the real owner).</summary>
        DevOwnFreezeTime,
        // Service modifiers: arrival chance per modifier, seeded from the lot (Run owns the numbers). Issue 121.
        /// <summary>Chance 0..1 that a new car arrives Muddy.</summary>
        ModifierChanceMuddy,
        /// <summary>Chance 0..1 that a new car arrives needing a charge (ramps up late in the shift).</summary>
        ModifierChanceNeedsCharge,
    }
}
