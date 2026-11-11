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

## traffic: two-way ambient roads, density and merge yielding [Claude]
Date: 2026-11-11
Files: Scripts/Traffic/{AmbientDensity,AmbientTraffic,LaneNetwork,LaneSet,SpawnGate,TrafficLanes,ViewWindow,YieldZone}.cs (new); Scripts/Traffic/{AmbientVehicle,ExitTrip,LaneMotion,LaneOccupancy,MergeGapRule,TrafficConfig,TrafficLane,TrafficModule,TripContext,TripTuning}.cs; removed Scripts/Traffic/AmbientSpawner.cs; Core/Contracts/{IRoadLayout,GameRows}.cs; Core/Events/TuningKey.cs; Bootstrap/{TrafficTuningLink,TuningSeeder,GameBootstrap}.cs (traffic wiring only); Data/Traffic/TrafficConfig.asset; Tests/EditMode/Traffic/{AmbientDensityTests,AmbientTrafficTests,PatronAmongAmbientTests,SpawnGateTests,ViewWindowTests,YieldZoneTests}.cs (new), {EntryRetargetTests,TripObstacleTests,TripTests}.cs, Tests/EditMode/Integration/{TrafficTuningLinkTests,TuningSeederTests}.cs; removed AmbientSpawnerTests.cs
Why: Issue #75 (Refs #75). Both roads now carry ambient cars both ways (two new right-to-left lanes, TrafficLane.EastBack/TopBack; the patron lanes and routes are unchanged). The random-rate spawner is replaced by AmbientDensity: 3 to 4 ambient cars on screen (counted against the camera's visible rectangle, ViewWindow, cached camera), spawned just beyond the screen edge they drive in from, only into a free gap (SpawnGate), removed once off screen, random speeds, gaps, looks and lanes. F2 rows: fewest and target cars on screen replace cars per minute (TuningKey.TrafficAmbientPerMinute removed, TrafficMinOnScreen and TrafficTargetOnScreen appended); speed scale key unchanged. A delivered car waiting to merge asks the exit lane to yield (YieldZone, a virtual stopped car one merge gap before the merge point): ambient cars that can still stop brake smoothly through the existing car-following, so the gap opens.
Tests: new density, spawn gate, view window, yield zone (dense stream merges in under 12 s with yielding, never without; no overlap), ambient system (3 to 4 on screen for 10 minutes, all four lanes used, directions, off-screen spawn and despawn, no overlap) and patrons among full traffic tests. RunBatch ran (Bootstrap.unity fileID churn reverted). EditMode 1159/1159 and PlayMode 16/16 pass.
