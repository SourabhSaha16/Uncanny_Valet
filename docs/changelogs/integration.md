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

## integration: round-4 wiring, tip calculation card, trait tags, camera centre
Date: 2026-11-06
Files: Assets/_Project/Scripts/Bootstrap/ (GameBootstrap, CameraFollow), Assets/_Project/Scripts/UI/ (NightSummaryCard, SummaryCardStyles, SummaryCardContent, SummaryCardFormatter, SummaryCardLayout, TicketBoard, TicketRow, TicketRowFormatter), Assets/_Project/Editor/ (ProjectSetup, BootstrapSceneBuilder), Assets/_Project/Tests/EditMode/UI/ (SummaryCardFormatterTests, TicketRowFormatterTests, TicketBoardTests)
Why: design.md section 10 (night length, challenge schedule). NightPlanConfig asset is created by ProjectSetup and passed to NightClock.Initialize (with RunConfig speed) and, via n => plan.For(n).NewTonight, to NightTallyRecorder. PatronSpawner.Random is set before the spawner activates. First night now starts one frame after Awake so StationSwitch (subscribes in Start) hears night 1's NightPlanChanged. End card gains a Tip calculation block (Base, signed Speed, Charge/Clean when any bonus is non-zero, = total, Zeroed tips when > 0) and a highlighted wrapped "Tomorrow:" block; the card grows to fit and scales down to fit the screen (font 18). Ticket rows get " EV 45%" and " DIRTY"/" clean" tags. Camera focus offset y -0.5 -> 0: view y is -8.5..8.5, queue cars reach y -8.3 and stations y 8.2, so both fit (-0.5 cut the station tops at 8.0); size unchanged.
Tests: added formatter, layout, wrap, fit and ticket-board trait tests (not run; Unity not launched).
