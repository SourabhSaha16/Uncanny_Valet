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

## integration: night summary card, night flow, audio wiring, camera 8.5
Date: 2026-11-06
Files: Assets/_Project/Scripts/UI/ (NightSummaryCard, SummaryCardFormatter, SummaryCardContent, SummaryCardLayout, UI asmdef), Assets/_Project/Scripts/Bootstrap/ (GameBootstrap, NightFlow, INightStarter, IValetPlacer, IRunReset, ClockNightStarter, ValetPlacer, RunDebt, CameraFollow, Bootstrap asmdef), Assets/_Project/Editor/ (BootstrapSceneBuilder, ProjectSetup, Editor asmdef), Assets/_Project/Scenes/Bootstrap.unity (camera size only), Assets/_Project/Tests/EditMode/ (UI/SummaryCardFormatterTests, UI/NightFlowTests, UI/TicketBoardTests, Tests asmdef)
Why: design.md section 10 (night length and summary, audio). Camera back to 8.5 with a (0,-0.5) focus offset so all three queue slots show from the valet start. IMGUI summary card on NightSummaryReady with Next/New run (button, Enter, Space) publishing NextNightRequested. Bootstrap creates NightTallyRecorder, initializes the clock from RunConfig; NightFlow starts the next night or a new run; RunDebt owns the DebtLedger and swaps in a fresh one for a new run (no Economy edit). AudioDirector child plus AudioListener on the camera; ProjectSetup creates AudioConfig before the scene is built.
Tests: added SummaryCardFormatterTests, NightFlowTests and a three-car ticket board test (not run; Unity not launched).
