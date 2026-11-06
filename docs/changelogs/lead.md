# Lead changelog

## lead: add Uncanny Valet starter kit
Date: 2026-11-05
Files: CLAUDE.md, docs/design.md, docs/changelogs/*, Assets/_Project/Scripts/Core/*, module asmdefs and
marker files, Assets/_Project/Editor/WebBuild.cs, Assets/_Project/Tests/EditMode/*, .github/workflows/ci-cd.yml,
tools/*, .claude/*
Why: Project skeleton, Core contracts and CI/CD so module agents can work in parallel (design.md sections 9-10).
Tests: EventBusTests (3 tests) to be run in P0.

## lead: add Unity 6000.2.5f1 project and git repo
Date: 2026-11-05
Files: ProjectSettings/*, Packages/*, Assets/Scenes/*, Assets/Settings/*, Assets/DefaultVolumeProfile.asset,
Assets/InputSystem_Actions.inputactions, Assets/UniversalRenderPipelineGlobalSettings.asset, .vsconfig,
.claude/settings.json, generated .meta files, docs/changelogs/lead.md
Why: Create the Unity project (2D URP template, Unity 6000.2.5f1, product name Uncanny Valet) so CI can build the Web player (design.md section 1, 10).
Tests: EditMode run in batch mode: 3 of 3 passed (EventBusTests).

## lead: switch to local Web build published from site/
Date: 2026-11-05
Files: .github/workflows/ci-cd.yml, tools/build-web.ps1, .gitattributes, docs/changelogs/lead.md
Why: Unity CI needs a license tied to a password login, which the project owner does not have. The Web player is now built locally and the committed site/ folder is published to GitHub Pages (design.md section 1, Platform).
Tests: Not run; workflow and script not yet exercised. Needs the WebGL Build Support module before the first build.

## lead: add first Web build in site/
Date: 2026-11-05
Files: site/*, ProjectSettings/URPProjectSettings.asset, ProjectSettings/PackageManagerSettings.asset, docs/changelogs/lead.md
Why: First Web (WebGL) build of the empty SampleScene, built locally with tools/build-web.ps1 and published from site/ to GitHub Pages (design.md section 1, Platform). Editor-written settings files from opening the project are included.
Tests: Web player served locally and loaded in a browser with no console errors; EditMode tests passed earlier (3 of 3).

## lead: update docs for the local Web build route
Date: 2026-11-05
Files: README.md, CLAUDE.md, KICKOFF.md, docs/design.md, docs/decisions.md, docs/status.md, .claude/agents/integration.md, docs/changelogs/lead.md
Why: Docs still described CI building the Web player with Unity secrets. They now describe the local build into site/ and the Unity 6000.2.5f1 decision (design.md section 1).
Tests: Not applicable (docs only).

## lead: start P1 module work
Date: 2026-11-05
Files: docs/status.md, docs/changelogs/lead.md
Why: P0 is done (EventBusTests 3 of 3 in batch mode), so the five P1 module agents start (CLAUDE.md, Phases).
Tests: EventBusTests run earlier: 3 of 3 passed.

## lead: merge agent/cars
Date: 2026-11-05
Files: merged branch agent/cars (see docs/changelogs/cars.md)
Why: P1 module work for cars merged into main after lead review of folder ownership (CLAUDE.md, Phases).
Tests: Agent could not run tests; run on merged main.

## lead: merge agent/valet
Date: 2026-11-05
Files: merged branch agent/valet (see docs/changelogs/valet.md)
Why: P1 module work for valet merged into main after lead review of folder ownership (CLAUDE.md, Phases).
Tests: Agent could not run tests; run on merged main.

## lead: merge agent/lot
Date: 2026-11-05
Files: merged branch agent/lot (see docs/changelogs/lot.md)
Why: P1 module work for lot merged into main after lead review of folder ownership (CLAUDE.md, Phases).
Tests: Agent reported 11 of 11 (8 new); re-run on merged main.

## lead: merge agent/patrons
Date: 2026-11-05
Files: merged branch worktree-agent-a9b213294782d4e64 (see docs/changelogs/patrons.md)
Why: P1 module work for patrons merged into main after lead review of folder ownership (CLAUDE.md, Phases).
Tests: Agent reported 7 of 7 (4 new); re-run on merged main.

## lead: merge agent/economy-night
Date: 2026-11-05
Files: merged branch worktree-agent-a04e0f3931e473e2f (see docs/changelogs/economy-night.md)
Why: P1 module work for economy-night merged into main after lead review of folder ownership (CLAUDE.md, Phases).
Tests: Agent reported 27 of 27 (24 new); re-run on merged main.

## lead: stop tracking agent worktrees
Date: 2026-11-05
Files: .gitignore, docs/changelogs/lead.md (removed .claude/worktrees/* from the index)
Why: The cars merge commit accidentally recorded the agents' worktree folders as embedded repos; they are local scratch space.
Tests: Not applicable.

## lead: add Unity-generated meta files for P1 modules
Date: 2026-11-05
Files: Assets/_Project/**/*.meta (Cars, Valet, Data, Tests folders and files)
Why: Unity generates .meta files on first import; the cars and valet agents could not run Unity, so these were missing.
Tests: Full EditMode run on merged main: 52 of 52 passed, no compile errors.

## lead: record P1 merge status
Date: 2026-11-05
Files: docs/status.md
Why: Status board shows the five P1 modules merged and lists what integration must wire (design.md section 10).
Tests: Not applicable (docs only).

## lead: merge agent/valet
Date: 2026-11-05
Files: Assets/_Project/Scripts/Valet/SeatController.cs, docs/changelogs/valet.md
Why: Valet fix so CarExited is published once, by CarDriver.SetDriver(false), not twice (design.md section 9, events).
Tests: Full EditMode suite re-run after this merge (see next entry).

## lead: merge agent/integration
Date: 2026-11-05
Files: merged branch worktree-agent-a99b776c9a31c40de (see docs/changelogs/integration.md)
Why: P2 integration code (GameBootstrap, DebugOverlay, CameraFollow, scene and config builders) merged after folder-ownership review; the agent could not launch Unity, so the lead generates the assets (design.md section 10).
Tests: Not run yet at merge time; compile and EditMode run follow.

## lead: generate Bootstrap scene and config assets
Date: 2026-11-05
Files: Assets/_Project/Scenes/Bootstrap.unity, Assets/_Project/Data/Lot/LotConfig.asset, Assets/_Project/Data/Patrons/PatronConfig.asset, ProjectSettings/EditorBuildSettings.asset, Assets/_Project/Editor/BootstrapSceneBuilder.cs, .meta files, docs/changelogs/lead.md
Why: The integration agent could not launch Unity, so the lead ran ProjectSetup.RunBatch. The scene builder failed because Assets/_Project/Scenes did not exist; the lead added Directory.CreateDirectory before SaveScene and set the real scene GUID in Build Settings (design.md section 10).
Tests: Full EditMode run after the generated assets: 52 of 52 passed, no compile errors. Play not yet verified.

## lead: merge agent/qa
Date: 2026-11-05
Files: Assets/_Project/Tests/EditMode/Integration/*, docs/changelogs/qa.md, generated .meta files
Why: Cross-module tests (full fake night, event contracts, tip and physics rules) merged; qa could not run Unity, so the lead ran the suite (design.md section 10).
Tests: Full EditMode run on merged main: 71 of 71 passed (52 before, 19 new from qa), no compile errors.

## lead: merge valet fix
Date: 2026-11-05
Files: merged branch agent/valet (see docs/changelogs/valet.md)
Why: QA review finding for valet fixed by its owner and merged (design.md section 10).
Tests: Full EditMode suite run after all three fix merges (see next entry).

## lead: merge lot fix
Date: 2026-11-05
Files: merged branch agent/lot (see docs/changelogs/lot.md)
Why: QA review finding for lot fixed by its owner and merged (design.md section 10).
Tests: Full EditMode suite run after all three fix merges (see next entry).

## lead: merge patrons fix
Date: 2026-11-05
Files: merged branch worktree-agent-a9b213294782d4e64 (see docs/changelogs/patrons.md)
Why: QA review finding for patrons fixed by its owner and merged (design.md section 10).
Tests: Full EditMode suite run after all three fix merges (see next entry).

## lead: publish first playable prototype build
Date: 2026-11-05
Files: site/*, ProjectSettings/ProjectSettings.asset, ProjectSettings/UnityConnectSettings.asset, Assets/_Project/Scripts/Valet/FrameGuard.cs.meta, docs/changelogs/lead.md
Why: Web build of the P1 prototype loop (Bootstrap scene, lot, patrons, valet, cars, economy) rebuilt from merged main after the qa fixes; publishes to GitHub Pages (design.md section 10). Settings files changed because the build switched the active target to WebGL and turned analytics off.
Tests: Full EditMode run on merged main: 75 of 75 passed. Web build loaded locally with no exceptions; the full park-and-return loop has not been played end to end.

## lead: record play-test round 1 and assign tasks
Date: 2026-11-05
Files: docs/decisions.md, docs/status.md, docs/changelogs/lead.md
Why: Human play-test notes (stacking cars, numbered spots) turned into decisions and tasks (design.md sections 8 and 10).
Tests: Not applicable (docs only).

## lead: merge patrons arrival gate
Date: 2026-11-05
Files: merged branch worktree-agent-a9b213294782d4e64 (see docs/changelogs/patrons.md)
Why: Play-test round 1: arrival gate (design.md section 10; decisions.md 2026-11-05).
Tests: Full EditMode suite run after all four merges (see next entry).

## lead: merge lot spot numbers
Date: 2026-11-05
Files: merged branch agent/lot (see docs/changelogs/lot.md)
Why: Play-test round 1: spot numbers (design.md section 10; decisions.md 2026-11-05).
Tests: Full EditMode suite run after all four merges (see next entry).

## lead: merge cars car numbers
Date: 2026-11-05
Files: merged branch agent/cars (see docs/changelogs/cars.md)
Why: Play-test round 1: car numbers (design.md section 10; decisions.md 2026-11-05).
Tests: Full EditMode suite run after all four merges (see next entry).

## lead: merge integration overlay spot column
Date: 2026-11-05
Files: merged branch worktree-agent-a99b776c9a31c40de (see docs/changelogs/integration.md)
Why: Play-test round 1: overlay spot column (design.md section 10; decisions.md 2026-11-05).
Tests: Full EditMode suite run after all four merges (see next entry).

## lead: merge cars car label size
Date: 2026-11-05
Files: merged branch agent/cars (see docs/changelogs/cars.md)
Why: Play-test round 1 visual fix: car label size (decisions.md 2026-11-05).
Tests: Full EditMode suite run after both merges (see next entry).

## lead: merge integration overlay fit
Date: 2026-11-05
Files: merged branch worktree-agent-a99b776c9a31c40de (see docs/changelogs/integration.md)
Why: Play-test round 1 visual fix: overlay fit (decisions.md 2026-11-05).
Tests: Full EditMode suite run after both merges (see next entry).

## lead: publish prototype build with numbered spots and arrival gate
Date: 2026-11-05
Files: site/*, docs/changelogs/lead.md
Why: Web build after play-test round 1: arrival gate against stacking cars, numbered spots, car numbers, overlay shows spot, label and box sizes fixed (design.md section 10; decisions.md 2026-11-05).
Tests: Full EditMode run on merged main: 81 of 81 passed. Web build loaded locally: spots 1-10 and CURB labelled, car number fits the car, overlay fits its text, one car waiting at the arrival point instead of stacking. Full park-and-return loop not played end to end.

## lead: log missing changelog entries for two meta commits
Date: 2026-11-05
Files: docs/changelogs/lead.md
Why: Commits a5d3a17 (lead: add meta files for round 1 scripts) and 2a49481 (lead: add meta files for label fixes) added only Unity-generated .meta files and were pushed without a changelog entry, which fails tools/check-changelog.sh for that push. History is not rewritten; this entry records them (CLAUDE.md, Changelog rule).
Tests: Full EditMode suite 81 of 81 passed on the merged code those meta files belong to.

## lead: add round-2 contracts and engineering standards
Date: 2026-11-06
Files: CLAUDE.md, docs/design.md, docs/decisions.md, docs/status.md, Assets/_Project/Scripts/Core/Contracts/ILotLayout.cs, Assets/_Project/Scripts/Core/Contracts/TipInput.cs, Assets/_Project/Scripts/Core/Events/CarEvents.cs, Assets/_Project/Scripts/Core/Events/TicketEvents.cs, Assets/_Project/Scripts/Core/Events/TimerKind.cs
Why: Play-test round 2 needs a queue of 3, park and pickup timers and car letters (design.md section 10, Pressure rules). CLAUDE.md now holds Unity and software engineering standards for every agent.
Tests: None run. ILotLayout gains ArrivalSlots, so main does not compile until the lot agent implements it.

## lead: merge round-2 agent branches and add generated meta files
Date: 2026-11-06
Files: Assets/_Project/Scripts/Core/Events/TimerKind.cs.meta, Assets/_Project/Scripts/Lot/*.cs.meta, Assets/_Project/Scripts/Patrons/*.cs.meta, Assets/_Project/Tests/EditMode/Lot/*.cs.meta, Assets/_Project/Tests/EditMode/Patrons/*.cs.meta, docs/changelogs/lead.md
Why: Merged lot, patrons, cars and economy-night round-2 work (design.md section 10, Pressure rules). Cars first arrived with duplicated members that broke compilation; the cars agent fixed them and they were re-merged. These are the Unity-generated meta files for the new scripts.
Tests: Full EditMode suite 120 of 120 passed on merged main.

## lead: publish round-2 build with queue, letters and timers
Date: 2026-11-06
Files: site/*, docs/changelogs/lead.md
Why: Web build after play-test round 2: queue of 3, car letters, park and pickup timers with dark fill, heavy cars, vertical CURB, overlay with letters and timers (design.md section 10, Pressure rules).
Tests: Full EditMode run on merged main: 130 of 130 passed. Web build loaded locally: CURB fits vertically, three cars queued as A, B, C with park timers counting down in the overlay and a fill on the car. Not verified in play: pickup timer, park expiry vanish, shoving a car.

## lead: add night summary contracts and Audio module ownership
Date: 2026-11-06
Files: CLAUDE.md, docs/design.md, docs/decisions.md, docs/status.md, Assets/_Project/Scripts/Core/Events/NightSummary.cs, Assets/_Project/Scripts/Core/Events/NightEvents.cs
Why: Round-3 play-test: shorter nights, end-of-night summary card, audio (design.md section 10, Night length and summary, Audio). CLAUDE.md gets an owner row for the new Audio module.
Tests: None run; Core additions are plain structs.

## lead: update night-end integration test for round-3 rule
Date: 2026-11-06
Files: Assets/_Project/Tests/EditMode/Integration/EventContractTests.cs, docs/changelogs/lead.md
Why: Merged round-3 branches (shorter nights, summary tally, audio, queue side by side, tickets close at night end). The old test expected an open ticket to survive NightEnded; design.md section 10 now closes every ticket with $0.
Tests: Full EditMode run: 160 of 162 passed before this edit; the two failures were this test (fixed here) and a valet test that OnDestroy cannot satisfy in EditMode (sent back to the valet agent).

## lead: publish round-3 build with audio, night summary and side-by-side queue
Date: 2026-11-06
Files: site/*, Assets/_Project/Scenes/Bootstrap.unity, Assets/_Project/Data/Audio/*, new .meta files, Assets/_Project/Tests/EditMode/Cars/CarDriverDestroyTests.cs, docs/changelogs/lead.md
Why: Round-3 play-test: shorter nights (180 min), end-of-night summary card with Next, generated music and sound effects, queue slots side by side at camera size 8.5 (design.md section 10). Scene and audio config regenerated with ProjectSetup.RunBatch. Test fix: reset the exit counter in SetUp.
Tests: Full EditMode run: 176 of 176 passed. Web build loaded locally (cache disabled): three cars queued A, B, C side by side in view, timers and vanishing working, night 1 ended with the summary card (12 not parked, nothing played), Next started night 2 clean. Audio is not heard or verified by ear; no console errors. Not verified: parking, pickup and tip path in this build.

## lead: add round-4 contracts for nights, stations and tip breakdown
Date: 2026-11-06
Files: docs/design.md, docs/decisions.md, docs/status.md, docs/changelogs/lead.md, Assets/_Project/Scripts/Core/Contracts/ITipCalculator.cs, Assets/_Project/Scripts/Core/Contracts/TipInput.cs, Assets/_Project/Scripts/Core/Events/NightEvents.cs, Assets/_Project/Scripts/Core/Events/NightPlan.cs, Assets/_Project/Scripts/Core/Events/NightSummary.cs, Assets/_Project/Scripts/Core/Events/StationEvents.cs, Assets/_Project/Scripts/Core/Events/StationKind.cs, Assets/_Project/Scripts/Core/Events/TicketEvents.cs, Assets/_Project/Scripts/Core/Events/TipBreakdown.cs, Assets/_Project/Scripts/Core/Events/TipZeroReason.cs
Why: Play-test round 4: seven nights with one new challenge each, EV charging and car wash stations, mirrors, faster clock (design.md section 10, Challenge schedule).
Tests: None run. ITipCalculator gains Explain, so main does not compile until economy-night implements it and fakes are updated.

## lead: merge round-4 module work and pin the test harness clock
Date: 2026-11-06
Files: Assets/_Project/Tests/EditMode/Integration/NightHarness.cs, generated .meta files, docs/changelogs/lead.md
Why: Merged economy-night, patrons, lot, cars and audio round-4 work (design.md section 10, Challenge schedule). The clock now defaults to 1.5 game minutes per second, so the integration test harness pins it at 1.0 to keep its minute maths exact.
Tests: Full EditMode suite 223 of 223 passed on merged main (4 integration tests failed before the harness fix).

## lead: publish round-4 build with seven-night challenges
Date: 2026-11-06
Files: site/*, Assets/_Project/Scenes/Bootstrap.unity, Assets/_Project/Data/Night/*, generated .meta files, docs/changelogs/lead.md
Why: Round-4 play-test: mirrors, 2-minute nights, seven nights with one new challenge each (EV charging, car wash), end card with tip calculation and next-night unlock note (design.md section 10, Challenge schedule). Scene regenerated with ProjectSetup.RunBatch.
Tests: Full EditMode run: 242 of 242 passed. Web build played locally through nights 1 to 3: night 1 one car at a time, night 2 three cars queued, night 3 blue EVs with charge bars and charging bays; end cards showed the tip calculation and the Tomorrow note. Not verified in play: charging and washing a car, the wash slot (night 4), station label fit after the last fix, nights 5 to 7.

## lead: merge balance simulation and fix clock line in design.md
Date: 2026-11-06
Files: tools/sim/*, docs/balance.md, docs/design.md, docs/changelogs/lead.md
Why: Human asked for a simulation of the ideal number of parking spots per night (design.md section 10, Challenge schedule). The qa agent built tools/sim and docs/balance.md; design.md section 10 item 1 still described the old clock speed.
Tests: Simulation self-test passed per the qa agent; no Unity code changed. Not re-run by the lead.

## lead: add ActiveSpots to NightPlan
Date: 2026-11-06
Files: Assets/_Project/Scripts/Core/Events/NightPlan.cs
Why: balance simulation (docs/balance.md) recommends 4 open spots on nights 1-2 and 5 from night 3; the plan carries the count (0 = all).
Tests: none yet; economy-night and lot add them with their changes.
