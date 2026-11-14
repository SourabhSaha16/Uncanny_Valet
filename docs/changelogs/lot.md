# Lot changelog

Entries are added by the lot agent, one per commit. Format in docs/changelogs/README.md.

## lot: build lot layout, spots, curb zone and parked rule
Date: 2026-11-05
Files: Assets/_Project/Scripts/Lot/{LotConfig,LotLayoutBuilder,RectFactory,StopZone,ParkingSpot,CurbZone,StopTimer,SpotOccupancy}.cs (LotModule.cs removed), Assets/_Project/Tests/EditMode/Lot/{SpotOccupancyTests,StopTimerTests}.cs
Why: design.md sections 4, 9, 10: runtime-generated lot, CarParked / CarLeftSpot / CarAtCurb after a car is stopped and empty for 0.5 s.
Tests: 8 EditMode tests added (SpotOccupancy, StopTimer); EditMode run in batch mode.

## lot: guard destroyed cars in stop zones
Date: 2026-11-05
Files: Assets/_Project/Scripts/Lot/StopZone.cs, LotConfig.cs, LotLayoutBuilder.cs
Why: qa finding: a car despawned inside the curb zone made StopZone.Update throw every frame. Destroyed cars are now pruned; arrival inset and valet start offset moved into LotConfig (design.md section 9).
Tests: none added (Unity-null check is not a pure helper); Unity not run per request.

## lot: draw spot numbers
Date: 2026-11-05
Files: Assets/_Project/Scripts/Lot/SpotLabel.cs, LotConfig.cs, LotLayoutBuilder.cs
Why: play-test: players could not tell which car is in which spot. World-space TextMesh labels (built-in LegacyRuntime font, WebGL safe) show each SpotId and a CURB label; size and colour are LotConfig fields.
Tests: none (rendering only); Unity not run per request.

## lot: arrival queue slots, vertical CURB label, release destroyed cars
Date: 2026-11-06
Files: Assets/_Project/Scripts/Lot/{ArrivalQueue,WallBuilder,ZoneRoster,StopZone,ParkingSpot,CurbZone,LotConfig,LotLayoutBuilder,SpotLabel}.cs, Assets/_Project/Tests/EditMode/Lot/{ArrivalQueueTests,ZoneRosterTests}.cs
Why: design.md section 10 pressure rules. ArrivalSlots: 3 slots in a line below the curb (count, spacing, gaps in LotConfig; ArrivalInset replaced); ground grows to fit them. CURB text is one letter per line and sized to fit the curb box. A car destroyed while settled in a spot now frees the spot and publishes CarLeftSpot (StopZone logic moved into pure ZoneRoster, keyed by CarId).
Tests: 9 EditMode tests added (ArrivalQueue, ZoneRoster); Unity not run per request.

## lot: arrival slots side by side below the curb
Date: 2026-11-06
Files: Assets/_Project/Scripts/Lot/{ArrivalQueue,LotConfig,LotLayoutBuilder}.cs, Assets/_Project/Tests/EditMode/Lot/ArrivalQueueTests.cs
Why: the 3 waiting cars were 5.5 units apart in a vertical line and did not fit the camera. They now sit in adjacent bays (pitch car width + 0.6 gap) in a row going -X from the curb column, nose towards the curb (rotation 0, configurable); the ground and walls grow to fit (design.md section 10).
Tests: ArrivalQueueTests rewritten (slot 0 = first point, pitch, no overlapping car rectangles, all slots inside the 4:3 camera extent, footprint rotation); Unity not run per request.

## lot: charging bays and wash slot stations
Date: 2026-11-06
Files: Assets/_Project/Scripts/Lot/{LotGeometry,StationZone,StationBuilder,StationSwitch,StopZone,ZoneRoster,LotConfig,LotLayoutBuilder}.cs, Assets/_Project/Tests/EditMode/Lot/{LotGeometryTests,ZoneRosterTests}.cs
Why: design.md section 10 challenges. Two charging bays and one wash slot are stop zones that publish CarEnteredStation/CarLeftStation; StationSwitch shows them per NightPlanChanged and releases cars (CarLeftStation) when one closes. They take no spot ids. Placement maths moved into pure LotGeometry; stations sit above the curb and the lot bounds grow to contain them.
Tests: LotGeometryTests (no overlaps, inside bounds, walkway) and 3 ZoneRoster tests (drain, destroyed in station once); Unity not run per request.

## lot: fit station labels inside their bays
Date: 2026-11-06
Files: Assets/_Project/Scripts/Lot/{LabelFit,StationBuilder,LotConfig}.cs, Assets/_Project/Tests/EditMode/Lot/LabelFitTests.cs
Why: Web build: the CHARGE labels were wider than their 2.6 bays and ran together. The size is now computed by the pure LabelFit helper from the bay width, a generous per-character width (0.9 x size) and a 70% fill, capped by StationLabelSize (now 0.45). Stations were not moved.
Tests: 3 LabelFitTests (cap, shrink with margin, default config fits CHARGE and WASH); Unity not run per request.

## lot: close spots beyond the night's ActiveSpots
Date: 2026-11-06
Files: Assets/_Project/Scripts/Lot/{OpenSpotRule,SpotCloser,SpotOccupancy,LotGeometry,LotLayoutBuilder}.cs, Assets/_Project/Tests/EditMode/Lot/{SpotClosingTests,LotGeometryTests}.cs
Why: design.md section 10 (spot count per night). SpotCloser listens to NightPlanChanged and calls SpotOccupancy.SetActiveSpots, so spots above ActiveSpots refuse cars (ParkingSpot never publishes CarParked) and get a dark cover; 0 or a later bigger plan reopens them. Spot ids now run column by column from the curb (they ran row by row before), so the farthest columns close first.
Tests: SpotClosingTests (4) and a LotGeometry id-order test; Unity not run per request.

## lot: expanding lot and charging spots
Date: 2026-11-06
Files: Assets/_Project/Scripts/Lot/{LotExpander,LotFrame,LotFrameRule,SpotBuilder,LotGeometry,SpotOccupancy,ParkingSpot,StationBuilder,StationSwitch,RectFactory,LotLayoutBuilder,LotConfig}.cs (SpotCloser, WallBuilder removed), Assets/_Project/Tests/EditMode/Lot/{LotExpansionTests,LotGeometryTests}.cs
Why: user feedback round 5, design.md sections 4/9/10. Closed spots are no longer drawn: LotExpander switches per-spot objects on/off and LotFrame moves the ground and walls to the frame from the pure LotFrameRule / LotGeometry.FrameFor, on every NightPlanChanged and when a car leaves a spot (no per-frame work). A spot holding a car stays open until empty. The dim covers are gone. Charging bays are now two ParkingSpots (ids after the regular ones, blue pad and bolt marker, open only while ChargingOpen, not counted in ActiveSpots, any car); they publish CarParked/CarLeftSpot plus CarEnteredStation/CarLeftStation with StationKind.Charging. The wash slot stays a station. ILotLayout.SpotIds now lists the currently open spot ids (regular and charging).
Tests: LotExpansionTests (5) and LotGeometryTests updated and extended (columns, frame growth, containment); Unity not run.

## lot: implement ILotLayout.IsChargingSpot
Date: 2026-11-07
Files: Assets/_Project/Scripts/Lot/{LotLayoutBuilder,SpotOccupancy}.cs, Assets/_Project/Tests/EditMode/Lot/ChargingSpotTests.cs
Why: Core's ILotLayout gained IsChargingSpot (design.md sections 4/10). LotLayoutBuilder delegates to SpotOccupancy.IsCharging, which now returns false for unknown ids.
Tests: ChargingSpotTests (3: regular false, charging true, unknown false); Unity not run.

## lot: light up wash and charging slots, drop the big ticket box
Date: 2026-11-10
Files: Scripts/Lot/{SlotTint,SlotTintRule,SlotTintPhase,LotConfig,SpotBuilder,StationBuilder,StopZone,ZoneRoster}.cs; Scripts/UI/Hud/{WorldTicketPainter,WorldTicketLayout,PlateLayout,ServiceIcons}.cs; Tests/EditMode/Lot/SlotTintRuleTests.cs; Tests/EditMode/UI/{ServiceIconsTests,HudRulesTests}.cs
Why: Play-test: the world-space info box over cars was too big. The slot itself now lights (aqua wash, lime charge, fading flash when done) and a small droplet/plug icon sits beside the plate (design.md Part A, stations).
Tests: 20 new EditMode tests (tint rule replays, icon rule, plate icon layout); full suite 791 passed, 0 failed.

## lot: show wash and charge icons from car arrival until served
Date: 2026-11-10
Files: Scripts/UI/Hud/{ServiceIcons,WorldTicketPainter}.cs; Tests/EditMode/UI/ServiceIconsTests.cs
Why: Play-test: the needs icons must show as soon as the car has arrived and its patron stepped out, not only inside the station (design.md Part A, stations). Beside the plate; under the car while no plate shows yet.
Tests: 9 ServiceIcons tests (rule and event replays: arrive, carried in, cleaned, charged, ticket closed, no needs); full suite 815 passed, 0 failed.

## lot: paint facing arrows in the pickup zone
Date: 2026-11-10
Files: Scripts/Lot/{CurbArrow,CurbArrowLayout,CurbArrowRule,CurbArrowState,CurbArrowStyle,LotCurbArrows,CurbFacingRule,CurbZone,LotConfig,LotLayoutBuilder}.cs; Tests/EditMode/Lot/CurbArrowTests.cs
Why: Human note: tell the player which way to face the car at VALET PICKUP (design.md A4 delivery rule). Two outlined arrows flank the vertical text, pointing along CurbFacingRule.RequiredDirection; fill is white idle, yellow wrong facing, green right facing (CurbFacingChanged only reports wrong, so CurbZone drives the tint itself).
Tests: 7 new EditMode tests (arrow state rule, required direction, layout); full suite 870 passed, 0 failed.

## lot: judge Perfect Drop-off and Perfect Park
Date: 2026-11-11
Files: Assets/_Project/Scripts/Lot/{PerfectSettings,PerfectTuning,DropOffJudge,ParkJudge,ParkVerdict,PerfectParkGate,CarTicketMap,PerfectRunner}.cs (new), Lot/{LotConfig,LotLayoutBuilder,ParkingSpot,SpotBuilder,CurbZone}.cs, Core/Events/{ParkEvents (new),TuningKey}.cs, Core/Contracts/{PerfectRows (new),TuningCatalog}.cs, Bootstrap/{TuningSeeder,GameBootstrap}.cs (one-line seeding), Tests/EditMode/Lot/PerfectJudgeTests.cs
Why: issues 36 and 16; valet_powers.md Q1 and car_handling_spec.md section 4. PerfectRunner publishes PerfectDropOff when Patrons accepts a delivery (CarDelivered) that was within 15 degrees of down, settled in the curb zone, before half the pickup timer; and PerfectPark (new Core event) when a car rests in a bay within 5 degrees, 0.15 side, 0.3 length, touching nothing, moved 3 units since last rest, 10 s per car. Thresholds live in LotConfig.Perfect and F2 Advanced rows. The +$2 ticket bonus is not wired (TipInput has no bonus field); key PerfectParkBonus carries the amount.
Tests: 31 new EditMode cases; EditMode 1020/1020, PlayMode 16/16.

## lot: widen drive aisle to 6.0 and add east asphalt
Date: 2026-11-10
Files: Assets/_Project/Scripts/Lot/LotConfig.cs, Assets/_Project/Scripts/Lot/SiteLayout.cs, Assets/_Project/Data/Lot/LotConfig.asset, Assets/_Project/Data/Cars/CarHandlingConfig.asset (resynced by setup), Assets/_Project/Tests/EditMode/Lot/LotGeometryTests.cs, Assets/_Project/Tests/EditMode/Integration/HandlingAisleCheckTests.cs, Assets/_Project/Tests/PlayMode/BayEntryTests.cs
Why: human asked for more room to manoeuvre (design.md A5 Lot v2). Aisle 4.6 -> 6.0 (now Range 3..9), StripGap 1.0 -> 3.5; everything else derives from these.
Tests: EditMode 950/950 (2 new geometry tests; shipped-defaults test now compares the shipped assets), PlayMode 14/14 (gap assertion 4.6 -> 6).

## lot: plain-language Perfect rows, loosen Perfect Park [Claude]
Date: 2026-11-11
Files: Assets/_Project/Scripts/Core/Contracts/PerfectRows.cs, Assets/_Project/Scripts/Lot/PerfectSettings.cs, Assets/_Project/Tests/EditMode/Lot/PerfectJudgeTests.cs, Assets/_Project/Tests/EditMode/Integration/DifficultyDialsTests.cs
Why: issue 69. Every Perfect label and tooltip rewritten in designer language with an example number; the three Perfect Park judging rows and the bonus now show without the Advanced toggle. Defaults loosened: angle 5 to 12 degrees, side offset 0.15 to 0.35, lengthwise 0.3 to 0.7 (bay slack is 0.8 side, 1.2 end, so both fit; length slider max lowered 1.5 to 1.2 to match). LotConfig.asset stores no Perfect block, so no asset change.
Tests: new cases (10 deg, 0.3, 0.6 perfect; 14 deg not); catalog visible count 13 to 17. EditMode 1088/1088, PlayMode 16/16.

## lot: exit delivered cars through the top wall onto a one-way top road [Claude]
Date: 2026-11-11
Files: Assets/_Project/Scripts/Core/Contracts/{IRoadLayout,IExitRouter,ICurbFacing}.cs; Scripts/Lot/{CurbFacingRule,DropOffJudge,CurbZone,LotCurbArrows,LotConfig,SiteLayout,LotGeometry,RoadLayout,LotFrame,LotLayoutBuilder,DecorLayout,LotDecor,LotArtConfig,WalkwayStyle,PixelDecal}.cs; new Scripts/Lot/{LotRoadArt,PixelFont}.cs; removed Scripts/Lot/{LotRouteArrows,RouteMarks,RouteMark}.cs; Editor/Art/LotArtBuilder.cs; Data/Lot/LotArtConfig.asset; Tests/EditMode/Lot/{CurbFacingRuleTests,CurbArrowTests,PerfectJudgeTests,RoadLayoutTests,LotGeometryTests,DecorLayoutTests,PixelFontTests}.cs; removed RouteMarksTests.cs; docs/redesign/lot_v2_layout.md
Why: Issue #68, docs/redesign/lot_v2_layout.md "v3: exit from the top". Drop-off now faces up (curb arrows too), delivered cars leave straight up through a gap in the top wall above the curb and merge into a new one-way top road (EXIT); the bottom road is one-way (ENTRY); both labelled with pixel text; the old exit-route arrows are gone. Lot is taller by SiteLayout.ExitRunway. IRoadLayout.WestLane became TopLane.
Tests: new facing-up accept/reject, exit-route-goes-up-through-gap-and-right, road layout and PixelFont tests. RunBatch ran (Bootstrap.unity was pure fileID churn, reverted). EditMode 1095/1095, PlayMode 16/16.

## lot: pickup arrows go green only when the patron will accept the car [Claude]
Date: 2026-11-11
Files: Core/Contracts/IPickupAcceptance.cs (new); Scripts/Lot/{CurbArrowTracker (new),CurbArrowRule,CurbZone}.cs; Scripts/Patrons/{PickupAcceptance (new),PatronSpawner}.cs; Tests/EditMode/Lot/CurbArrowTests.cs
Why: Issue #71. Green now means a correctly facing car whose ticket is PickupRequested (patron waiting; wash or charge not done still counts). Facing right but not asked yet is neutral, wrong facing stays red. CurbZone re-evaluates on PickupRequested so the arrows flip live; Patrons registers IPickupAcceptance in PatronSpawner.
Tests: 4 new EditMode cases (not-accepted idle, wrong stays wrong, live flip then idle on leave, mixed cars). EditMode 1115/1115, PlayMode 16/16.

## traffic: two-way ambient roads, density and merge yielding [Claude]
Date: 2026-11-11
Files: Scripts/Lot/{DecorLayout,LotArtConfig,LotDecor,LotGeometry,LotRoadArt,RoadLayout}.cs; Data/Lot/LotArtConfig.asset; Tests/EditMode/Lot/{DecorLayoutTests,RoadLayoutTests}.cs
Why: Issue #75 (Refs #75), traffic agent. Each road gets a second lane for right-to-left ambient traffic, next to the patron lane and away from the lot: geometry (EastBackLaneY, TopBackLaneY), IRoadLayout lanes, wider asphalt with a dashed centre line (LotArtConfig lane dash fields), sidewalks pushed out. DecorLayout.ViewBottom keeps the camera framing exactly as before (the opposite lanes are partly out of view).
Tests: DecorLayoutTests and RoadLayoutTests updated and extended (opposite lanes, framing unchanged). EditMode 1159/1159, PlayMode 16/16.

## lot: stay starts when a car first rests clear of the queue [Claude]
Date: 2026-11-11
Files: Core: Events/{CarEvents,TicketEvents,TuningKey}.cs, Contracts/{QueueRows,TuningCatalog}.cs. Lot: QueueClearRule, QueueClearRunner, QueueClearSettings, LotConfig, LotLayoutBuilder. Patrons: Ticket, StayStarter (new), TicketRegistry, ServiceTuning, PatronConfig, PatronTuning, LiveTicketTuning. Director: CarRecord, CarBook, LotStateTracker, SnapshotBuilder, ReturnCandidates, LateReturnPlanner (docs). Bootstrap: TuningSeeder, GameBootstrap (seed the new distance). Tests: StayStartTests, QueueClearRuleTests, TicketReturnTests, TicketRegistry*Tests, LotStateTrackerTests, ShiftHarness, Soak* (fake lot).
Why: Issue #78 (Refs #78), closes the loophole left by #77 where a car left in the queue could be handed back fast. The patron stay now starts the first time the car rests (no driver, slow, settle time) at least QueueClearDistance (default 6 units, from the nearest queue slot centre) from the queue, or when it is parked in a bay, or after QueueMaxWaitMinutes (default 3 game minutes) in the queue. New Core events CarClearedQueue (Lot) and StayStarted (Patrons, for the Director); TuningKey QueueClearDistance and QueueMaxWaitMinutes appended at the end; F2 rows in QueueRows (Advanced). A ticket whose stay has not started has no ReturnMinute (infinity), is not summoned and cannot be shifted; the Director sees infinite SecondsToPickup, so it is neither a return candidate nor in the late phase until its stay starts (the wait limit bounds that).
Tests: new StayStartTests (6) and QueueClearRuleTests (5); returns and registry tests now start the stay with CarClearedQueue; soak fake lot starts the stay at placement or the queue limit, all 200 seeds pass. EditMode 1226/1226, PlayMode 17/17 (incl. BootstrapSceneSmokeTests); RunBatch run, no scene change needed (QueueClearRunner is added at runtime by LotLayoutBuilder).

## powers: Park Smash parks waiting cars, pays waiting patrons, with flash, ring, coins and boom [Claude]
Date: 2026-11-11
Files: Lot/SmashRunner.cs, SmashPlan.cs, SmashRoster.cs, ParkingSpot.cs (Place, Release, settle guard), LotLayoutBuilder.cs, tests Lot/SmashPlanTests.cs and PlayMode/ParkSmashPlayModeTests.cs (edited by the powers agent for issue #43).
Why: Park Smash (design.md A11) stands every waiting arrived car in a free bay on PowerActivated(ParkSmash) without a Perfect Park judgement.
Tests: EditMode and PlayMode suites pass.

## powers: shared power bar of three buckets, remove speed boost and Park Smash [Claude]
Date: 2026-11-13
Files: Lot: removed SmashRunner, SmashPlan, SmashRoster; ParkingSpot (Place, Release removed), LotLayoutBuilder.
Why: Park Smash deleted (issue #86 decision 9). issue #86 of epic #79 (Valet to Valhalla).
Tests: SmashPlanTests and the Smash PlayMode tests removed; suites pass.

## services: one model for what a car needs (service modifiers) [Claude]
Date: 2026-11-13
Files: Assets/_Project/Scripts/Lot/SlotTint.cs
Why: Issue #121. The slot tint follows ModifierProgressChanged and ModifierCleared of the modifier its station kind clears (read from the catalog) instead of the four wash and charge events.
Tests: see docs/changelogs/services.md (EditMode and PlayMode all green).

## art: procedural prop toolkit, Midgard and Vanaheim props, lighting presets [Claude]
Date: 2026-11-13
Files: Core/Art (PaletteSlot, WorldPalette, PropShapeKind, PropShape, PropAnimation, PropLayer, PropRecipe, PropRecipeRules, PropPlacement, PointLightSpec, LightingPreset); Lot (ShapeRaster, PropPixels, PropSpriteCache, PropBuilder, PropAnimator, PropMotion, LotLighting, LightFlicker, PixelRecolour, DinerSkin, PixelFont O and P, LotDinerArt, LotDecor, LotLayoutBuilder, UncannyValet.Lot.asmdef); Run (LotDefinition palette, props and lighting fields, LotDefinitionRules); Editor (Art/RecipeKit, ArtAssets, MidgardArt, VanaheimArt, ArtAssetSetup, LotWorldSceneSetup.ApplyArt, ProjectSetup, BootstrapSceneBuilder); Data/Art (2 palettes, 13 recipes, 4 presets); both Midgard lot assets and scenes; tests (EditMode Lot and Run, PlayMode PropArtPlayModeTests, test asmdefs).
Why: issue #116 of epic #79. Every prop is a recipe asset drawn once per lot load on the 32 px grid from a world palette with a 1 px outline and an optional sprite override; lots get a LightingPreset (global light plus up to 16 point lights, no shadows). The diner is re-skinned tan with a green roof by a palette swap of the MetroCity building.
Tests: EditMode 1489 passed (recipe validation, generator and outline rule per shipped recipe, cache, override, builder, lighting cap, recolour, allocation-free ticks, lot art rules); PlayMode 47 passed (new PropArtPlayModeTests plus the lot smoke tests); RunBatch clean.
