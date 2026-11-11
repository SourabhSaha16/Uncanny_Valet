# Test review: what is needed, where the time goes

Date: 2026-11-10. Branch `agent/qa` (from `pivot/chaos` 9a8f5f2). Unity 6000.2.5f1, Windows, batch mode.
Measured in a fresh worktree whose `Library/` was copied from the main checkout (warm cache, but paths differ,
so scripts recompiled). A cold `Library/` costs several extra minutes and is not what the lead normally sees.

## Verdict in one paragraph

The EditMode suite (974 tests) is already cheap and almost all needed: its tests run in 6.1 s inside a 23 s
Unity launch, and 2.6 s of those 6.1 s was one badly written test (fixed). Deleting tests would save
essentially nothing. The only real test time was PlayMode (16 tests, 54 s of simulated-in-real-time physics);
running it at 4x simulated time (same fixed physics step) cut it to 13.7 s. Everything else is Unity start-up,
compile and IL2CPP, which no test change can reduce. The big wins are process changes (below).

## Measured timings (wall clock of the whole command)

| Step | Wall | Test time inside it | Overhead (start-up, import, compile, domain reload) |
| --- | --- | --- | --- |
| ProjectSetup.RunBatch (first run, scripts changed) | 30 s | 0 | 30 s |
| ProjectSetup.RunBatch (second run, nothing changed) | 13 s | 0 | 13 s |
| EditMode, before | 23 s | 6.1 s | 17 s |
| EditMode, after | 22 s | 1.9 s | 20 s (noise, about 17 to 20) |
| PlayMode, before | 72 s | 54.0 s | 18 s |
| PlayMode, after | 32 s | 13.7 s | 18 s |
| Web build (Unity step of `tools/build-web.ps1`, no site copy) | 497 s | n/a | all of it (IL2CPP + wasm link) |

Not timed: the `site/` copy and `write-build-info.sh` at the end of `build-web.ps1` (a 58 MB copy, seconds).
Unity logs: start-up `Initialize engine` to ready is about 1 s, the initial asset refresh 7.6 to 7.9 s, a domain
reload about 5 s, PlayMode adds one more forced recompile and reload of about 3 s. That is the floor of about 17 s
per launch.

Side finding: `RunBatch` rewrites `Assets/_Project/Scenes/Bootstrap.unity` every run (about 1200 changed lines
even with no source change), so it dirties the working tree on every release. Worth checking that the output is
deterministic (integration owns it).

## Per-test durations

EditMode before: 974 tests, sum of test durations 5.16 s (NUnit run duration 6.14 s). 20 slowest:

| s | Test |
| --- | --- |
| 2.632 | Patrons.PlateTests.ExhaustedPoolFallsBackToAValidPlateWithoutThrowing |
| 0.675 | Director.DirectorSoakTests.TwoHundredSeededShiftsStayWithinTheGuardrails |
| 0.408 | Audio.FunkLoopBuilderTests.BuildIsDeterministic |
| 0.069 | Integration.DifficultyDialsTests.ADialMove_WritesDerivedKeysOnceEach_WithoutFeedback |
| 0.043 | Audio.SfxTests.EveryEffectIsShortAudibleAndWithinFullScale |
| 0.038 | Director.ArchetypePickerTests.LockedStationsNeverEnterTheDraw |
| 0.032 | Director.ArchetypeBoostTests.TheMixStaysWithinTenPointsOfTheBaseShareOverABudgetShift |
| 0.029 | Traffic.CarFollowingTests.FollowerNeverReachesAStoppedLeader |
| 0.027 | Audio.CarBumpSfxTests.CarBumpRecipe_IsShortAudibleAndWithinRange |
| 0.026 | Integration.DifficultyDialsTests.MovingADialAndBack_RestoresEveryKey |
| 0.026 | Audio.TyreSoundTests.SquealLoop_IsAudibleWithinRangeAndDeterministic |
| 0.025 | Traffic.AmbientSpawnerTests.SameSeed_SameSequence |
| 0.024 | Lot.WalkwayLayoutTests.NoBandCoversABay |
| 0.023 | Economy.TipCalculatorTests.Explain_PenaltyIsNegative |
| 0.021 | Audio.FunkLoopBuilderTests.LoopPointHasNoJump |
| 0.021 | Integration.EventContractTests.CarAtCurb_BeforePickup_PaysNothing |
| 0.021 | Director.ArchetypePickerTests.StreakLimitHoldsEvenWhenDirtyAndEvDominate |
| 0.021 | Audio.FunkLoopBuilderTests.PeakIsWithinFullScaleAndLoopIsNotSilent |
| 0.020 | Audio.FunkLoopBuilderTests.LengthIsWholeBarsOfWholeBeatSamples |
| 0.017 | Audio.CarBumpSfxTests.CarsCollided_PlaysTheBumpOnceAndIsRateLimitedWithAClock |

Everything past the top three is under 0.07 s; the remaining 950 tests total about 1.3 s. After the fix the sum is
about 1.9 s (PlateTests 2.64 s -> 0.07 s).

PlayMode (all 16), before -> after (4x simulated time):

| Test | Before s | After s |
| --- | --- | --- |
| TripCarSolidTests.TripCar_StopsBehindAnObstacle_AndResumesAndArrivesWhenItIsRemoved | 11.00 | 2.75 |
| BayEntryTests.Car_FromAisleCentre_EndsInsideBay_Ev | 3.85 | 1.06 |
| BayEntryTests.Car_FromAisleCentre_EndsInsideBay_Regular(1..6), each | 3.74 | 0.94 |
| SkidMarkTests.SlideEnds_WhenTheDriverGetsOut | 3.06 | 0.77 |
| SkidMarkTests.DrivenCarThatSlides_LeavesMarksAndReportsTheSlide | 3.04 | 0.76 |
| CarBounceTests.DrivenCarHittingAParkedCar_ReboundsAndTheParkedCarSettles | 3.02 | 0.75 |
| TripCarSolidTests.DrivenCarPushedIntoATripCar_Rebounds_AndTheTripCarStaysOnItsRoute | 3.02 | 0.75 |
| CarBounceTests.CarNotLive_NeitherTriggersNorSuffersTheResponse | 3.02 | 0.76 |
| CarBounceTests.GentleTouch_IsIgnored | 1.00 | 0.25 |
| SkidMarkTests.CarWithoutADriver_NeverLeavesMarksEvenWhenSlidingFast | 0.52 | 0.13 |
| BayEntryTests.MeasuredGeometry_AisleBaysEvBayAndCarCollider | 0.02 | 0.02 |

Per-fixture PlayMode: BayEntryTests 8 tests 26.3 s -> 6.7 s; TripCarSolidTests 2 tests 14.0 -> 3.5;
CarBounceTests 3 tests 7.0 -> 1.8; SkidMarkTests 3 tests 6.6 -> 1.7. Total 54.0 -> 13.7 s.
The seven bay cases were about 26 s in total here (not 100 s). They cannot be shared into one lot build or one
looped test to save time: each case is simulated physics time (about 3.4 s of 50 Hz steps per bay), so a loop in one
test costs the same and weakens failure isolation. Only simulated speed helps, which is what was applied.

## Per-fixture review (EditMode)

All 974 tests are KEEP unless noted. Test time is negligible for every fixture except the three noted; "time" is
the fixture sum before the change. Every fixture maps to a module that still exists in `Assets/_Project/Scripts`
(no tests for removed Part B modules were found). Bay/wash/EV/pickup rules, ticket state, tips, combo and director
rules are pure logic that CLAUDE.md requires to have tests, so none can be deleted.

| Area | Fixtures (count) | Time | Verdict |
| --- | --- | --- | --- |
| Patrons.PlateTests | 23 | 2.64 s -> 0.07 s | SLOW-FIX done (one test used a constant random source, quadratic probing) |
| Director.DirectorSoakTests | 2 (200 seeded shifts + replay) | 0.69 s | KEEP: it is the guardrail for the whole Director; 0.7 s is not worth cutting |
| Audio.FunkLoopBuilderTests | 5 | 0.47 s | KEEP: 0.4 s is one deterministic synth build; could share one build (see ranked list) |
| Integration.DifficultyDialsTests | 33 | 0.20 s | KEEP: parameterised x20 cases cover each dial's documented direction |
| Audio (Sfx, TyreSound, CarBump, Synth, Urgency) | 5+14+3+6+6 | 0.2 s | KEEP; synth range checks, router rate-limit tests |
| Cars (all 25 fixtures) | about 190 | 0.2 s | KEEP; Config_*Defaults* tests mirror spec numbers (low value, near-zero cost) |
| Economy (TipCalculator 28, ShiftScore 11, ComboAndStar 11) | 50 | 0.07 s | KEEP; ShiftScore.Stars_FollowThePlaceholderThresholds and ComboAndStar.Stars_FollowTheThresholds test different layers |
| Director (other 14 fixtures) | about 125 | 0.2 s | KEEP; all rule and pacing logic |
| Integration (EventContract 10, FullShift 8, Physics 3, others) | about 75 | 0.1 s | KEEP; EventContractTests partly overlaps TicketRegistryTests (CarAtCurb_BeforePickup_*) but at the cross-module level, which is its job |
| Lot, Traffic, Pedestrians, Patrons (rest), Night, UI, Valet, Core | about 400 | 0.4 s | KEEP; no duplicates with identical assertions found, no tests of removed features |

Near-duplicates found but not removed (none saves measurable time, each guards a rule): the three
`*_DefaultsMatchTheSpec` / `Config_*Defaults` config tests (mirror the implementation but pin spec numbers);
`SquealLoop_*` quartet in TyreSoundTests (distinct properties of one buffer). Recommendation: leave.

## PlayMode fixtures

| Fixture | Count | Time before -> after | Verdict |
| --- | --- | --- | --- |
| BayEntryTests | 8 | 26.3 -> 6.7 s | KEEP, SLOW-FIX done: the 7 bays are distinct geometry (6 regular + EV) and each one is the car-handling spec section 3 check |
| TripCarSolidTests | 2 | 14.0 -> 3.5 s | KEEP, SLOW-FIX done: real-time WaitForSeconds(5+1+5) now simulated |
| CarBounceTests | 3 | 7.0 -> 1.8 s | KEEP, SLOW-FIX done |
| SkidMarkTests | 3 | 6.6 -> 1.7 s | KEEP, SLOW-FIX done. None could be EditMode: all need Rigidbody2D physics |

## What was changed (all zero-risk)

1. `Tests/EditMode/Patrons/PlateTests.cs`: `ExhaustedPoolFallsBackToAValidPlateWithoutThrowing` now fills the pool
   from `SeededPatronRandom(3)` instead of `FixedRandom(0.3f)`. A constant draw makes every one of the 3610 acquires
   probe past all held plates (about 6.5 million `FromIndex` string builds). Assertions are identical (Count equals
   Total, extra plate does not throw and is valid). 2.63 s -> 0.04 s. Tests 974 -> 974.
2. `Tests/PlayMode/FastTimeFixture.cs` (new `[SetUpFixture]`): sets `Time.timeScale = 4` for the PlayMode run and
   restores it. The physics step stays the fixed 0.02 s, so the simulation is step for step the same, and no assertion
   or tolerance was touched. 16 of 16 pass in two consecutive runs. 54.0 s -> 13.7 s of tests, 72 s -> 32 s wall.
   If a future PlayMode test depends on frame-rate (Update-driven smoothing, real-time waits), set it back to 1 in
   that fixture.

## Ranked savings

| # | Change | Saves | Risk | Status |
| --- | --- | --- | --- | --- |
| 1 | PlayMode at 4x simulated time | 40 s per PlayMode run | none seen (fixed step unchanged, 2 clean runs) | done |
| 2 | Skip the web build when nothing player-visible changed; never build per merge | 497 s per skipped build | none | process |
| 3 | One Unity launch for setup + EditMode + PlayMode (see script) | about 35 to 45 s per release | low: single launch must run setup before the tests | proposal |
| 4 | Run PlayMode only before a site deploy or on request; EditMode alone for logic, docs and asset-only changes | 32 s per merge | low: PlayMode covers 4 physics fixtures only | process |
| 5 | Skip `RunBatch` when nothing under Assets/_Project/Editor, Data or Scenes changed | 13 to 30 s | low | process |
| 6 | PlateTests slow test | 2.6 s | none | done |
| 7 | Share one synth build across FunkLoopBuilderTests | 0.35 s | none | not worth doing |
| 8 | Trim DirectorSoakTests from 200 to fewer shifts | 0.3 s | weaker guardrail | not recommended |
| 9 | Delete any EditMode test | under 0.1 s each | loses coverage | not recommended |

Not recommended at all: removing EditMode tests (they cost 1.9 s of a 22 s launch).

## Overhead that no test change can reduce

EditMode: about 17 to 20 s of 22 s is Unity launch, asset refresh (7.6 s) and domain reload (5 s). PlayMode: about
18 s of 32 s is the same plus one more forced recompile and reload to enter play mode (about 3 s). RunBatch: all
13 to 30 s. Web build: all 497 s (IL2CPP compile of 487 steps, one wasm object alone took over 150 s, then link).

## Recommended release routine (proposal; `tools/` not changed)

The three Unity launches share start-up cost. Unity allows only one batch run per project, and combining
`-executeMethod` with `-runTests` in one command was not tried, so the safe saving is to drop launches, not merge them
(a merged launch is an untested option, ranked 3 above as a proposal only). Proposed, with the Unity editor closed:

```bash
U="/c/Progra~1/Unity/Hub/Editor/6000.2.5f1/Editor/Unity.exe"

# Everyday merge check (about 22 s): logic only.
$U -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults TestResults/edit.xml -logFile TestResults/edit.log

# Only if Editor/, Data/ or Scenes/ sources changed (13 to 30 s):
$U -batchmode -nographics -projectPath . -executeMethod UncannyValet.EditorTools.ProjectSetup.RunBatch -quit -logFile TestResults/setup.log

# Only before a site deploy or on request (about 32 s):
$U -batchmode -nographics -projectPath . -runTests -testPlatform PlayMode -testResults TestResults/play.xml -logFile TestResults/play.log

# Only at a publish point (about 8 minutes):
powershell -NoProfile -File tools/build-web.ps1
```

Other ideas, none applied: keep `Library/` warm (never delete it; a new worktree needs a copy of main's `Library`,
20 s to copy 3.4 GB, versus a cold import of several minutes); keep Unity compression disabled as already set;
make `RunBatch` deterministic so the scene stops showing as modified; the web build is the dominant cost, so
batch several merges per deploy.

Expected release cost with the proposal when only code changed: EditMode 22 s + PlayMode 32 s + build 497 s =
about 9 minutes, versus about 10.5 minutes before (setup 13 to 30 s + EditMode 23 s + PlayMode 72 s + build).
