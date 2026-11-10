# lot-v4 changelog

## lot-v4: drop cars returned from the results screen
Date: 2026-11-10
Files: Scripts/UI/{ShiftStats,GameUi}.cs, Scripts/UI/Screens/ResultsFormatter.cs, Tests/EditMode/UI/ResultsScreenTests.cs
Why: human request (design.md A5 results screen): the CARS RETURNED stat and the counter that only fed it are removed; the other stats stay.
Tests: ResultsScreenTests updated (three stat lines, ShiftStats without the returned counter). Run in the full EditMode pass at the end of the series.

## lot-v4: traffic defaults from the F2 tuning
Date: 2026-11-10
Files: Scripts/Traffic/{TrafficConfig,TrafficModule}.cs, Scripts/Bootstrap/TuningSeeder.cs, Data/Traffic/TrafficConfig.asset, Tests/EditMode/Integration/TuningSeederTests.cs
Why: human tuned traffic in F2 (design.md A5): ambient 13 cars per minute, ambient speed scale 1.25, entry car speed 7.3, exit car speed 5 (units per second, as the panel shows). New TrafficConfig fields EntrySpeed and ExitSpeed hold the start values; TuningSeeder seeds the keys from them and TrafficModule applies them at build. CruiseSpeed and RouteSpeed stay the scale-1 references (and still drive ambient cars), so the panel value is the real speed in units per second.
Tests: TuningSeederTests asserts the four defaults.

## lot-v4: waiting patrons always face the pickup zone
Date: 2026-11-10
Files: Scripts/Pedestrians/{WalkAnimMath,WalkerRouter,PatronWalkers}.cs, Tests/EditMode/Pedestrians/WalkAnimMathTests.cs
Why: human play test (design.md A5, image 6): some patrons at the curb faced down. Root cause: they turned toward the zone centre point and FacingToward lets the dominant axis win, so a patron standing well above or below the (tall) zone centre faced up or down along the curb instead of across to it. Now the look point is the centre x at the patron own height (WalkAnimMath.CurbLookPoint), so they face straight across, and the facing is set in WalkerRouter.Apply for the Waiting phase, so any re-apply of that phase restores it, not only the arrival moment.
Tests: WalkAnimMathTests.CurbLookPoint_FacesAcrossToTheZoneHoweverFarUpOrDownItsCentreIs (regression; shows the old rule gave Up).

## lot-v4: key board lists a car only once it is parked
Date: 2026-11-10
Files: Scripts/UI/{TicketRow,TicketBoard,GameUi}.cs, Scripts/UI/Hud/{KeyBoardRows,KeyBoardPainter}.cs, Tests/EditMode/UI/TicketBoardTests.cs
Why: human request (design.md A5 key board): a car in the arrival queue or being driven no longer shows on the key board; it appears when CarParked (spot) or CarEnteredStation (wash or charger) fires and stays until the ticket closes (delivered). TicketBoard keeps a Placed flag per row (also for a car parked before its ticket opens); the pure KeyBoardRows rule filters the rows for the key board only, other painters still see every ticket.
Tests: TicketBoardTests (not listed on arrival, listed when parked, stays through leaving the spot and the pickup, gone on close, station parking, parked before ticket).

## lot-v4: key board panel renamed CAR SLOTS
Date: 2026-11-10
Files: Scripts/UI/Hud/KeyBoardPainter.cs
Why: human request (design.md A5): the bottom-left panel title KEY BOARD becomes CAR SLOTS. Type and file names are unchanged. The style guide mention is for the lead.
Tests: none (a title string); full EditMode run at the end of the series.
