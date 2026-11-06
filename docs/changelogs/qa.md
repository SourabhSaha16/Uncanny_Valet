# QA changelog

Entries are added by the qa agent, one per commit. Format in docs/changelogs/README.md.

## qa: add cross-module integration tests
Date: 2026-11-05
Files: Assets/_Project/Tests/EditMode/Integration/{FakeCarFactory,EventLog,NightHarness,FullNightTests,EventContractTests,PhysicsAndTipTests}.cs
Why: design.md sections 9 and 10: real EventBus, NightClock, TicketRegistry, TipCalculator, DebtLedger, SpotOccupancy, StopTimer, NearestPicker, ValetState and CarPhysicsMath wired together; covers a whole night, event order, exactly-once publishing and tips never below $0.
Tests: 19 EditMode tests added. NOT RUN: the batch-mode Unity launch was refused by the agent sandbox, so results are unverified and .meta files were not generated (Unity creates them on next open).

## qa: add lot-count balance simulation
Date: 2026-11-06
Files: tools/sim/{params,rules,geometry,policies,car,valet,tasks,night,stats,grid,runs,schedule,lot_sim,selftest}.py, tools/sim/.gitignore, tools/sim/results/{report.md,grid.csv}, docs/balance.md
Why: design.md section 10 (pressure rules, night length, challenge schedule): a seeded Monte Carlo of one night with one valet to find how many spots, cars and what arrival pace keep nights 1-7 moderately challenging, and whether the $1500 debt is reachable. Analysis only; no game code, scene or Data asset changed.
Tests: python tools/sim/selftest.py passes (tip rules mirror TipCalculatorTests, LotConfig geometry, motion maths, seeded reproducibility and bookkeeping invariants). Full run (500 nights per cell, 441 grid cells plus schedule search) completed with seed 20261106.
