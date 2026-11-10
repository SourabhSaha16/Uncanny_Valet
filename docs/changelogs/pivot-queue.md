# Pivot queue changelog

Source: the human's play-test notes (lead brief), docs/design.md A2, A4, A5, A10 and docs/redesign/ui_style_guide.md Revision 4.

## pivot-queue: queue of 4, no park timer, single wash slot, plug icons, big arrows
Date: 2026-11-09
Files:
- Core (additive/removal): TimerKind.Park removed; TuningKey.ParkWindowMinutes removed (and its catalog row); PlanMaxWaitingCars range 1-4; new event StationEntryRefused (StationEvents); new PlugIconArt (shared plug bitmap and colours); ShiftConfig default max waiting 4.
- Patrons: Ticket (ParkTimer, SyncParkTimer, MarkAbandoned removed), TicketRegistry (no park tick or abandon), TicketTimerPublisher (pickup only), StationService (no clock), LiveTicketTuning, PatronTuning, PatronConfig (max waiting 4, no park window), PatronRequest (park window parameter removed: constructor is stay, tip, impatient, pickupWindow, isEv, isDirty), PatronRequestRoller.
- Director: CarRecord/CarBook (patience from the pickup timer only), WorkloadEstimator (pressure P uses pickup tickets only; waiting cars still cost park work), DirectorBrain (default queue cap 4), LotStateTracker.
- Audio: PendingCarTracker (no timer-expiry release). Economy: ShiftScore comment.
- Lot: LotConfig (4 queue slots, curb facing tolerance 80, refusal repeat), StationOccupancy (pure), StationBlocker (solid box while the wash slot is held; the holder ignores it, non-cars are let through), StationZone (UseBlocker), StopZone.TryGetCar, StationBuilder (wash blocker), PlugDecal and SpotBuilder (plug under bay numbers, bolt removed), LotArrowMarks (big outlined arrows with chevrons), LotCurbArt, LotLayoutBuilder (up arrows at arrival slots), LotWalkwayArt (ChevronDown removed), WalkwayStyle (mark fields).
- UI: KeyBoardPainter (no bar before pickup), OccupiedHintTracker, OccupiedHintPainter, GameUi, PixelIconData (plug from PlugIconArt).
- Data: PatronConfig.asset, ShiftConfig.asset (max waiting 4, no park window), LotArtConfig.asset (curb arrow length 3.4).
- Docs: ui_style_guide.md Revision 4.
- Tests: park-timer tests rewritten as "unparked car waits forever, no complaint" regressions (TicketRegistryTimerTests, FullShiftTests, ChargeSpotTests, TicketRegistryServiceTests, LiveTuningTests, CarDeliveredTests, TicketPlateTests, ShiftScoreTests, UrgencyTests, TicketBoardTests, HudRulesTests, LotStateTrackerTests, WorkloadEstimatorTests); new StationOccupancyTests, OccupiedHintTests; CurbFacingRuleTests at 80 degrees; LotGeometryTests (4 slots).
Why: Part A2/A4/A5/A10 play-test changes: queue of 4 with any car takeable, no park window (only the pickup timer drives tip and anger), cars never drive away unparked (only delivered cars and shift-end cleanup), one car in the wash slot, plug icon on EV bays, patron accepts any car pointing towards the road, visible curb and arrival arrows.
Tests: NOT run: Unity could not be launched in this sandbox, so compilation and the suite are unverified. New .meta files are not committed.
