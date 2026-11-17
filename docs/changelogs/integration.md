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

## integration: fix tip pop never showing after boarding
Date: 2026-11-11
Files: Assets/_Project/Scripts/UI/Hud/{PendingTipPops,TipPopTracker}.cs, Tests/EditMode/UI/TipPopTests.cs, Tests/EditMode/Integration/{TipPopFlowTests,TransformCarFactory}.cs
Why: Refs #67. PatronBoarded makes Patrons/PatronDeparture publish CarDeparted and despawn the car synchronously, and it subscribes before the HUD, so the tracker dropped the held tip on CarDeparted and then could not find the car. The tracker now reads the car's spot when the ticket closes, keeps it with the tip, and no longer drops on CarDeparted (PatronWalkers always publishes PatronBoarded for a delivered patron; ShiftEnded still clears). Settings from HudEmotionConfig.asset checked: defaults survive the missing nested field.
Tests: 2 new EditMode (real bus + real PatronDeparture + tracker; shipped asset settings), 1 changed. EditMode 1074 pass, PlayMode 16 pass.

## integration: Will not tip state, angry at 80%, tunable levels [Claude]
Date: 2026-11-11
Files: Assets/_Project/Scripts/Core/Events/TuningKey.cs, Core/Contracts/{EmotionRows,TuningCatalog}.cs, Scripts/Bootstrap/{TuningSeeder,GameBootstrap}.cs, Scripts/UI/GameUi.cs, Scripts/UI/Hud/KeyBoardPainter.cs, Scripts/UI/Mood/{Emotion,EmotionRule,EmotionStyle,EmotionTuningLink,HudEmotionConfig,MoodBubbleLayout}.cs, Scripts/UI/Status/{CarStatus,CarStatusTable,CarStatusResolver,CustAngryStatusRule,WillNotTipStatusRule}.cs, Data/UI/HudEmotionConfig.asset, Tests/EditMode/UI/{EmotionTests,EmotionTuningLinkTests,CarStatusTests,PickupMarkTests}.cs
Why: Refs #7, design.md A8. Angry (red plate, bubble, Cust. Angry) now starts at 80% of the pickup patience (was 85%); at 100% or on expiry the patron is WillNotTip: dark red, slow flash, frown bubble, status word "Won't tip" (short "No tip"). Both levels are F2 Advanced rows (Patron mood) seeded from HudEmotionConfig and applied live by EmotionTuningLink. The CAR SLOTS panel is 16 px wider so "Cust. Angry" and "Won't tip" show in full (issue 11). Tip maths unchanged.
Tests: EditMode 1082 pass (threshold, live retune, status rule, link, catalog; 85% tests updated); RunBatch ok (scene churn reverted); PlayMode 16 pass.

## integration: how-to-play screen after the first title press [Claude]
Date: 2026-11-11
Files: Assets/_Project/Scripts/UI/GameUi.cs, Scripts/UI/Screens/{UiPhase,UiInput,HowToPlayGate,HowToPlayText,HowToPlayPainter,WordWrap,HighlightMask}.cs, Tests/EditMode/UI/HowToPlayTests.cs
Why: Refs #72. Flow is Title -> any key -> HowToPlay (first time per page load only) -> any key -> Playing; retry goes straight to Playing. HowToPlayGate is an in-memory flag. The shift starts (TitleDismissed) only when how-to-play is dismissed; the press that changes a phase is consumed that frame. Key words are drawn gold per character; text is wrapped once at construction. Key polling moved to UiInput to keep GameUi under 200 lines.
Tests: 6 new EditMode (gate, wrap, highlight mask, text fits and has glyphs). EditMode 1117 pass, PlayMode 16 pass. No scene or asset change, so no RunBatch.

## integration: F2 panel Copy all values button [Claude]
Date: 2026-11-11
Files: Assets/_Project/Scripts/UI/{TuningPanel,TuningSnapshotJson}.cs, Assets/_Project/Tests/EditMode/UI/TuningSnapshotJsonTests.cs
Why: Refs #76. A "Copy all values" button next to Advanced and Reset all puts a flat JSON object of every catalog key (Advanced ones too, ordered by enum value, invariant culture) on the clipboard via GUIUtility.systemCopyBuffer, and shows "Copied!" for 1.5 s of unscaled time. JSON is built only on press by the pure TuningSnapshotJson.
Tests: 6 new EditMode (object shape, every key, no duplicates, enum order, dot decimals under de-DE, null for non-finite). EditMode 1165 pass, PlayMode 16 pass. No scene or asset change, so no RunBatch.

## integration: fix start-up crash, add Bootstrap smoke test [Claude]
Date: 2026-11-11
Files: Assets/_Project/Data/Director/DifficultyDialsConfig.asset, Assets/_Project/Tests/PlayMode/BootstrapSceneSmokeTests.cs, Assets/_Project/Tests/PlayMode/UncannyValet.Tests.PlayMode.asmdef
Why: Refs #77, #73. Commit c6992c0 (traffic) removed TrafficAmbientPerMinute from the middle of the TuningKey enum, shifting every later value down by one. DifficultyDialsConfig.asset stores TuningKey as raw ints, so every dial link pointed at the wrong key and DifficultyDials.Derive threw KeyNotFoundException in GameBootstrap.Awake, aborting start-up (lot never built). Asset ints above the removed slot remapped by minus one. New PlayMode smoke test loads the real Bootstrap scene, starts the shift and fails on any logged error or exception.
Tests: smoke test failed with the exact KeyNotFoundException before the fix and passes after (about 5 s). EditMode 1215 pass, PlayMode 17 pass. No scene change, so no RunBatch.

## integration: screen shake on hard impacts [Claude]
Date: 2026-11-12
Files: Assets/_Project/Scripts/Bootstrap/{CameraShake,ShakeLink,CameraFollow,PerfectFeedback,TuningSeeder}.cs, Assets/_Project/Scripts/Core/Contracts/{ShakeRows,TuningCatalog}.cs, Assets/_Project/Scripts/Core/Events/TuningKey.cs, Assets/_Project/Tests/EditMode/Integration/CameraShakeTests.cs
Why: Refs #20. The camera shakes when the car the player drives has a counted hard hit (CarImpact with IsPlayerCar). Pure CameraShake (decaying two-sine offset, unscaled time, no allocation, weaker hits never replace a stronger running shake) is fed by ShakeLink, created from PerfectFeedback.Start; CameraFollow adds the offset beside the Perfect Park kick so smoothing never chases it. Three F2 Advanced rows (strength, length, full-strength force) with TuningKeys appended at the end; defaults seeded in TuningSeeder.
Tests: 10 new EditMode (zero below limit, scales, cap, decay to zero, deterministic, no stacking, link player/non-player/below-limit). EditMode 1324 pass, PlayMode 29 pass (incl. BootstrapSceneSmokeTests). No scene or asset change, so no RunBatch.

## integration: RunProgress, LotDefinition, lot scenes, picker [Claude]
Date: 2026-11-13
Files: Assets/_Project/Scripts/Bootstrap/{GameBootstrap,TuningSeeder,LotBounds,RunBootstrap,ValetSpawner}.cs and asmdef, Assets/_Project/Scripts/UI/{GameUi.cs,Screens/TitlePicker.cs,Screens/TitlePainter.cs,Screens/UiInput.cs}, Assets/_Project/Editor/{BootstrapSceneBuilder,RunAssetSetup,ProjectSetup}.cs and asmdef, Assets/_Project/Scenes/{Bootstrap,Lot_MidgardCrazy}.unity, Assets/_Project/Data/Run/*.asset, ProjectSettings/EditorBuildSettings.asset, Core LotId, World, LotKind, StoryPower, LotEvents, IRunProgress, ILotPicker, LotChoice, Assets/_Project/Tests/PlayMode/BootstrapSceneSmokeTests.cs and asmdefs
Why: Refs #80 (epic #79). GameBootstrap gets a LotDefinition and a RunDefinition; TuningSeeder applies the lot's shift length, arrival gaps, waiting cap (never above the layout's queue slots) and dial positions before the dials capture their baselines. RunBootstrap finds or creates the DontDestroyOnLoad RunHost, registers IRunProgress and ILotPicker, links the scene's events to RunProgress and publishes LotStarted. The scene builder generates one scene per lot of the RunDefinition (Bootstrap.unity stays scene 0 as the Midgard training lot, plus Lot_MidgardCrazy.unity). The title screen lists the lots (digit keys 1 to 9 and 0) in every build. GameBootstrap clears Services only while it still owns them, so a newer lot scene is never wiped by the old scene's teardown. Extracted ValetSpawner and LotBounds.TryFrame to keep GameBootstrap within the limit.
Tests: EditMode 1361 pass (new: run progress, run definition, lot definition rules, seeding, picker); PlayMode 31 pass (both Midgard lots load and play 4 s with no logged errors; the picker switches lot and the run state survives). RunBatch run; Bootstrap.unity has real changes (the two definition references).

## integration: queue slots and arrival budget per lot [Claude]
Date: 2026-11-13
Files: Assets/_Project/Scripts/Run/{LotDefinition,LotDefinitionRules,LotPacing}.cs, Scripts/Lot/{LotGeometry,LotLayoutBuilder,LotDecor}.cs, Scripts/Bootstrap/{GameBootstrap,TuningSeeder,QueueSlotCheck}.cs, Editor/{BootstrapSceneBuilder,RunAssetSetup}.cs, Scenes/{Bootstrap,Lot_MidgardCrazy}.unity, Data/Run/Midgard{Training,Crazy}Lot.asset, Tests/EditMode/{Run/QueueSlotsAndBudgetTests,Lot/QueueSlotCountTests,Run/LotDefinitionTests,Patrons/QueueSlotAllocatorTests}.cs, Tests/PlayMode/BootstrapSceneSmokeTests.cs
Why: Refs #80 follow-up. The queue slot count is a LotDefinition layout parameter (training 3, crazy 4); the scene builder writes it into the lot builder and the decor, and LotGeometry grows the lot below the curb for the extra slot (walls, entry gap, roads and camera frame follow from the geometry). The effective waiting cap stays min(cap, slots) (LotPacing.EffectiveMaxWaiting, now fed by the definition). Each lot has an arrival budget (0 = derived: shift length / mean arrival gap, rounded up, max 40) seeded to DirArrivalBudget before the dial link captures baselines, replacing the shared profile's 14 (slack stays the profile's 2). Training budget changes 14 to 8 (150 s / 20 s mean gap); crazy becomes 20 (was capped at 16). GameBootstrap verifies the built slot count against the definition. Seeder's queueSlots parameter removed (the definition owns it).
Tests: 10 new EditMode (validation, derived and authored budget, per-lot seeding, effective cap, geometry with 4 slots, 4-slot allocator); EditMode 1371 pass; PlayMode 33 pass (training 3 slots and cap 2, crazy 4 slots with 4 cars parked in the slots, budgets not capped). RunBatch run; scene churn reverted, only the two `_queueSlots` fields kept per scene.

## services: one model for what a car needs (service modifiers) [Claude]
Date: 2026-11-13
Files: Assets/_Project/Scripts/UI/ (RowModifier, TicketRow, TicketNeedsTracker, TicketBoard, Hud/ServiceIcons, Hud/PlayingHud, Status/*, Kit/{PixelIcon,PixelIconData,PixelPalette,ModifierIcons}, TuningRowView), Scripts/Bootstrap/{TuningSeeder,GameBootstrap,ModifierBootstrap,DirectorBootstrap}.cs, Editor/{ModifierAssetSetup,ProjectSetup,RunAssetSetup}.cs, Data/Modifiers/*, Data/Shift/ShiftConfig.asset, Tests/PlayMode/ServiceModifierPlayModeTests.cs
Why: Issue #121. The board rows hold a modifier list; icons next to the plate come from each open modifier (mud splat, plug); the key board status words are generic (Servicing and Serviced take their word from the modifier). Bootstrap registers the lot's IModifierCatalog, seeds clear times, bonuses and chances from the definitions and the lot, and the setup tool creates the two definition assets and fills the lots' chance lists.
Tests: see docs/changelogs/services.md (EditMode and PlayMode all green).

## blessings: tips become blessings, favour, curses, lot threshold [Claude]
Date: 2026-11-13
Files: see docs/changelogs/blessings.md (this module's part of the same commit).
Why: Issue #81 of epic #79: tips, combo and complaints are renamed to blessings, favour and curses; stars are replaced by the lot's blessing threshold.
Tests: see docs/changelogs/blessings.md (EditMode and PlayMode all pass).

## valet: Thunder stride dash on foot and car boost with lightning, crack and kick [Claude]
Date: 2026-11-13
Files: Bootstrap: ThunderKickLink (new), PerfectFeedback (wires the kick and ThunderBoostLink); Editor/ValetSceneBuilder.cs. Part of the valet commit for issue #88.
Why: issue #88 of epic #79 (Valet to Valhalla): one power, one bucket and one 4 s cooldown. On foot a 0.2 s straight dash of 5 units (8 at level 2) in the move direction else the facing, passing pedestrians (layer ignore pair for the dash) and stopping at solids; in a car a 1.5 s (2.2 s) boost of top speed x1.5 and acceleration x2 through the Cars drive modifier seam; pixel lightning streak, thunder crack and a camera kick.
Tests: EditMode 1409 passed (ThunderDashTests, ThunderStrideTests, ThunderBoostTests, ThunderSfxTests, ThunderKickLinkTests, DevPowerLinkTests key checks); PlayMode 37 passed (ThunderStridePlayModeTests: dash distance per level and pedestrian pass-through, wall stop, boosted top speed, boosted wall damage, cooldown) incl. BootstrapSceneSmokeTests; RunBatch run.

## integration: story flow between lots, intro, reward and end cards [Claude]
Date: 2026-11-14
Files: Core/Story/* (StoryTextAsset, WorldStory, LotStory, PowerStory, StoryGenericText, StoryTextRules), Core/Contracts/{IStoryFacts,StoryLotFacts,StoryPowerFacts,LotPickerExtensions}.cs, UI/Flow/* (FlowMachine, FlowState, FlowStep, FlowController, FlowEventLink, IFlowActions, DevLotPicker, StoryCard, StoryCardLayout, StoryCardBuilder, IntroCards, RewardCards, EndCards, StoryCardPainter, PendingReward), UI/GameUi.cs, UI/Screens/{UiInput,ResultsPainter,ResultsFormatter}.cs (UiPhase and HowToPlayGate removed), Bootstrap/{FlowHost,FlowActions,FlowBootstrap,RunStoryFacts,StartLotRule,RunBootstrap,GameBootstrap}.cs, Editor/{StoryAssetSetup,StoryTextDrafts,BootstrapSceneBuilder,ProjectSetup}.cs, Data/UI/StoryText.asset, the four lot scenes, Tests/EditMode/Flow/*, Tests/PlayMode/StoryFlowPlayModeTests.cs
Why: Issue #84 of epic #79. The game flow is now title, how-to-play (once per page load), the lot's intro card, the shift, results, a reward card when a power is earned, the thrown-back card after a failed crazy lot, and the end card (TO BE CONTINUED or THOR IS HOME) then the title with a fresh run. A pure FlowMachine (persistent on the RunHost object through FlowHost) holds the state across scene loads and reacts to NextLotChosen, RunCompleted and PowerGranted; all card words live in one editable StoryTextAsset (drafted from the design notes); the lot picker shows only while F3 is held on the title; Next Lot on the results screen is live (any key but Enter or Space, or the button).
Tests: EditMode FlowMachineTests, FlowControllerTests, StoryCardTests, StoryTextTests; PlayMode StoryFlowPlayModeTests.

## integration: story cards fit the panel with the shipped text [Claude]
Date: 2026-11-14
Files: UI/Flow/{RewardCards,EndCards}.cs (tighter gaps), Tests/EditMode/Flow/{ShippedCardsTests,StoryCardTests}.cs
Why: Issue #84. A test builds every intro, reward (both levels), thrown-back and end card from the shipped story text, run and power definitions and checks each line sits inside the panel and above the prompt; the finale end card with four powers overflowed by one row and its gaps were tightened.
Tests: EditMode 1721 passed; PlayMode 66 passed (incl. StoryFlowPlayModeTests, BootstrapSceneSmokeTests).

## integration: GameUi hands card building to StoryCardCache [Claude]
Date: 2026-11-14
Files: UI/GameUi.cs, UI/Flow/StoryCardCache.cs, Tests/PlayMode/StoryFlowPlayModeTests.cs
Why: Issue #84. GameUi was over the 200-line limit; the once-per-screen card building moved into a small cache class.
Tests: EditMode 1721 passed; PlayMode 66 passed.

## integration: no retry after a failed crazy lot, lot picker on F4 [Claude]
Date: 2026-11-14
Files: UI/Flow/{FlowMachine,FlowController}.cs, UI/GameUi.cs, UI/Screens/{ResultsPainter,UiInput}.cs, Tests/EditMode/Flow/{FlowMachineTests,FlowControllerTests}.cs
Why: Issue #84 owner decisions. After a failed crazy lot or finale the results screen offers only the continue path (Retry button greyed, Enter continues, hint reads ANY KEY: NEXT LOT); Retry stays for failed training and cleared lots. The title's lot picker is shown while F4 is held (F4 is unused elsewhere); F3 stays the Director overlay toggle.
Tests: new EditMode tests FlowMachine Retry_IsOffAfterAFailedCrazyLot and FlowController Results_EnterContinuesInsteadOfRetrying; NOT run, see the report (Unity batch mode hung on a stuck bee_backend).

## integration: results screen shows the local god verdict, Refs #85 [Claude]
Date: 2026-11-14
Files: Core/Story/{VerdictTier,VerdictRule,VerdictStory,TrainerStory,VerdictTextRules,StoryTextAsset,StoryTextRules}.cs, UI/Screens/{VerdictPicker,VerdictSpeech,ResultsContent,ResultsFormatter,ResultsPainter}.cs, UI/GameUi.cs, Editor/{VerdictTextDrafts,StoryAssetSetup}.cs, Data/UI/StoryText.asset, Tests/EditMode/Flow/VerdictTests.cs, Tests/EditMode/UncannyValet.Tests.EditMode.asmdef
Why: Issue #85. Crazy lots and the finale show the god name and a failed, cleared or cleared-well speech line (cleared well at 1.5x the threshold, a field of the story text); training lots show a generic trainer line without a panel. Lines drafted for all nine gods and eight worlds in the editable story text asset (setup adds missing entries, never overwrites edits). Stats moved to two columns. GameUi reads the story text asset (Core), not the Run module. EditMode tests now reference the Editor assembly (for the drafts).
Tests: EditMode 1772 passed (new VerdictTests); PlayMode 71 passed; RunBatch ok.

## integration: run-unity wrapper guards Unity batch runs against commit exhaustion, Refs tooling [Claude]
Date: 2026-11-14
Files: tools/run-unity.ps1, tools/run-unity.sh, tools/README-unity.md, tools/tests/test-run-unity.ps1, tools/run-tests.ps1, tools/run-tests.sh, tools/build-web.ps1
Why: Windows commit memory hit 67.5 of 68.5 GB on 2026-11-14 (paging file error 1455, hung Unity.exe and bee_backend.exe). The wrapper adds a machine-wide single-run lock, an 8 GB free-commit guard, a timeout that kills the process tree it started, memory-crash diagnosis and cleanup in finally; the test and web build scripts launch Unity through it.
Tests: tools/tests/test-run-unity.ps1 with fake executables (12 checks: dry run, exit code passthrough, memory diagnosis, headroom refusal, timeout tree kill, lock release, lock contention, lock wait give-up) all passed. Unity was not launched.

## integration: smaller watcher speech bubbles, tunable scale (Refs #122) [Claude]
Date: 2026-11-14
Files: Assets/_Project/Scripts/UI/Speech/SpeechBubbleSettings.cs, SpeechBubblePainter.cs; Assets/_Project/Scripts/UI/Kit/PixelCanvas.cs; Assets/_Project/Scripts/UI/Mood/HudEmotionConfig.cs; Assets/_Project/Scripts/UI/Hud/PlayingHud.cs; Assets/_Project/Scripts/UI/GameUi.cs
Why: owner playtest note (#122): the bubbles were too big. The bubble (panel, name, text, tail) is drawn at a tunable share of its size (HudEmotionConfig "Speech Bubble" Scale, Range 0.4 to 1, default 0.72 = about 28% smaller) by a zoom in PixelCanvas that shrinks towards the tail, so the clamped box stays on screen.
Tests: EditMode 1801 passed, PlayMode 73 passed, RunBatch ok.

## integration: zoom the game camera out a little (Refs #123) [Claude]
Date: 2026-11-14
Files: Assets/_Project/Scripts/Bootstrap/CameraFollow.cs; Assets/_Project/Scenes/Bootstrap.unity, Lot_MidgardCrazy.unity, Lot_VanaheimCrazy.unity, Lot_VanaheimTraining.unity (only the _fitTightness line)
Why: owner playtest note (#123): the view was too close. CameraFollow Fit Tightness 0.78 to 0.83 (size x1.064, about 13% more visible area); the play area is still never cropped, and the Max Orthographic Size cap (15) is unchanged. Scene values were updated by RunBatch; its pure fileID churn was reverted.
Tests: EditMode 1801 passed, PlayMode 73 passed, RunBatch ok.

## integration: the camera always shows the whole lot, whatever the zoom cap (Refs #123 #127) [Claude]
Date: 2026-11-14
Files: Assets/_Project/Scripts/Bootstrap/CameraFit.cs, CameraFollow.cs (tooltip); Assets/_Project/Tests/EditMode/Integration/CameraFitTests.cs; Assets/_Project/Tests/PlayMode/CameraFramingPlayModeTests.cs (+ .meta)
Why: root cause of the cut-off bottom of the taller lots (4th queue slot, #127): CameraFit clamped the zoom to Max Orthographic Size 15, which is below what the lot needs, so the play area was cropped. The play-area floor now wins over the cap (the cap only limits zoom-out for margin and decor). Zoom-out intent of #123 untouched (tightness 0.83 kept). PlayMode test asserts the valet, diner door, valet start and every queue slot are inside the camera view on all four lots at 16:9 and 16:10 (it failed before the fix on queue slot 3 at size 15).
Tests: EditMode 1802 passed (new LotTallerThanTheCapStillFits), PlayMode 77 passed (4 new), RunBatch ok (scene fileID churn reverted).

## integration: speech bubble only while the speaker is in the camera view (Refs #129) [Claude]
Date: 2026-11-14
Files: Assets/_Project/Scripts/UI/Speech/SpeechBubbleVisibility.cs, SpeechBubbleSettings.cs (ViewMargin), SpeechBubblePainter.cs (+ .meta); Assets/_Project/Tests/EditMode/UI/SpeechBubbleVisibilityTests.cs (+ .meta)
Why: Grandma tip bubble drew at the screen edge even when she was off screen. The painter now skips the bubble while the speaker viewport position is outside the view plus a tunable margin (ViewMargin, default 0.02); the tracker keeps its timer, so the bubble returns if she re-enters before it expires. Tip money is unaffected (display only). Uses the camera HUD already passes in; no per-frame allocation.
Tests: EditMode SpeechBubbleVisibilityTests (7, incl. leave and re-enter regression).

## integration: speech bubbles off by default, car status reads No blessing (Refs #134 #135) [Claude]
Date: 2026-11-14
Files: Assets/_Project/Scripts/UI/Speech/SpeechBubbleSettings.cs (Enabled switch, default off), SpeechBubbleTracker.cs (enabled gate), Hud/PlayingHud.cs, Status/CarStatusTable.cs, CarStatusResolver.cs (comment), Core/Contracts/EmotionRows.cs (tooltip wording only, approved); Assets/_Project/Tests/EditMode/UI/SpeechBubbleTests.cs, CarStatusTests.cs
Why: playtest: speech bubbles distract (#134). One serialized bool on HudEmotionConfig's Speech Bubble settings turns them back on; off, the tracker never subscribes to SpeechShown and no painter is built, so nothing is wrapped, tracked or drawn (tips and other events unchanged; blessing and emotion icons untouched). #135: the Unblessed status label is now No blessing (fallback No bless when the box is too narrow; the key board box has room for the full label).
Tests: EditMode SwitchedOff_NeverTracksABubble, TheSettingsDefaultIsOff, updated label-fitting test; full suite run before commit.

## integration: power bar markers at real thresholds, prayer hands for blessings [Claude]
Date: 2026-11-14
Files: Assets/_Project/Scripts/Core/Contracts/PowerBarThresholds.cs (new, one threshold list), Powers/PowerBar.cs, UI/Hud/PowerBarPainter.cs, BlessingPopPainter.cs, BlessingPops.cs, HudPainter.cs, UI/Screens/ResultsPainter.cs, UI/Kit/PixelIcon.cs, PixelIconData.cs; Tests/EditMode/UI/PowerBarMarkerTests.cs, HudLayoutTests.cs (+ .meta)
Why: #142 the bolt markers were drawn at the centre of each third of the bar (first bolt at about 17 percent) while the first power opens at 33.3, so the fill passed the bolt long before the power worked. Markers now sit at the threshold fraction from the same list the bar uses (new Core file, flagged to the lead), with one pixel rounding shared by fill and markers. #143 blessings show a new PrayerHands pixel icon (HUD counter, blessing pops, results score); the bolt stays on the power bar.
Tests: EditMode 1883 passed (4 new marker tests).

## integration: red -N pop when a pedestrian hit costs blessings (Refs #152) [Claude]
Date: 2026-11-16
Files: Assets/_Project/Scripts/UI/Hud/CursePopRule.cs, CursePopTracker.cs, BlessingPopKind.cs, BlessingPops.cs, BlessingPopPainter.cs, BlessingPopSettings.cs, PlayingHud.cs, UI/Kit/PixelPalette.cs (+ .meta); Assets/_Project/Tests/EditMode/UI/CursePopTests.cs (+ .meta)
Why: the slight and full pedestrian curses took blessings with no feedback. A red "-N" (prayer hands after it) now rises at the hit spot, N = blessings really lost (clamped at 0, no pop when none). Uses existing events only: PedestrianHit (spot), CurseApplied with no ticket (amount), BlessingsChanged (running total); the two arrive in either order and pair within one frame, so one pop per hit and a locked-out hit never pops. Font scale and lift are tunable in BlessingPopSettings.
Tests: EditMode CursePopTests (11); full suite 1937 passed.

## ui: +X bolt pop beside the power bar on PowerFilled, Refs #160 [Claude]
Date: 2026-11-17
Files: Assets/_Project/Scripts/UI/Hud/ (new: PowerFillPopSettings, PowerFillPops, PowerFillPopRule, PowerFillPopTracker, PowerFillPopPainter; edited PlayingHud, BoltGlyph (alpha), PowerBarPainter (layout consts public)); UI/Mood/HudEmotionConfig.cs, UI/GameUi.cs; Tests/EditMode/UI/PowerFillPopTests.cs (new)
Why: #160 each PowerFilled shows "+X" (amount rounded, nothing under 0.5) with the lightning bolt, rising and fading left of the power icons. Pops arriving together take different stack slots (lowest slot no recent pop still holds) so they never overlap; fixed pool, no per-frame allocation; cleared on ShiftStarted and ShiftEnded. Colour per source is its tint pulled toward gold. Tunables in PowerFillPopSettings inside HudEmotionConfig (new field takes its default from the code, no asset edit). Drawn only while the power bar itself is visible.
Tests: EditMode 1969 passed (PowerFillPopTests, 15 cases). PlayMode and Web build not run.

## integration: remove CAR SLOTS board and plates, add pickup timer pie, Refs #154 #155 #156 [Claude]
Date: 2026-11-17
Files: Assets/_Project/Scripts/UI/Pie/ (new: PickupPieRule, PiePixels, PickupPieSprites, PickupPieView, PickupPieLayer, PickupPieSettings, PieObjects); UI/Hud/PlayingHud.cs, WorldTicketPainter.cs, PickupMark.cs; UI/TicketBoard.cs, TicketRow.cs, TicketNeedsTracker.cs, GameUi.cs; UI/Mood/EmotionStyle.cs, HudEmotionConfig.cs; deleted UI/Status/ (9 classes), UI/Hud/KeyBoardPainter, KeyBoardRows, PlatePainter, PlateLayout, DamageTag; Tests/EditMode/UI/PickupPieTests.cs (new), PickupMarkTests, TicketBoardTests, HudRulesTests, EmotionTests edited; CarStatusTests, PlateUiTests, DamageTagTests deleted (+ .meta)
Why: #154 the CAR SLOTS key board is gone, with the status table that only fed it and the damage tag that only showed there. #155 the licence plate under cars is no longer drawn (the CarPlateAssigned data stays in Core and Patrons); service icons now sit centred under the car. The plate flash is gone, so the flash-rate fields and EmotionStyle.IsLit/PeriodOf went too. #156 a world-sprite pie over each car with a running pickup timer: wedge cut clockwise from 12 o'clock (full at 0, empty at 1), green/yellow/red from the HudEmotionConfig patience levels, pre-built point-filtered sprites tinted by renderer colour, pooled, released when the timer stops, the ticket closes or the car is destroyed. Size, steps, offset and sorting order in PickupPieSettings (inside HudEmotionConfig). design.md sections on plates and CAR SLOTS are now out of date.
Tests: EditMode 1933 passed (PickupPieTests, 8 new); PlayMode BootstrapSceneSmokeTests, StoryFlowPlayModeTests, ServiceModifierPlayModeTests 13 passed.

## integration: bay border flash, bigger pickup pie, Refs #158 [Claude]
Date: 2026-11-16
Files: Assets/_Project/Scripts/UI/Pie/ (new: BayBorderLayer, BayBorderRule, BayBorderSettings, BayBorderSprites, BayBorderView; PickupPieSettings default 20 px, offset 1.45); UI/Mood/HudEmotionConfig.cs, UI/Hud/PlayingHud.cs, UI/GameUi.cs, Bootstrap/GameBootstrap.cs; Tests/EditMode/UI/BayBorderTests.cs (new)
Why: #158 a bay's border flashes green when its car parks completely inside (CarParkedInside), then in the pickup-timer colour at 1.5/3/7 Hz (happy/impatient/angry) while its patron waits; released when the car leaves, the ticket closes, the shift ends or the car is gone. Bay rects come from the new ISpotGeometry, registered in Bootstrap when the lot implements it (no border until Lot implements it). Pie is 20 px (was 14).
Tests: EditMode 1942 passed (BayBorderTests, 9 new). PlayMode not run (run-tests.ps1 is EditMode only).

## ui: green PERFECT cues, clock tab under score panel, Refs #161 #162 [Claude]
Date: 2026-11-17
Files: Assets/_Project/Scripts/UI/Hud/ (new: ClockLayout, ClockSettings; BlessingPopKind +Perfect, BlessingPops, BlessingPopPainter, PowerFillPopRule, PowerFillPopPainter, HudPainter, PlayingHud); UI/Mood/HudEmotionConfig.cs, UI/GameUi.cs; Tests/EditMode/UI/ClockPlacementTests.cs (new)
Why: #161 the PERFECT! pop and the PerfectPark +X pop now use the bay border green (EmotionStyle Happy colour, one value passed from PlayingHud, no literal); ParkedInside keeps its lighter gold-pulled green. #162 the shift clock is a tab flush under the score panel at the left edge (was top centre over the lot); gap and inset tunable in ClockSettings (HudEmotionConfig.Clock), clear of the power column and its +X pops. Not checked visually.
Tests: EditMode passed (ClockPlacementTests, 9 new cases); RunBatch not needed (no Editor/Data/Scenes change).

## ui: JetBrains Mono UI font, title is Valet to Valhalla, Refs #168 #50 [Claude]
Date: 2026-11-17
Files: Assets/_Project/Scripts/UI/Kit/ (new: UiFontConfig, UiFontSizing, PixelFontRenderer; PixelCanvas, PixelKit, PixelGlyphs edited: bitmap atlas and glyph rows removed); UI/Screens/TitlePainter.cs; Assets/_Project/Art/Fonts/ (JetBrainsMono-Bold.ttf, OFL.txt); Assets/_Project/Resources/UiFontConfig.asset; Tests/EditMode/UI/UiFontTests.cs (new), PixelKitTests.cs; ProjectSettings/ProjectSettings.asset (productName); README.md; tools/write-build-info.sh (+ .meta files)
Why: #168 all UI text now draws with a real TrueType font instead of the 5x7 bitmap. Font: JetBrainsMono-Bold.ttf from https://github.com/JetBrains/JetBrainsMono (fonts/ttf), licence SIL Open Font License 1.1 (OFL.txt kept beside it), 274 KB, embedded in the build. It is monospaced (advance 0.6 em, capitals 0.73 em), so each letter is centred in the same 6x7 virtual-pixel cell as before: every layout, box and wrap width is unchanged and no label can overflow where it fitted before. Font size is asked in screen pixels (capitals one row tall) per grid scale and text scale, so letters are crisp at any window size; sizes snap to steps while pops zoom, and cap at 96 px. Letters are cached one-char strings drawn through a reused GUIStyle/GUIContent: no per-frame allocation. Size and nudge tunables are in Resources/UiFontConfig (_emPerRow, _maxFontSize, _zoomSizeStep, _verticalNudge). Still upper case only; colours untouched. #50 the title screen reads VALET TO / VALHALLA (two lines, each fits at scale 7); productName, README title and the build-info text renamed. site/ not rebuilt.
Tests: EditMode 2007 passed (UiFontTests: sizing maths, shipped font monospaced and every glyph within the text cell, title fits the canvas). Not checked visually in a Web build; PlayMode not run.

## wash: wash pie beside the pickup pie, blue drop when washed, Refs #169 [Claude]
Date: 2026-11-17
Files: Assets/_Project/Scripts/UI/Pie/ (new: PieSet, PieSlots, WashPieRule, WashPieLayer; PickupPieLayer now uses PieSet; PickupPieSettings +SideGap +WashColor); UI/Hud/ServiceIcons.cs, PlayingHud.cs; UI/Kit/PixelIcon.cs, PixelIconData.cs (appended Drop icon and key 'u'); Tests/EditMode/UI/WashPieTests.cs (new), ServiceIconsTests.cs edited (+ .meta)
Why: washing already takes WashMinutes game minutes in the wash slot (StationService, ModifierProgressChanged, F2 key WashMinutes), so no Core change or new key was needed; the UI reads TicketRow only. A blue pie (progress drains the wedge) shows over a muddy car while it sits in the wash and replaces the mud splat; when the wash completes a blue drop stays in its place until the ticket closes. With a pickup pie too, the two sit side by side (gap in PickupPieSettings.SideGap). Pools shared through PieSet, no per-frame allocation.
Tests: EditMode WashPieTests (5) and ServiceIconsTests updated; full suite 2008 passed. Not checked visually.

## cars: despawn every car at once on ShiftEnded, Refs #170 [Claude]
Date: 2026-11-17
Files: Assets/_Project/Scripts/Cars/ShiftEndCleanup.cs (new), CarFactory.cs (DespawnAll, subscription created in Start, disposed in OnDestroy); Tests/EditMode/Cars/ShiftEndCleanupTests.cs, Tests/PlayMode/ShiftEndDespawnTests.cs (new, + .meta)
Why: cars leaving at the end of a shift drove off screen behind the results screen. Despawn goes through the existing CarFactory.Despawn, so seats, stations and trips are released by the destroyed car as before; PatronDeparture's later drive-away call finds no car and does nothing. Ambient traffic is untouched.
Tests: EditMode 2008 passed; PlayMode ShiftEndDespawnTests passed (full PlayMode 84 of 86; PowerBarPlayModeTests.FreezeTime_Owned... and VanaheimStormWindPlayModeTests.WithStormWindOwned... fail in power-bucket asserts, areas this change does not touch).

## ui: plain background for title and how-to, lot intro and EV text fixes [Claude]
Date: 2026-11-17
Files: Scripts/UI/Screens/TitlePainter.cs, HowToPlayPainter.cs, HowToPlayText.cs, Editor/StoryTextDrafts.cs, Data/UI/StoryText.asset, Tests/EditMode/UI/HowToPlayTests.cs
Why: Refs #171 #172. Title and how-to-play now fill the screen with PixelPalette.PanelDark instead of veiling the lot; removed the Thor line from the Midgard training intro and the EV charging words from the how-to-play special-requests line.
Tests: EditMode suite passed; RunBatch ran (its unrelated scene/config rewrites were reverted).
