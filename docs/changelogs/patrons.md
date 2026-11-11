# Patrons changelog

Entries are added by the patrons agent, one per commit. Format in docs/changelogs/README.md.

## patrons: apply night plan, EV charging, washing and tip breakdown
Date: 2026-11-06
Files: Assets/_Project/Scripts/Patrons/{ServiceProgress,ChargeRule,WashRule,ServiceTuning,IPatronRandom,UnityPatronRandom,PatronPlan,PatronRequestRoller,StationService,TicketSettler,Ticket,TicketBook,TicketRegistry,PatronRequest,PatronConfig,PatronSpawner}.cs, Assets/_Project/Data/Patrons/PatronConfig.asset, Assets/_Project/Tests/EditMode/Patrons/{ServiceRules,TicketRegistryService,TicketRegistryNight,TicketRegistry,TicketRegistryTimer}Tests.cs
Why: design.md section 10. NightPlanChanged sets the waiting cap, arrival gap scale and EV/dirty odds (injectable random). EVs charge and dirty cars wash only while stopped in their bay; any station pauses the park timer. Arrival events are published in the order CarSpawned, TicketOpened, CarLetterAssigned, TicketTraitsAssigned, then initial charge/clean. Delivered tickets use ITipCalculator.Explain and publish TipExplained before TicketClosed; forced closes publish an all-zero breakdown (reason None).
Tests: new EditMode tests for the rules, plan, trait rolls, arrival order, station pause and tip flags; fakes updated for Explain; not run, Unity not run per instruction.

## patrons: close tickets and stop spawning at night end
Date: 2026-11-06
Files: Assets/_Project/Scripts/Patrons/{TicketRegistry,Ticket,TicketBook,LetterPool,PatronSpawner}.cs, Assets/_Project/Tests/EditMode/Patrons/TicketRegistryNightTests.cs
Why: design.md section 10. On NightEnded every open ticket closes with wait 0 and tip 0 (no TipEarned), timers stop, letters free, and PatronDeparture removes the car; the spawner stops until NightStarted, which resets its arrival timer, leftover tickets and letters.
Tests: 4 EditMode tests added (close all, timers stopped, double close safe, two-night clean state with letters from A); not run, Unity not run per instruction.

## patrons: add arrival gate against stacking cars
Date: 2026-11-05
Files: Assets/_Project/Scripts/Patrons/{ArrivalGate,PatronConfig,PatronSpawner,TicketRegistry}.cs, Assets/_Project/Tests/EditMode/Patrons/ArrivalGateTests.cs
Why: Play-test: arriving cars stacked. A car now arrives only if fewer than MaxWaitingCars tickets await parking and the arrival point is clear; otherwise retry next minute (docs/decisions.md).
Tests: 3 ArrivalGate EditMode tests added; not run, Unity not run per instruction.

## patrons: close ticket when car already at curb at pickup
Date: 2026-11-05
Files: Assets/_Project/Scripts/Patrons/{Ticket,TicketRegistry}.cs, Assets/_Project/Tests/EditMode/Patrons/TicketRegistryTests.cs
Why: QA found a ticket stuck if the car waited at the curb before pickup (CarAtCurb fires once). Registry now remembers AtCurb per ticket (cleared on CarEntered), closes on pickup request, iterates a copy of tickets and removes closed ones.
Tests: 3 EditMode tests added (early curb, CarEntered clears flag, open inside handler); not run, Unity not run per instruction.

## patrons: add tickets, registry, spawner and departure
Date: 2026-11-05
Files: Assets/_Project/Scripts/Patrons/{PatronConfig,PatronRequest,Ticket,TicketState,TicketRegistry,PatronSpawner,PatronDeparture}.cs, Assets/_Project/Tests/EditMode/Patrons/TicketRegistryTests.cs
Why: P1 patron loop (design.md sections 5 and 10): arrive, park, stay, pickup request, close with tip, depart.
Tests: 4 EditMode tests for TicketRegistry (whole ticket, early curb ignored, no pickup before park, unknown car); all 7 EditMode tests pass.

## patrons: add arrival queue, car letters and park/pickup timers
Date: 2026-11-06
Files: Assets/_Project/Scripts/Patrons/{ArrivalGate,ArrivalSlotScanner,LetterPool,PatronConfig,PatronRequest,PatronSpawner,PatronDeparture,Ticket,TicketBook,TicketRegistry,TicketTimer,TicketTimerPublisher}.cs, Assets/_Project/Data/Patrons/PatronConfig.asset, Assets/_Project/Tests/EditMode/Patrons/{ArrivalGate,LetterPool,TicketTimer,TicketRegistryTimer,TicketRegistry}Tests.cs
Why: design.md section 10 pressure rules. Up to 3 cars queue in ILotLayout.ArrivalSlots; each ticket gets the lowest free letter; the park timer (45 min, pauses while the valet drives) makes the patron take the car for $0, no penalty; the pickup timer (60 min) zeroes the tip via TipInput.PickupExpired but the car is still owed. PatronSpawner.Spawn is now TrySpawn(request, out ticket).
Tests: new EditMode tests for the gate, letter pool, timer and registry timers (expiry, pause, stop, vanished car frees letter and ticket); one existing registry test adjusted for park expiry. Unity not run.

## patrons: live tuning, late EV surge, charge spots
Date: 2026-11-06
Files: Assets/_Project/Scripts/Patrons/{PatronTuning,LiveTicketTuning,EvChanceCurve,PatronRequestRoller,PatronPlan,PatronSpawner,PatronConfig,StationService,Ticket,TicketBook,TicketRegistry,TicketTimer}.cs, Assets/_Project/Tests/EditMode/Patrons/{ChargeSpotTests,LiveTuningTests,ServiceRulesTests,TicketRegistryServiceTests}.cs
Why: round 5 feedback. Gap, stay, impatience, park and pickup windows, charge and wash minutes are read live from ITuning (config is only a fallback); window changes also reach running timers. EV chance ramps from EvChance to LateEvChance after LateEvStart. Charging bays are parking spots: only EVs charge, and a charging spot no longer pauses the park timer (the wash still does). Plan changes mid-night only overwrite PatronPlan values.
Tests: new ChargeSpotTests and LiveTuningTests; two existing tests adjusted (RollTraits signature, park timer pauses in the wash bay). Unity not run.

## patrons: use SpawnArriving and DriveAwayAndDespawn, reserve arrival slots
Date: 2026-11-06
Files: Assets/_Project/Scripts/Patrons/{ArrivalReservations,PatronSpawner,PatronDeparture}.cs, Assets/_Project/Tests/EditMode/Patrons/ArrivalReservationsTests.cs
Why: cars now drive in and out on their own (design.md section 5). Arrivals call ICarFactory.SpawnArriving; departures call DriveAwayAndDespawn after CarDeparted. A car still driving in is invisible to the physics scan, so each ticket reserves its arrival slot until it leaves AwaitingPark, which stops two cars heading for the same slot.
Tests: new ArrivalReservationsTests (slot stays blocked while arriving, freed on park, departure drives away not despawns). Park timer is unchanged and starts at spawn, so it also counts the drive-in time.

## patrons: publish CarDelivered before TicketClosed on delivery
Date: 2026-11-07
Files: Assets/_Project/Scripts/Patrons/TicketSettler.cs, Assets/_Project/Tests/EditMode/Patrons/CarDeliveredTests.cs
Why: Core now has CarDelivered (patron got the car back at the curb). TicketSettler.Pay, the single delivered close (normal and late pickup), publishes it right before TicketClosed; Void (vanished car, never parked, night end) does not.
Tests: new CarDeliveredTests (normal and late pickup publish CarDelivered then TicketClosed; park-timer vanish and night end publish TicketClosed only). Unity run: see report.

## patrons: fix stalled arrivals, queue slot re-evaluation, late stays
Date: 2026-11-10
Files: Assets/_Project/Scripts/Core/Contracts/IQueueSlotSource.cs, Assets/_Project/Scripts/Director/ArrivalPacer.cs, Assets/_Project/Scripts/Director/PacingSettings.cs, Assets/_Project/Scripts/Patrons/{ArrivalReservations,ArrivalSlotScanner,IArrivalScan,QueueSlotAllocator,QueueSlotChooser,StayFit,PatronRequest,PatronConfig,PatronTuning,PatronSpawner,TicketRegistry}.cs, Assets/_Project/Scripts/Traffic/{EntryTrip,RouteRunner,TripContext,TripTuning}.cs, Assets/_Project/Scripts/Cars/CarAutoDrive.cs, Assets/_Project/Tests/EditMode/{Director/ArrivalPacerStuckTests,Patrons/QueueSlotChooserTests,Patrons/QueueSlotAllocatorTests,Patrons/StayFitTests,Patrons/ArrivalReservationsTests,Traffic/EntryRetargetTests}.cs
Why: Bug 1: a Spawn decision the spawner could not execute left the pacer waiting for a CarSpawned forever (overlay "next in 0.0 s", RELIEF stuck); it now times out and re-sends. Late arrivals could get a stay running past the shift end, so their patron never came back; stays are now cut to fit (StayFit). The slot scan ignores trigger colliders. Bug 2: arrival slots were fixed at spawn and reservations lived until the car parked, so a freed slot stayed "full"; reservations now end on CarArrived and arriving cars re-ask IQueueSlotSource (QueueSlotAllocator) and move up to the first free slot. design.md sections 5, 9, 10.
Tests: new QueueSlotChooser, QueueSlotAllocator, StayFit, ArrivalPacerStuck and EntryRetarget tests; ArrivalReservations tests rewritten. Unity run: full EditMode suite 772/772 passed.

## patrons: nine difficulty dials, Advanced toggle, plain-language director rows
Date: 2026-11-10
Files: Assets/_Project/Scripts/Core/Events/TuningKey.cs, Assets/_Project/Scripts/Core/Contracts/{TuningInfo,TuningCatalog,DifficultyRows,GameRows,DirectorPacingRows,DirectorMixRows,DirectorPlayerRows}.cs, Assets/_Project/Scripts/Director/{DialLink,DialEffect,DialAnchors,DialDilemmaAnchors,DialPlayerAnchors,DifficultyDialsConfig,DifficultyDials,DifficultyDialsLink,ShapeSettings,TunedIntensityProfile,DirectorConfig,DirectorProfile,DirectorSettings,PatronDirector}.cs, Assets/_Project/Scripts/Bootstrap/{TuningSeeder,GameBootstrap}.cs, Assets/_Project/Scripts/UI/{TuningPanel,TuningRowView,DirectorOverlay,DirectorOverlayText}.cs, Assets/_Project/Editor/ProjectSetup.cs, Assets/_Project/Data/Director/{DifficultyDialsConfig,DirectorConfig}.asset, regenerated Bootstrap scene, Assets/_Project/Tests/EditMode/{Integration/DialRig,Integration/DifficultyDialsTests,Director/TunedIntensityProfileTests}.cs, docs/redesign/difficulty_dials.md
Why: The human wants fewer tunables. Eight new dials (plus the existing seed) drive about 70 underlying director, plan and mix keys through piecewise-linear anchors, and the default position reproduces every authored value exactly. Everything but the dials and the clock rows sits behind an Advanced toggle in F2; director rows were renamed into plain designer language and regrouped (Pacing, Car mix, Dilemmas, Mercy, Skill reward, Workload costs). Seven director values that had no tuning key (target curve scale, ramp shape, late boost, four base weights) became keys so dials can reach them; the director reads them through TunedIntensityProfile. design.md A10.
Tests: new DifficultyDialsTests (defaults reproduce all values, round trips, monotone direction per dial, no feedback loop, hand-edit rule, catalog layout) and TunedIntensityProfileTests. Unity run: see the report.

## patrons: adopt the hand-tuned dial baseline as the new defaults
Date: 2026-11-10
Files: Assets/_Project/Scripts/Director/{DialStart,DifficultyDialsConfig,DifficultyDials,DifficultyDialsLink}.cs, Assets/_Project/Tests/EditMode/Integration/{DialRig,DifficultyDialsTests}.cs, docs/redesign/difficulty_dials.md
Why: The human tuned the F2 panel (Intensity 8, Ramp 8, Late Rush 6, Dirty 5, EV 5, Dilemmas 6.5, Mercy 4, Skill 5.5, Seed 0) and wants it as the baseline. Dials now seed at configurable start positions and the derived keys are seeded at their derived values, so Reset and Reset all return to the baseline. Clock stays 1 min/s and 240 s (already the config values).
Tests: anchor-default test kept (dials moved to 5 reproduce authored values); new tests for the seeded baseline and in-range derived keys, Reset semantics, and the shipped asset carrying the baseline. Full EditMode suite 953/953 passed.

## patrons: lower pickup patience, impatient patrons wait less, visible tuning
Date: 2026-11-10
Files: Scripts/Patrons/{PatronRequest,PatronConfig,PatronTuning,PatronRequestRoller,LiveTicketTuning}.cs, Data/Patrons/PatronConfig.asset, Scripts/Core/Events/TuningKey.cs, Scripts/Core/Contracts/GameRows.cs, Scripts/Bootstrap/TuningSeeder.cs, Tests/EditMode/Patrons/LiveTuningTests.cs, Tests/EditMode/Integration/{DifficultyDialsTests,PhysicsAndTipTests}.cs
Why: Play-test: patrons should lose patience sooner (design.md A4). Default window 60 -> 45 game min; impatient patrons wait 0.6x of it (was a tip flag only). New key ImpatientWindowScale; both rows visible in the Clock block; changes reach waiting tickets.
Tests: 4 new in LiveTuningTests; dial-catalog visible count 11 -> 13; PhysicsAndTipTests pins a long window so complaint penalties do not skew its sum check. Full EditMode passes (978).

## patrons: pay the Perfect Park bonus on the ticket's tip
Date: 2026-11-11
Files: Scripts/Patrons/{PerfectBonusLedger,TicketSettler,TicketRegistry}.cs, Scripts/Economy/{TipCalculator,TipConfig}.cs, Scripts/Core/Contracts/TipInput.cs, Scripts/Core/Events/TipBreakdown.cs, Tests/EditMode/Patrons/PerfectBonusTests.cs
Why: Issue #16 (money part). PerfectBonusLedger counts PerfectPark events per ticket, the settler takes the count on payout and clears it on void; the calculator adds count x PerfectParkBonus (live tuning, default $2) to the subtotal so damage and combo apply, and TipBreakdown.PerfectBonus shows it.
Tests: 6 new in PerfectBonusTests (bonus, combo, per-ticket survival to pickup, one per event, ticketless/void, zero tip). Full EditMode 1061/1061 passed.

## patrons: stay runs from drop-off, patron asks wherever the car is [Claude]
Date: 2026-11-11
Files: Scripts/Patrons/{Ticket,CurbCall,TicketRegistry}.cs, Scripts/Director/{CarBook,CarRecord,LotStateTracker,SnapshotBuilder,WorkloadEstimator}.cs, Tests/EditMode/Patrons/{TicketReturnTests,TicketRegistryTests,TicketRegistryServiceTests,TicketRegistryTimerTests}.cs, Tests/EditMode/Director/{LotStateTrackerTests,SoakSim}.cs, Tests/EditMode/Integration/FullShiftTests.cs
Why: Issue #77. The stay clock started only on CarParked (numbered bays), so cars left in the queue, wash or aisle never got their patron back. Now the clock starts at ticket creation and restarts once at CarArrived (drop-off); StayEnded and MarkPickupRequested work from AwaitingPark and Parked (wash and charge have no state of their own); CurbCall.AcceptsArrival accepts any pre-pickup state. Director return time is arrival minute + stay (CarRecord.ArrivedAtMinute) and the "pickup soon" cost applies to any dropped-off car.
Tests: 10 new in TicketReturnTests (every state reaches PickupRequested, registry with the real EventBus, delivery from queue and station, no stuck tickets); 6 old tests updated to the new clock; SoakSim stay now runs from arrival. EditMode all pass, PlayMode 16/16.

## patrons: unwashed dirty car still tips, asked-for car joins key board [Claude]
Date: 2026-11-11
Files: Scripts/Economy/TipCalculator.cs, Scripts/Core/Events/TipZeroReason.cs, Scripts/UI/TicketBoard.cs, Tests/EditMode/Economy/TipCalculatorTests.cs, Tests/EditMode/UI/TicketBoardTests.cs
Why: Issue #77 owner decisions. An unwashed dirty car no longer zeroes the tip: it pays base and speed and just misses the clean bonus (the NotCleaned zero reason is removed, the breakdown shows CleanBonus 0). A car whose patron asked is marked Placed on PickupRequested, so it is listed on the key board and shows its plate even while still in the queue.
Tests: replaced Explain_UncleanedZeroes with a no-clean-bonus test; new KeyBoard_ListsAnUnplacedCarAsSoonAsItsPatronAsks. EditMode all pass, PlayMode 16/16.

## lot: stay starts when a car first rests clear of the queue [Claude]
Date: 2026-11-11
Files: Core: Events/{CarEvents,TicketEvents,TuningKey}.cs, Contracts/{QueueRows,TuningCatalog}.cs. Lot: QueueClearRule, QueueClearRunner, QueueClearSettings, LotConfig, LotLayoutBuilder. Patrons: Ticket, StayStarter (new), TicketRegistry, ServiceTuning, PatronConfig, PatronTuning, LiveTicketTuning. Director: CarRecord, CarBook, LotStateTracker, SnapshotBuilder, ReturnCandidates, LateReturnPlanner (docs). Bootstrap: TuningSeeder, GameBootstrap (seed the new distance). Tests: StayStartTests, QueueClearRuleTests, TicketReturnTests, TicketRegistry*Tests, LotStateTrackerTests, ShiftHarness, Soak* (fake lot).
Why: Issue #78 (Refs #78), closes the loophole left by #77 where a car left in the queue could be handed back fast. The patron stay now starts the first time the car rests (no driver, slow, settle time) at least QueueClearDistance (default 6 units, from the nearest queue slot centre) from the queue, or when it is parked in a bay, or after QueueMaxWaitMinutes (default 3 game minutes) in the queue. New Core events CarClearedQueue (Lot) and StayStarted (Patrons, for the Director); TuningKey QueueClearDistance and QueueMaxWaitMinutes appended at the end; F2 rows in QueueRows (Advanced). A ticket whose stay has not started has no ReturnMinute (infinity), is not summoned and cannot be shifted; the Director sees infinite SecondsToPickup, so it is neither a return candidate nor in the late phase until its stay starts (the wait limit bounds that).
Tests: new StayStartTests (6) and QueueClearRuleTests (5); returns and registry tests now start the stay with CarClearedQueue; soak fake lot starts the stay at placement or the queue limit, all 200 seeds pass. EditMode 1226/1226, PlayMode 17/17 (incl. BootstrapSceneSmokeTests); RunBatch run, no scene change needed (QueueClearRunner is added at runtime by LotLayoutBuilder).
