# Traffic changelog

## traffic: add road traffic module with car following and patron trips
Date: 2026-11-10
Files: Assets/_Project/Scripts/Traffic/* (new assembly UncannyValet.Traffic), Assets/_Project/Tests/EditMode/Traffic/{CarFollowingTests,LanePathTests,LaneOccupancyTests,MergeGapRuleTests,AmbientSpawnerTests,TripTests}.cs, Assets/_Project/Tests/EditMode/UncannyValet.Tests.EditMode.asmdef (reference added)
Why: docs/redesign/lot_v2_layout.md "Road below the lot" and "Traffic mechanics". Pure rules (lane path, car following, merge gap, ambient spawner, occupancy registry), pooled visual-only ambient cars on both lanes (TrafficModule + TrafficConfig), and entry/exit trips (IEntryRouter, IExitRouter, ITripDriver) for patron cars, built on the Core IRoadLayout contract.
Tests: six EditMode fixtures written; Unity not run in this sandbox, the lead compiles and runs them.

## traffic: keep a turning patron car visible to lane traffic [Claude]
Date: 2026-11-11
Files: Assets/_Project/Scripts/Traffic/{EntryTrip,LanePath,TripContext,TripTuning,TrafficConfig,TrafficModule}.cs, Assets/_Project/Tests/EditMode/Traffic/TurnIntoQueueTests.cs
Why: Issue #3. EntryTrip dropped the car from LaneOccupancy at the turn point, so an ambient car right behind it accelerated and drove through the car while it was still arcing across the lane. The car now stays registered (at its nearest lane distance and speed) until its centre is LaneClearance (new TrafficConfig tunable) from the lane line. See docs/redesign/lot_v2_layout.md "Traffic mechanics".
Tests: new TurnIntoQueueTests (entry turn with real RoadLayout failed before the fix, plus an exit-merge sweep that passes). EditMode 1077/1077 and PlayMode 16/16 pass.

## lot: exit delivered cars through the top wall onto a one-way top road [Claude]
Date: 2026-11-11
Files: Assets/_Project/Scripts/Traffic/{TrafficLane,AmbientSpawner,TrafficModule,TripContext,EntryTrip,ExitTrip,TrafficConfig}.cs, Assets/_Project/Tests/EditMode/Traffic/{AmbientSpawnerTests,LaneOccupancyTests,TripTests,TripObstacleTests,EntryRetargetTests,TurnIntoQueueTests}.cs
Why: Issue #68. TrafficLane.West became Top (the one-way exit road above the lot); ambient cars split evenly over the entry and exit roads, both left to right at the same speed; ExitTrip merges into the top lane and yields to top-road traffic only (TripContext.CanJoin/Join take the lane). EastShare tooltip updated.
Tests: new exit tests (goes up then right onto the top lane, ignores entry-road traffic, yields to top-road traffic), spawner share test. EditMode and PlayMode pass.
