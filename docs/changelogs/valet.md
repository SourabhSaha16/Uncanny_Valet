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

## valet: free Shift dash on foot with cooldown [Claude]
Date: 2026-11-16
Files: Valet: ShiftDash, DashCooldown (new); Editor/ValetSceneBuilder.cs adds ShiftDash to the valet; Scenes rebuilt by RunBatch; UI/Screens/HowToPlayText.cs (one control line); Tests: DashCooldownTests, ThunderStridePlayModeTests, HowToPlayTests.
Why: issue #145. Shift (the shared Dash/Handbrake `IInputSource.Drift`, already in Core, no contract change) dashes on foot only: 3 units in 0.2 s in the held direction else the facing, then a 1.5 s cooldown (serialized on ShiftDash, no stamina). It reuses DashRule, never ignores pedestrians (they block and bump him), stops at solids, and reads the gated input so cards, pause and the F2 text field block it.
Tests: EditMode DashCooldownTests (press edge, cooldown, reset, no dash while driving); PlayMode distance, wall stop, pedestrian blocks, cooldown.

## valet: Thunder stride is a walking speed boost on foot [Claude]
Date: 2026-11-16
Files: Valet: WalkSpeedBoost (new), ThunderDash (now trail, camera kick and boost owner), OnFootMover (SetSpeedBoost); Powers: ThunderStridePower (one duration for foot and car), PowerActivationController, DrivingTracker removed; PowerDefaults; Core/Contracts: PowerLevelRows (labels), PowerLevelValues and PowerField (comments); Data/Powers/ThunderStrideDefinition.asset (distance 0, secondary 1.6 and 2, description); Tests updated.
Why: issue #146. The level 1 power multiplies Thor's walking speed for the power's Duration (PowerLevelValues.Secondary, row "walking speed (times)", default 1.6, level 2 is 2.0; the old dash Distance field is now unused and has no row); the car boost is unchanged. The on-foot power dash and its pedestrian-layer ignore are gone.
Tests: EditMode WalkSpeedBoostTests, ThunderStrideTests, PowerFrameworkTests; PlayMode walking speed on and off with the power.

## valet: Thunder stride lasts 5 s and glows while it runs [Claude]
Date: 2026-11-16
Files: Valet: ThunderAura, ThunderAuraClock, ThunderAuraTiming, ThunderAuraLook, AuraTint, AuraGlow (new), ThunderDash (owns the glow, ShiftEnded clears it); Powers: PowerDefaults (boost 5 s level 1, 7 s level 2); Data/Powers/ThunderStrideDefinition.asset (same durations); Tests: ThunderAuraClockTests, ThunderAuraPlayModeTests (new), ThunderStrideTests (timings, no-stack test).
Why: issue #150. The boost ended too fast and was hard to see. It now lasts 5 s (7 s at level 2, still tunable in F2 Advanced) and the valet, or the car he drives, gets a pulsing golden tint (MaterialPropertyBlock), a soft glow under it and a steady spray of lightning sparks for the whole boost, blinking in the last second. The cooldown still starts when the boost ends and a fire during the boost is refused, so nothing stacks. The glow clears on PowerEnded, ShiftEnded, disable and destroy, and follows car enter and exit.
Tests: EditMode 1916 passed (clock range, warning window, clear, restart); PlayMode 83 passed (tint and glow on, off at PowerEnded and ShiftEnded, no leaked glow object); RunBatch ok (scene and asset churn reverted).
