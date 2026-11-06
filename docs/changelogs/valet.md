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
