# Lot changelog

Entries are added by the lot agent, one per commit. Format in docs/changelogs/README.md.

## lot: build lot layout, spots, curb zone and parked rule
Date: 2026-11-05
Files: Assets/_Project/Scripts/Lot/{LotConfig,LotLayoutBuilder,RectFactory,StopZone,ParkingSpot,CurbZone,StopTimer,SpotOccupancy}.cs (LotModule.cs removed), Assets/_Project/Tests/EditMode/Lot/{SpotOccupancyTests,StopTimerTests}.cs
Why: design.md sections 4, 9, 10: runtime-generated lot, CarParked / CarLeftSpot / CarAtCurb after a car is stopped and empty for 0.5 s.
Tests: 8 EditMode tests added (SpotOccupancy, StopTimer); EditMode run in batch mode.
