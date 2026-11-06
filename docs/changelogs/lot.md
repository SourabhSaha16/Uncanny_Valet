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
