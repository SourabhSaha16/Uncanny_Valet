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

## lot-v4: fewer exit route arrows
Date: 2026-11-10
Files: Scripts/Lot/WalkwayStyle.cs, Data/Lot/LotArtConfig.asset, Tests/EditMode/Lot/RouteMarksTests.cs
Why: human request (design.md A5): the painted exit arrows were too many. Spacing is now 5.5 units (WalkwayStyle field, set in LotArtConfig), a few arrows from just after the curb down the corridor to the turn east and the road.
Tests: RouteMarksTests uses the style defaults and expects 3 to 6 arrows on the real exit route.

## lot-v4: pickup zone moves right, patrons wait in the gap beside the diner
Date: 2026-11-10
Files: Scripts/Lot/{SiteLayout,LotGeometry,LotConfig,PedestrianPathMap,WalkwayStyle}.cs, Data/Lot/{LotConfig,LotArtConfig}.asset, Tests/EditMode/Lot/{LotGeometryTests,PedestrianPathMapTests}.cs, Scripts/UI/Hud/KeyBoardRows.cs.meta
Why: human request (design.md A5): clear gap between the diner wall and the VALET PICKUP zone. New SiteLayout.CurbGap (1.75): the curb centre moves from x -10 to -8.25 while the diner stays where it was (the diner right wall is Curb.xMin - CurbGap), so the doors, the walkway side leg and the main-door route follow the diner wall. The three curb standing spots now sit in the middle of that gap (LotGeometry.CurbGap rect; WalkwayStyle.WaitInset is removed) and face east to the zone. The queue column stays under the curb (it follows the curb x), the exit route, crosswalks, walls and the camera fit (measured from the built renderers) follow the geometry. To keep the exit car clear of the crosswalk and the first bay column, the curb-to-bays corridor is 5.25 (was 6) and ExitShare 0.75 (was 0.8): bays move right by 1.0, so the lot is 1.0 wider overall. Other bay positions relative to each other are unchanged.
Tests: LotGeometryTests (gap between diner and curb, gap inside the frame, exit car clear of crosswalk and bays); PedestrianPathMapTests (slots inside the gap, west of the zone). Full EditMode run: 712 passed, 0 failed. ProjectSetup.RunBatch ran clean; Bootstrap.unity churn reverted, LotConfig and LotArtConfig kept.
