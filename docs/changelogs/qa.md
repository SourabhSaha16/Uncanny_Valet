# QA changelog

Entries are added by the qa agent, one per commit. Format in docs/changelogs/README.md.

## qa: add cross-module integration tests
Date: 2026-11-05
Files: Assets/_Project/Tests/EditMode/Integration/{FakeCarFactory,EventLog,NightHarness,FullNightTests,EventContractTests,PhysicsAndTipTests}.cs
Why: design.md sections 9 and 10: real EventBus, NightClock, TicketRegistry, TipCalculator, DebtLedger, SpotOccupancy, StopTimer, NearestPicker, ValetState and CarPhysicsMath wired together; covers a whole night, event order, exactly-once publishing and tips never below $0.
Tests: 19 EditMode tests added. NOT RUN: the batch-mode Unity launch was refused by the agent sandbox, so results are unverified and .meta files were not generated (Unity creates them on next open).
