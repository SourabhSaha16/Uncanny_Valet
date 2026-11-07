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
