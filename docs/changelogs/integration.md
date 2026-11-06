# Integration changelog

Entries are added by the integration agent, one per commit. Format in docs/changelogs/README.md.

## integration: add GameBootstrap, overlay, camera and scene builder
Date: 2026-11-05
Files: Assets/_Project/Scripts/Bootstrap/GameBootstrap.cs, CameraFollow.cs, PlaceholderSprite.cs; Assets/_Project/Scripts/UI/DebugOverlay.cs; Assets/_Project/Editor/ProjectSetup.cs, BootstrapSceneBuilder.cs, UncannyValet.Editor.asmdef
Why: P2 wiring for the design.md section 10 loop. Editor script creates LotConfig, PatronConfig and Bootstrap.unity (scene 0). Scene not yet generated: Unity batch launch was blocked in this run.
Tests: none run; code not yet compiled in Unity.

## integration: show parking spot in the overlay
Date: 2026-11-05
Files: Assets/_Project/Scripts/UI/DebugOverlay.cs, Assets/_Project/Scripts/UI/TicketBoard.cs
Why: play-test request; ticket lines show the spot from CarParked/CarLeftSpot events (Core-only). Larger font and dark background, sizes serialized. design.md section 10.
Tests: none run (Unity not launched).

## integration: fit overlay box to its text
Date: 2026-11-05
Files: Assets/_Project/Scripts/UI/DebugOverlay.cs, Assets/_Project/Scripts/UI/TicketBoard.cs
Why: the fixed-size overlay box covered the curb area. The box now sizes to the measured text (serialized padding) and shows at most 8 ticket lines then "+N more".
Tests: none run (Unity not launched).

## integration: ticket board shows car letter and live timers
Date: 2026-11-06
Files: Assets/_Project/Scripts/UI/TicketBoard.cs, TicketRow.cs, TicketRowState.cs, TicketRowFormatter.cs (+ .meta), Assets/_Project/Tests/EditMode/UI/ (TicketRowFormatterTests, TicketBoardTests, + metas), Assets/_Project/Tests/EditMode/UncannyValet.Tests.EditMode.asmdef, Assets/_Project/Scripts/Bootstrap/CameraFollow.cs, Assets/_Project/Editor/BootstrapSceneBuilder.cs, Assets/_Project/Scenes/Bootstrap.unity
Why: design.md section 10 pressure rules. Rows show the car letter (CarLetterAssigned) and Park/Pickup timer percent left, "LATE: tip $0" after pickup expiry; text formatting is a pure class. Camera size 8.5 -> 11 so the curb and queue slots 0-1 are visible together (slot 2 comes into view as the valet walks down). Tests asmdef now references UncannyValet.UI. Bootstrap checked: IEventBus and ILotLayout are registered before cars and PatronSpawner activate; no references to ArrivalInset or CanArrive outside Lot/Patrons.
Tests: added TicketRowFormatterTests and TicketBoardTests (not run; Unity not launched).
