# Economy and Night changelog

Entries are added by the economy-night agent, one per commit. Format in docs/changelogs/README.md.


## economy-night: add tips, debt ledger and night clock
Date: 2026-11-05
Files: Assets/_Project/Scripts/Economy/{TipConfig,RunConfig,TipCalculator,DebtLedger}.cs (+ metas), Assets/_Project/Scripts/Night/{ClockFormat,NightClock}.cs, Assets/_Project/Data/Economy/{TipConfig,RunConfig}.asset, Assets/_Project/Tests/EditMode/{Economy,Night}/*, removed the EconomyModule and NightModule markers
Why: P1 build of design.md sections 2, 5 and 9. Tuning numbers live in config assets.
Tests: EditMode tests for tip floor, impatient bonus, penalty curve, ledger night-end maths and clock format incl. midnight wrap.

## economy-night: apply Schedule A pacing and $250 debt
Date: 2026-11-06
Files: Assets/_Project/Scripts/Night/{NightPlanDefaults,NightPlanEntry}.cs, Assets/_Project/Scripts/Economy/RunConfig.cs, Assets/_Project/Data/Night/NightPlanConfig.asset, Assets/_Project/Data/Economy/RunConfig.asset, Assets/_Project/Tests/EditMode/Night/ClockTests.cs
Why: docs/balance.md sections 1 and 6 (Schedule A) and the lead's $250 starting debt decision. Per night MaxWaitingCars/ArrivalGapScale/ActiveSpots: N1 1/3.7/4, N2 3/3.2/4, N3-N7 3/(2.5,2.4,2.3,2.2,2.0)/5. NightPlanEntry gains a serialized _activeSpots (Min 0, tooltip) passed to NightPlan; the NightPlanConfig asset was edited directly with the new values. Tip values unchanged.
Tests: ClockTests.PlanDefaults_MatchDesignTable updated (gap 3.7) and PlanDefaults_MatchScheduleA added (table values, ActiveSpots non-decreasing); not run (Unity not launched).

## economy-night: 180-minute nights and end-of-night tally
Date: 2026-11-06
Files: Assets/_Project/Scripts/Economy/{NightTally,NightTallyRecorder,RunConfig}.cs, Assets/_Project/Scripts/Night/NightClock.cs, Assets/_Project/Data/Economy/RunConfig.asset, Assets/_Project/Tests/EditMode/Economy/NightTallyTests.cs, Assets/_Project/Tests/EditMode/Night/ClockTests.cs
Why: design.md section 10 (night length and summary). RunConfig.NightLengthMinutes = 180; NightClock default 180 and Initialize(bus, nightLengthMinutes) lets Bootstrap pass RunConfig's value. NightTally is the pure counter; NightTallyRecorder subscribes to the bus and publishes NightSummaryReady once per night. Bootstrap: `new NightTallyRecorder(bus, ledger, runConfig.Nights)` (ledger is the IDebtView) created before the first StartNight, `clock.Initialize(bus, runConfig.NightLengthMinutes)`, and `recorder.Dispose()` on teardown. Debt left = max(0, debt at NightStarted - tips), independent of DebtLedger order; tickets closing after NightEnded are ignored so nothing double counts.
Tests: NightTallyTests (counts, debt floor, both close orders, reset, dispose) and clock tests (180 min, ends once, restarts) added; not run (Unity not launched).

## economy-night: night plans, tip breakdown and richer summary
Date: 2026-11-06
Files: Assets/_Project/Scripts/Economy/{TipCalculator,TipConfig,RunConfig,NightTally,NightTallyRecorder}.cs, Assets/_Project/Scripts/Night/{NightClock,NightPlanConfig,NightPlanEntry,NightPlanDefaults}.cs, Assets/_Project/Data/Economy/{RunConfig,TipConfig}.asset, Assets/_Project/Tests/EditMode/{Economy,Night}/*
Why: design.md section 10 (challenge schedule). RunConfig.GameMinutesPerSecond 1.5; clock publishes NightPlanChanged before NightStarted; TipCalculator.Explain with charge/clean bonuses (forced-zero tips have all parts 0 so base+speed+charge+clean equals tips when nothing is zeroed); the summary carries the breakdown and the next night note. Bootstrap wiring: clock.Initialize(bus, run.NightLengthMinutes, run.GameMinutesPerSecond, planConfig); new NightTallyRecorder(bus, ledger, run.Nights, n => planConfig.For(n).NewTonight). NightPlanConfig has no asset yet: create one via the Create menu (fills the default table) or pass null to use built-in defaults.
Tests: tip branches, breakdown sums and note, clock speed, plan order and defaults added; not run (Unity not launched).

## economy-night: faster pacing, layered unlocks, live tuning
Date: 2026-11-06
Files: Assets/_Project/Scripts/Night/{NightClock,NightPlanSource,NightPlanDefaults,NightPlanEntry}.cs, Assets/_Project/Scripts/Economy/TipCalculator.cs, Assets/_Project/Data/Night/NightPlanConfig.asset, Assets/_Project/Tests/EditMode/Night/{ClockTests,TuningTests}.cs
Why: design.md sections 5 and 10 (user feedback round 5). Gap scale/waiting/open regular spots N1 2.0/1/4, N2 1.5/3/5, N3 1.3/3/6, N4 1.2/3/6, N5 1.1/3/7, N6 1.0/3/7, N7 0.9/3/8. Unlocks layer: N3 charging+EVs (0.35), N4 adds wash, EVs 0.3 stay, N5-N7 both. Nights 3-7 carry LateEvChance 0.75 from LateEvStart 0.6. NightClock reads GameMinutesPerSecond and NightLengthMinutes live from ITuning (Initialize tuning param, else Services in Start); NightPlanSource applies plan-key overrides to the running and later nights (a value equal to the seeded one clears the override) and the clock republishes NightPlanChanged mid-night. TipCalculator(config, tuning) reads BaseTip, ChargeBonus, CleanBonus, SpeedWindowMinutes live; without tuning it uses config/input as before.
Tests: ClockTests updated to the new table; TuningTests added (unlocks monotonic, late EV, live speed, live length, plan override republish and reset, live tips); not run (Unity not launched).

## economy-night: final strike ends the night with RunLost
Date: 2026-11-07
Files: Assets/_Project/Scripts/Night/{INightEnder,RunLossGuard,NightClock}.cs, Assets/_Project/Scripts/Economy/{NightTally,NightTallyRecorder}.cs, Assets/_Project/Tests/EditMode/Night/RunLossTests.cs
Why: design.md sections 2 and 6 (boss strikes). RunLossGuard (plain class, Bootstrap must `new RunLossGuard(bus, clock)`) asks the clock to end on a final StrikeGiven while the night runs; NightClock.EndAtNextTick ends on the next Advance so every StrikeGiven subscriber has run first and the end goes through the normal NightEnded path. NightTallyRecorder flags a final strike during an active tally and the summary is published with RunLost = true (tips so far kept, DebtLeft floored at 1 so PaidOff stays false, no next-night note).
Tests: RunLossTests (ends once with RunLost, non-final ignored, ignored when not running, no double end, new run after loss, never paid off); not run (sandbox refused the Unity batch command).

## services: one model for what a car needs (service modifiers) [Claude]
Date: 2026-11-13
Files: Assets/_Project/Scripts/Economy/{TipCalculator,TipConfig}.cs, Scripts/Night/ShiftPlanSource.cs, Data/Economy/TipConfig.asset, Tests/EditMode/Economy and Night
Why: Issue #121. The tip pays each cleared modifier's own bonus (live key when tuned) as TipBreakdown.ModifierBonus; the old charge and clean bonuses move into the modifier definitions. The shift plan no longer carries the EV and wash chances (the lot owns them).
Tests: see docs/changelogs/services.md (EditMode and PlayMode all green).

## blessings: tips become blessings, favour, curses, lot threshold [Claude]
Date: 2026-11-13
Files: see docs/changelogs/blessings.md (this module's part of the same commit).
Why: Issue #81 of epic #79: tips, combo and complaints are renamed to blessings, favour and curses; stars are replaced by the lot's blessing threshold.
Tests: see docs/changelogs/blessings.md (EditMode and PlayMode all pass).

## blessings: half a blessing when a needed service is missed [Claude]
Date: 2026-11-13
Files: Core/Events/BlessingBreakdown.cs, Core/Events/EconomyEvents.cs, Economy/BlessingCalculator.cs, Economy/ShiftBlessings.cs, Patrons/TicketSettler.cs, Tests/EditMode/Economy/ServiceMissedTests.cs, BlessingCalculatorTests.cs
Why: Issue #82 (epic #79): any open service modifier at hand-back halves the final amount once (ties up); favour untouched; ServiceMissed on BlessingBreakdown and BlessingEarned.
Tests: ServiceMissedTests added, BlessingCalculatorTests updated; EditMode 1364 passed, PlayMode 30 passed.
