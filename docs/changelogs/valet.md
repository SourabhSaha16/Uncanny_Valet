# Valet changelog

Entries are added by the valet agent, one per commit. Format in docs/changelogs/README.md.

## valet: add input, walking, interaction and seat control
Date: 2026-11-05
Files: Assets/_Project/Scripts/Valet/{ValetState,NearestPicker,KeyboardInputSource,OnFootMover,Interactor,SeatController}.cs, Assets/_Project/Tests/EditMode/Valet/ValetTests.cs
Why: P1 valet module (design.md sections 3, 9, 10): walk, enter and exit cars, drive via IDrivable.
Tests: ValetTests (state transitions, nearest pick) added; not run (batch-mode Unity launch was blocked in the agent sandbox); lead to run.
