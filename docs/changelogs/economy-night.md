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
