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
