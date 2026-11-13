# Valet changelog

Entries are added by the valet agent, one per commit. Format in docs/changelogs/README.md.

## valet: add input, walking, interaction and seat control
Date: 2026-11-05
Files: Assets/_Project/Scripts/Valet/{ValetState,NearestPicker,KeyboardInputSource,OnFootMover,Interactor,SeatController}.cs, Assets/_Project/Tests/EditMode/Valet/ValetTests.cs
Why: P1 valet module (design.md sections 3, 9, 10): walk, enter and exit cars, drive via IDrivable.
Tests: ValetTests (state transitions, nearest pick) added; not run (batch-mode Unity launch was blocked in the agent sandbox); lead to run.

## valet: stop double-publishing CarExited
Date: 2026-11-05
Files: Assets/_Project/Scripts/Valet/SeatController.cs
Why: CarDriver.SetDriver(false) already publishes CarExited, so the valet no longer does (design.md section 9).
Tests: none run (Unity not run per instruction); main suite reported 52/52 before this change.

## valet: guard same-frame re-entry after exit
Date: 2026-11-05
Files: Assets/_Project/Scripts/Valet/{FrameGuard,Interactor,SeatController}.cs, Assets/_Project/Tests/EditMode/Valet/ValetTests.cs
Why: exiting re-enabled the Interactor while E was still pressed, so the valet re-entered the car the same frame (design.md section 10).
Tests: added FrameGuard_BlocksOnlyTheBlockedFrame; not run (Unity not run per instruction).

## valet: leave a car that is despawned while driven
Date: 2026-11-06
Files: Assets/_Project/Scripts/Valet/{ExitPlacement,SeatController}.cs, Assets/_Project/Scripts/Cars/CarDriver.cs (approved single-method cross-module edit), Assets/_Project/Tests/EditMode/Valet/ValetTests.cs, Assets/_Project/Tests/EditMode/Cars/CarDriverDestroyTests.cs
Why: Patrons despawns every car at night end, possibly with the valet inside; CarDriver.OnDestroy now publishes CarExited if driven, and SeatController reappears on foot beside the last position (design.md sections 9, 10).
Tests: added ExitPlacement and CarDriver destroy tests; not run (Unity not run per instruction).

## valet: make destroy-release testable
Date: 2026-11-06
Files: Assets/_Project/Scripts/Cars/CarDriver.cs, Assets/_Project/Tests/EditMode/Cars/CarDriverDestroyTests.cs
Why: OnDestroy does not run on DestroyImmediate in EditMode, so the body moved to public ReleaseDriverOnDestroy() which OnDestroy calls (design.md section 9).
Tests: tests now call ReleaseDriverOnDestroy: driven publishes once, undriven none, twice still once; not run (Unity not run per instruction).

## valet: speed boost walk, barge-through, rainbow and sparkles [Claude]
Date: 2026-11-11
Files: Core (IPowerBoost, PedestrianId, ValetBargedPedestrian in PedestrianEvents, TuningKey PowerStumbleSeconds/PowerTrailPerSecond appended, PowerRows), Valet (ValetBoost, BoostMath, BargeRule, BargeScanner, TrailEmitter, SparklePool, SparkleSprite, ValetBoostFx, OnFootMover), Pedestrians (PedestrianStumbleRule, WalkerStumbler, PedestrianWalker, WalkerSprites, PedestrianConfig + asset), Powers (SpeedBoostEffect implements IPowerBoost and is registered, PowerMeterConfig, PowerSettings, PowerActivationController), Audio (SfxPowers, SfxId.SpeedBoost, SfxRecipeBook, SfxEventRouter), Editor/ValetSceneBuilder.cs, Scenes/Bootstrap.unity (rebuilt), tests (EditMode Valet/BoostTests, Pedestrians/StumbleTests, Powers/SpeedBoostContractTests; PlayMode ValetBargeTests and asmdef reference to Valet)
Why: issue 41, power level 1 (valet_powers.md, car_handling_spec.md section 5 and row 8). On foot the valet walks at the multiplier read from IPowerBoost; while boosted its collider excludes the pedestrian layer, overlapped walkers get ValetBargedPedestrian and stumble (0.4 s, push and spin); rainbow outline, sparkle trail and a fanfare. Test it by lowering F2 "Meter per Perfect Park" and "Speed boost unlocks at".
Tests: EditMode 1231/1231 passed, PlayMode 21/21 passed (incl. barge tests and BootstrapSceneSmokeTests). Refs #41

## powers: shared power bar of three buckets, remove speed boost and Park Smash [Claude]
Date: 2026-11-13
Files: Valet: removed BargeRule, BargeScanner, BoostMath, SparklePool, SparkleSprite, TrailEmitter, ValetBoost, ValetBoostFx; OnFootMover.
Why: Speed boost deleted (issue #86 decision 9). issue #86 of epic #79 (Valet to Valhalla).
Tests: BoostTests and ValetBargeTests removed; suites pass.

## valet: Thunder stride dash on foot and car boost with lightning, crack and kick [Claude]
Date: 2026-11-13
Files: Valet: DashRule, DashDirection, BoltShape, ThunderStreaks, ThunderDash (new); OnFootMover (SetDash, EndDash), SeatController (IsDriving, Car), ValetAnimator (Facing). Editor/ValetSceneBuilder.cs adds ThunderDash to the valet; Scenes/Bootstrap.unity rebuilt. Core: PowerField.Secondary, PowerLevelValues.Secondary, PowerTuningKeys (FieldCount 6, 8 Secondary keys appended contiguously at the end of TuningKey), PowerLevelRows (Thunder stride row names), PowerEvents.ThunderStrideStarted. Data: ThunderStrideDefinition.asset. Tests as listed.
Why: issue #88 of epic #79 (Valet to Valhalla): one power, one bucket and one 4 s cooldown. On foot a 0.2 s straight dash of 5 units (8 at level 2) in the move direction else the facing, passing pedestrians (layer ignore pair for the dash) and stopping at solids; in a car a 1.5 s (2.2 s) boost of top speed x1.5 and acceleration x2 through the Cars drive modifier seam; pixel lightning streak, thunder crack and a camera kick.
Tests: EditMode 1409 passed (ThunderDashTests, ThunderStrideTests, ThunderBoostTests, ThunderSfxTests, ThunderKickLinkTests, DevPowerLinkTests key checks); PlayMode 37 passed (ThunderStridePlayModeTests: dash distance per level and pedestrian pass-through, wall stop, boosted top speed, boosted wall damage, cooldown) incl. BootstrapSceneSmokeTests; RunBatch run.
