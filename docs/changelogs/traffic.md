# Traffic changelog

## traffic: add road traffic module with car following and patron trips
Date: 2026-11-10
Files: Assets/_Project/Scripts/Traffic/* (new assembly UncannyValet.Traffic), Assets/_Project/Tests/EditMode/Traffic/{CarFollowingTests,LanePathTests,LaneOccupancyTests,MergeGapRuleTests,AmbientSpawnerTests,TripTests}.cs, Assets/_Project/Tests/EditMode/UncannyValet.Tests.EditMode.asmdef (reference added)
Why: docs/redesign/lot_v2_layout.md "Road below the lot" and "Traffic mechanics". Pure rules (lane path, car following, merge gap, ambient spawner, occupancy registry), pooled visual-only ambient cars on both lanes (TrafficModule + TrafficConfig), and entry/exit trips (IEntryRouter, IExitRouter, ITripDriver) for patron cars, built on the Core IRoadLayout contract.
Tests: six EditMode fixtures written; Unity not run in this sandbox, the lead compiles and runs them.
