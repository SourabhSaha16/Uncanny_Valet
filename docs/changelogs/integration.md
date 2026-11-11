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

## integration: tuning store seeding and F2 debug panel
Date: 2026-11-06
Files: Assets/_Project/Scripts/Bootstrap/ (GameBootstrap, CameraFollow, TuningSeeder, LotBounds, GatedInputSource), Assets/_Project/Scripts/UI/ (TuningPanel, TuningRowView, DebugOverlay), Assets/_Project/Editor/BootstrapSceneBuilder.cs, Assets/_Project/Tests/EditMode/Integration/GatedInputSourceTests.cs
Why: user feedback round 5. GameBootstrap creates a TuningStore, registers ITuning before any child is activated and seeds every TuningKey from RunConfig, PatronConfig, TipConfig and night 1's plan (ActiveSpots 0 is reseeded with the lot's spot count). Tuned values persist across runs; Reset all returns to seeds. F2 toggles an IMGUI panel (grouped sliders, number fields, per-row reset, Reset all) that never pauses the game and writes only via ITuning.Set; the valet input is wrapped in GatedInputSource so keys are ignored only while a number field has focus. Camera now measures the built lot's renderer bounds, zooms to fit (cap 16, min 8.5) and clamps the view inside the lot, so a lot up to ~1.6x wider stays framed. The scene builder adds the TuningPanel child and the PatronConfig reference; RunBatch is unchanged.
Tests: added GatedInputSourceTests (not run; Unity not launched). Panel and camera need a play check.

## integration: pickup banner shows car letter and spot
Date: 2026-11-06
Files: Assets/_Project/Scripts/UI/ (PickupBannerFormatter, TicketBoard, DebugOverlay), Assets/_Project/Tests/EditMode/UI/PickupBannerTests.cs
Why: design.md section 10 pickup feedback. The banner now reads "PICKUP: car B from spot 4", one line per pending pickup (max 3, then "+N more"), letter only when no spot is known, and its height follows the line count. TicketBoard.AppendPickups fills a reused StringBuilder with no LINQ; text rules live in PickupBannerFormatter.
Tests: added PickupBannerTests (none, one, unknown spot, filtering, three, cap with +2 more, closed ticket).

## integration: boss message panel and run-lost card
Date: 2026-11-07
Files: Assets/_Project/Scripts/UI/ (MessagePanel, MessagePanelStyles, MessageFeed, MessageFeedSettings, MessageStatus, FeedMessage, MessageFormatter, RunLostCard, TuningPanel), Assets/_Project/Scripts/Bootstrap/GameBootstrap.cs, Assets/_Project/Editor/BootstrapSceneBuilder.cs, Assets/_Project/Tests/EditMode/UI/MessageFeedTests.cs
Why: design.md section 6. Right-hand BOSS panel fed only by OrderIssued, OrderResolved, StrikeGiven, NightStarted (Core events): strike marks, up to 5 cards, OPEN/DONE/FAILED badges, flash on arrival, Cancelled fades, silent. It sits below the pickup banner, shrinks to the screen, caps at 30% width and hides while the F2 panel is open (TuningPanel.IsOpen). RunLostCard shows on the final strike and freezes the game with Time.timeScale 0; its New run button calls a handler set via Configure, which is NOT wired yet: NextNightRequested cannot be reused because NightFlow ignores it while a night runs and nothing ends the night on the final strike (lead decision). Scene must be rebuilt (Build Everything) to add the two new children; until then GameBootstrap logs an error and skips them. Orders is not wired.
Tests: added MessageFeedTests (issue, flash, each outcome, cap, cancel, reset on night 1, strikes); not run, the sandbox blocked launching Unity. TuningSeeder tests may fail on the four unseeded TuningKey values (expected).

## integration: wire Orders module, run lost via summary card
Date: 2026-11-07
Files: Assets/_Project/Scripts/Bootstrap/ (GameBootstrap, TuningSeeder, asmdef), Assets/_Project/Editor/ (BootstrapSceneBuilder, ProjectSetup, asmdef), Assets/_Project/Scripts/UI/SummaryCardFormatter.cs, Assets/_Project/Scripts/UI/RunLostCard.cs (deleted), Assets/_Project/Tests/EditMode/UI/SummaryCardFormatterTests.cs, Assets/_Project/Tests/EditMode/Integration/TuningSeederTests.cs
Why: design.md section 6. BossOrderDirector is a scene child with OrdersConfig, activated in GameBootstrap.Awake (before the one-frame-late night 1, so it hears NightStarted). TuningSeeder calls OrdersConfig.SeedInto, so all TuningCatalog keys are seeded. RunLostCard and its timeScale freeze are removed: a lost run ends through NightSummaryCard (title 'The boss is finished with you', headline cites strikes and never 'paid off', button 'New run'). ProjectSetup creates OrdersConfig if missing. Scene must be regenerated with ProjectSetup.RunBatch.
Tests: added 3 RunLost formatter tests and 2 TuningSeederTests. Not run: the parallel Orders/Lot changes (IChargingSpotQuery removal, IsChargingSpot) are not merged, so the Orders director and fixture do not compile against Core on this branch.

## integration: status Washed/Charged clear on park; closer camera
Date: 2026-11-10
Files: Assets/_Project/Scripts/UI/TicketRow.cs, TicketNeedsTracker.cs, Status/WashedStatusRule.cs, Status/ChargedStatusRule.cs, Assets/_Project/Scripts/Bootstrap/CameraFollow.cs, Assets/_Project/Scenes/Bootstrap.unity, Tests/EditMode/UI/CarStatusTests.cs, Tests/EditMode/UI/TicketBoardTests.cs, Tests/EditMode/Integration/CameraFitTests.cs
Why: A washed or charged car kept Washed/Charged after being parked (design Part A, CAR SLOTS). They are now flags set by CarCleaned/CarFullyCharged and cleared by CarParked. Camera fit tightness 0.87 -> 0.78; play-area floor unchanged.
Tests: wash and charge event replays, mid-wash drive-out, pickup priority; EditMode suite run.

## integration: patience 25 min and tip pop over a boarding patron
Date: 2026-11-10
Files: Scripts/Patrons/PatronRequest.cs, Data/Patrons/PatronConfig.asset, Tests/EditMode/Patrons/LiveTuningTests.cs, Scripts/UI/Hud/{TipPopSettings,PendingTipPops,TipPopKind,TipPopText,TipPops,TipPopTracker,TipPopPainter,PlayingHud}.cs, Scripts/UI/GameUi.cs, Scripts/UI/Mood/HudEmotionConfig.cs, Tests/EditMode/UI/TipPopTests.cs
Why: Play-test notes: pickup patience default 45 -> 25 game minutes (impatient 15; patrons module one-off edit), and a floating "+$N" over the car when the patron boards (design.md A4 tips and pickup).
Tests: 14 new EditMode (PendingTipPops pairing, text/look/curves, pool); LiveTuningTests updated to 25/15. EditMode 992 pass, 0 fail.

## integration: vertical power meter HUD
Date: 2026-11-11
Files: Assets/_Project/Scripts/UI/Hud/{PowerMeterSettings,PowerMeterView,ActivePowers,PowerMeterTracker,PowerMeterPainter,PowerColors,SquareRing,PlayingHud}.cs, Scripts/UI/GameUi.cs, Scripts/UI/Mood/HudEmotionConfig.cs, Tests/EditMode/UI/PowerMeterTests.cs
Why: Issue #39, valet_powers.md section 4. Vertical bar on the right edge (gold, ice blue, hot pink notches, smooth fill, pulse on every fill, star at the level reached) plus a letter icon with a countdown ring per active power. Listens only to Core PowerMeterChanged, PowerActivated and PowerEnded; ring durations are serialized in PowerMeterSettings (HudEmotionConfig.PowerMeter), no Powers reference.
Tests: 12 new EditMode (view smoothing, pulse, ready, active timers, tracker events and dispose, ring geometry); EditMode suite 1004 pass, 0 fail.

## integration: wire Powers, PERFECT! pop, camera kick, ding, results stat
Date: 2026-11-11
Files: Assets/_Project/Editor/{BootstrapSceneBuilder,ValetSceneBuilder,PowersSceneBuilder,ProjectSetup}.cs, Editor/UncannyValet.Editor.asmdef, Scenes/Bootstrap.unity, Data/Powers/PowerMeterConfig.asset, Scripts/Bootstrap/{GameBootstrap,CameraFollow,CameraKick,PerfectKickLink,PerfectFeedback,PedestrianBootstrap}.cs, Scripts/UI/{ShiftStats,GameUi}.cs, Scripts/UI/Hud/{TipPopSettings,TipPopText,TipPops,TipPopTracker}.cs, Scripts/UI/Screens/ResultsFormatter.cs, Scripts/Audio/{SfxId,SfxRecipeBook,SfxEventRouter,SfxAlerts}.cs (one small hook, authorised by the lead), Tests/EditMode/Integration/{PerfectFeedbackTests,TransformCarFactory}.cs
Why: Issues #16 (feedback), #38 (wiring), #60 (builder split; BootstrapSceneBuilder 214 to 146 lines, valet and camera moved to ValetSceneBuilder). A Powers object (PowerMeterController, PowerActivationController, PerfectFeedback) starts inactive and GameBootstrap activates it after the bus and tuning are registered; PowerMeterConfig is created by Setup and passed as powers: to TuningSeeder. PerfectPark and PerfectDropOff give a PERFECT! pop over the car, a 0.08 s / 0.05 unit camera kick along the car nose (CameraFollow serialized fields), a ding, and the results screen shows PERFECT PARKS. PedestrianBootstrap extracted to keep GameBootstrap under 200 lines. design.md A8, valet_powers.md, car_handling_spec.md section 4.
Tests: 8 new EditMode (kick maths, kick link, pop tracker, stats, results line, ding routing). RunBatch ok, EditMode 1062 pass, PlayMode 16 pass.
