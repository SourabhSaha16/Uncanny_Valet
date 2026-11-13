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

## lead: apply Schedule A pacing, closed spots, $250 debt
Date: 2026-11-06
Files: docs/design.md, docs/decisions.md, .meta files for new Lot scripts and tests
Why: user asked to apply the simulation's recommendation (docs/balance.md); merged agent branches economy-night and lot.
Tests: EditMode 248 of 248 passed in batch mode.

## lead: rebuild site with Schedule A pacing and closed spots
Date: 2026-11-06
Files: site/
Why: publish point for the simulation-based pacing round.
Tests: EditMode 248 of 248; Web build succeeded.

## lead: add ITuning, TuningKey catalog and late EV fields on NightPlan
Date: 2026-11-06
Files: Core/Contracts/ITuning.cs, TuningStore.cs, TuningCatalog.cs, TuningInfo.cs; Core/Events/TuningKey.cs, TuningEvents.cs, NightPlan.cs
Why: user asked for every pacing number to be tunable live from a debug panel and for a late-night EV surge.
Tests: none yet; module agents add theirs.

## lead: merge round 5 (pacing, layered unlocks, charge spots, expanding lot, urgency audio, tuning panel)
Date: 2026-11-06
Files: merges of economy-night, patrons, lot, audio, integration branches; Bootstrap.unity regenerated; docs/design.md, docs/decisions.md; LotGeometryTests.cs (removed a wrong assertion)
Why: user feedback round 5.
Tests: EditMode 281 of 281 passed in batch mode.

## lead: rebuild site for round 5
Date: 2026-11-06
Files: site/
Why: publish point for pacing, charge spots, expanding lot, urgency audio and the F2 tuning panel.
Tests: EditMode 281 of 281; Web build succeeded; local load shows no console errors and F2 opens the panel.

## lead: add SpawnArriving and DriveAwayAndDespawn to ICarFactory
Date: 2026-11-06
Files: Core/Contracts/ICarFactory.cs, Cars/CarFactory.cs (stub), Tests/EditMode/Integration/FakeCarFactory.cs
Why: Round 6 asks for cars that drive in and out; the contract lets Patrons ask and Cars animate. Stubs keep main compiling until the cars agent implements them.
Tests: not run (contract only, stubs delegate to Spawn and Despawn).

## lead: merge round 6 (drive in/out, mud wipe, pickup banner), debt $500
Date: 2026-11-06
Files: RunConfig.cs, RunConfig.asset, docs/design.md, docs/decisions.md, merges of cars, patrons, integration branches
Why: Round 6 requests. Pacing increase was cancelled by the human; economy-night work discarded; only the debt change kept.
Tests: pending (Unity editor open, batch tests cannot run yet).

## lead: rebuild site for round 6
Date: 2026-11-06
Files: site/, Assets/_Project/Scenes/Bootstrap.unity, two .meta files
Why: Publish round 6 (cars drive in and out, dark mud wiped left to right, pickup banner names car and spot, debt $500).
Tests: 297 of 297 EditMode passed; local browser smoke test loaded with no console errors.

## lead: add boss order contracts (OrderId, OrderEvents, CarDelivered, tuning keys)
Date: 2026-11-07
Files: Core/Ids/OrderId.cs, Core/Events/OrderEvents.cs, OrderOutcome.cs, TicketEvents.cs (CarDelivered), TuningKey.cs, Core/Contracts/TuningCatalog.cs
Why: Boss messages feature (design.md section 6). New Orders module issues orders, keeps car visit history, judges at delivery and gives strikes; Patrons publishes CarDelivered; UI shows the message panel.
Tests: not yet run (contracts only; TuningSeeder must seed the four new keys before tests pass).

## lead: add IsChargingSpot and RunLost to Core contracts
Date: 2026-11-07
Files: Core/Contracts/ILotLayout.cs, Core/Events/NightSummary.cs; merges of the orders, patrons and ui branches
Why: Orders needs to tell charging spots from regular ones; a third strike must end the run through the normal summary card (NightSummary.RunLost makes IsFinalNight true, so Next starts a new run).
Tests: pending (follow-up agents fix the implementers; full suite run after).

## lead: merge round 7 follow-ups; wire RunLossGuard and NightFlow run loss
Date: 2026-11-07
Files: Bootstrap/GameBootstrap.cs, Bootstrap/NightFlow.cs; merges of lot, orders, integration and night branches
Why: A final strike ends the night (RunLossGuard) and Next after a lost summary starts a new run (NightFlow reads NightSummary.RunLost).
Tests: run next (full EditMode suite).

## lead: fix CameraFollow crash when GameBootstrap.Awake runs first
Date: 2026-11-07
Files: Bootstrap/CameraFollow.cs, Tests/EditMode/Integration/CameraFollowTests.cs
Why: After the scene was regenerated, GameBootstrap.Awake ran before CameraFollow.Awake, so SetWorldBounds hit a null camera and aborted Bootstrap (no night started). The camera is now cached on first use. Found by running the Bootstrap scene in batch play mode.
Tests: new regression test; full suite and a browser smoke test follow.

## lead: rebuild site for round 7 (boss messages)
Date: 2026-11-07
Files: site/, Assets/_Project/Scenes/Bootstrap.unity, meta files
Why: Publish boss messages: side panel, orders from night 2, strikes, run lost on the third strike, debt $500.
Tests: 366 of 366 EditMode passed; Bootstrap scene ran in batch play mode; browser smoke test shows night 1, BOSS panel and car arriving with no new console errors.

## lead: start pivot/chaos branch with redesign doc and MetroCity art sources
Date: 2026-11-09
Files: docs/redesign/chaos_redesign.md, docs/design.md, docs/decisions.md, art-src/MetroCity/*.rar, .github/workflows/ci-cd.yml
Why: Human approved the chaos redesign. Baseline on main: 366 of 366 EditMode tests. CI now also deploys site/ from pivot/chaos so the pivot build replaces the live site.
Tests: baseline run, 366 of 366 passed.

## lead: commit generated .meta files for MetroCity art
Date: 2026-11-09
Files: Assets/_Project/Art/ThirdParty/MetroCity/**/*.meta
Why: Unity batch import compiled the importer with no errors and sliced sheets (cars 36, characters 144, road 100, buildings 9), PPU 32, Point filter.
Tests: batch import only; EditMode run follows with pivot-core.

## lead: regenerate scene, configs and metas after pivot-core merge
Date: 2026-11-09
Files: Assets/_Project/Scenes/Bootstrap.unity, Data/Shift/ShiftConfig.asset, new .meta files
Why: pivot-core could not run Unity; ProjectSetup.RunBatch builds ShiftConfig and the shift scene.
Tests: 317 of 317 EditMode passed (old night/orders tests removed with their code).

## lead: rebuild site for pivot step 2-3 (shifts, score, results)
Date: 2026-11-09
Files: site/
Why: First pivot/chaos publish; replaces the live site per the human. 4-minute shift, score, combo, stars, retry; art not yet in use.
Tests: 317 of 317 EditMode; browser smoke test shows shift clock, score overlay and car A with no console errors.

## lead: changelog check ignores commits already on main for a new branch
Date: 2026-11-09
Files: tools/check-changelog.sh
Why: The first push of pivot/chaos failed CI because the check walked the whole history, including two old main commits without entries.
Tests: ran the script locally against HEAD with a zero base: passes.

## lead: merge pivot-visuals and rebuild site with MetroCity art
Date: 2026-11-09
Files: Assets/_Project/Scenes/Bootstrap.unity, art configs, metas, site/
Why: Diner lot, car sprites and valet animation from the imported MetroCity art.
Tests: 338 of 338 EditMode; batch setup had no compile errors or missing-sprite warnings; browser shows the lot, a car sprite and the valet.

## lead: add UI style guide from the scene art prompt deck
Date: 2026-11-09
Files: docs/redesign/ui_style_guide.md
Why: The human supplied a scene art prompt deck; the UI-relevant parts (style bible, cast, HUD, results, title) are kept for the UI agent.
Tests: docs only.

## lead: merge pivot-ui and rebuild site with pixel UI
Date: 2026-11-09
Files: Assets/_Project/Scenes/Bootstrap.unity, metas, site/
Why: Pixel-style HUD, floating tickets, results and title screens per docs/redesign/ui_style_guide.md.
Tests: 363 of 363 EditMode; batch setup compiled cleanly; browser shows title, key-press start, HUD (clock, score and stars, key board) and a car driving in; one 404 resource error (not investigated).

## lead: revise UI style guide (no car letters, flash urgency, info box, timing, zoom, pedestrians)
Date: 2026-11-09
Files: docs/redesign/ui_style_guide.md
Why: Human feedback with aerial car-park references after the first UI build.
Tests: docs only.

## lead: merge pivot-polish and rebuild site
Date: 2026-11-09
Files: Assets/_Project/Scenes/Bootstrap.unity, new configs and metas, site/
Why: Tag colours instead of car letters, pickup flash, info box after arrival, closer camera, crosswalks and pedestrians.
Tests: 403 of 403 EditMode; batch setup compiled with no errors or sprite warnings; browser shows zoomed lot, crosswalk with arrows, walking pedestrians, tagged car; flash, info box and dirt/charge meters not yet seen in play.

## lead: UI style guide revision 2
Date: 2026-11-09
Files: docs/redesign/ui_style_guide.md
Why: Human feedback: drop car meters and roof band, add licence plates, tint sprites, more car sprites.
Tests: docs only.

## lead: merge pivot-plates and rebuild site
Date: 2026-11-09
Files: Assets/_Project/Scenes/Bootstrap.unity, configs, metas, site/
Why: Licence plates, tinted car bodies, legacy car meters and roof stripe removed, police sprites added.
Tests: 442 of 442 EditMode; batch setup clean; browser shows plate DLZ 595 under the car and on the key board, no meters or roof band.

## lead: rewrite design.md as a living document (Part A current, Part B legacy)
Date: 2026-11-09
Files: docs/design.md, docs/status.md, docs/decisions.md
Why: Human asked to keep the game design updated with every change. Part A records the chaos redesign as built, in progress and planned, with a Design log.
Tests: docs only.

## lead: merge pivot-crowd, fix seeded random, update design doc, rebuild site
Date: 2026-11-09
Files: Patrons/SeededPatronRandom.cs, Assets scene and metas, docs/design.md, site/
Why: Short plates, patron and ambient people flows, face-down delivery, bell on key board, audio mix, headlights. Regression found by PlateTests: xorshift gave near-zero first output for small seeds, so the seed is now scrambled.
Tests: 482 of 482 EditMode; browser check: closer zoom, plate ZR 9, walkers in the lot, no new console errors.

## lead: save Patron Director spec
Date: 2026-11-09
Files: docs/redesign/patron_director_spec.md, docs/decisions.md
Why: Human supplied the spec; agents implement it in stages.
Tests: docs only.

## lead: log play-test notes decisions
Date: 2026-11-09
Files: docs/decisions.md
Why: Human answered clarifying questions for the queue, wash and arrow notes; implementation is queued behind director-1.
Tests: docs only.

## lead: merge director-1, fix two test bugs, add design A10, rebuild site
Date: 2026-11-09
Files: Assets scene and metas, Tests/EditMode/Director (FakeClock public, fixture lists cleared in SetUp), docs/design.md, site/
Why: Patron Director steps 1-2. The merged tests had an accessibility compile error and shared event lists across tests (NUnit reuses the fixture).
Tests: 557 of 557 EditMode; browser: F3 overlay shows Director ON, phase BuildUp, W/T/P/F, arrivals 1/14, next in 24.6 s, decision log.

## lead: merge pivot-queue, narrow queue gap and fix tests, update design, rebuild site
Date: 2026-11-09
Files: Lot/LotConfig.cs (queue slot gap 0.6 to 0.15), Tests/EditMode/Patrons/TicketRegistryNightTests.cs, docs/design.md, scene and metas, site/
Why: Queue of 4 widened the lot past the camera and geometry limits (two Lot tests); one Patrons test still expected the removed park timer.
Tests: 565 of 565 EditMode; browser shows 4 arrival slots with up arrows, curb down arrows, plug icons under 13 and 14, key board without a bar before pickup.

## lead: shrink curb and arrival arrows and EV plug icons, rebuild site
Date: 2026-11-09
Files: Lot/WalkwayStyle.cs defaults, Data/Lot/LotArtConfig.asset, Lot/SpotBuilder.cs (PlugPixel 0.16 to 0.07), site/
Why: Human found the arrows and plug icons far too large. Curb arrow 3.4 to 1.6, stroke 0.4 to 0.16, head 1.1 to 0.45, arrival arrow 3 to 1.4, plug pixel 0.07.
Tests: 565 of 565 EditMode; browser shows small arrows and plugs.

## lead: merge pivot-stations, update design doc, rebuild site
Date: 2026-11-09
Files: scene and metas, docs/design.md, site/
Why: EV bays and the wash slot moved into the spot grid (bays numbered 10 and 11), wash icon, smaller score and combo HUD.
Tests: 577 of 577 EditMode; browser shows the new layout.

## lead: merge director-2 (picker, dilemma rules, gate, soak test), update design A10, rebuild site
Date: 2026-11-09
Files: scene and metas, docs/design.md, site/
Why: Patron Director step 3 per docs/redesign/patron_director_spec.md.
Tests: 625 of 625 EditMode (including the 200-shift soak); browser F3 overlay shows dilemma section and the director choosing the archetype, no console errors.

## lead: add pedestrian count to the F2 tuning panel, start at 3
Date: 2026-11-10
Files: TuningKey.cs, TuningCatalog.cs, TuningSeeder.cs, GameBootstrap.cs, PedestrianConfig.cs, PedestrianCrowd.cs, docs/design.md, site/
Why: the human wants fewer pedestrians at the start and control over the number (design.md A4).
Tests: 625 of 625 EditMode.

## lead: lot v2 spec, IRoadLayout contract, design doc updates
Date: 2026-11-10
Files: docs/redesign/lot_v2_layout.md, docs/redesign/lot_v2_sketch.png, docs/design.md, docs/decisions.md, Core/Contracts/IRoadLayout.cs
Why: the human's sketch and answers (design.md A5, A8, A9).
Tests: none (spec and an interface).

## lead: merge lot-v2 and traffic, update design doc
Date: 2026-11-10
Files: merges of worktree-agent-aed01734270b59177 and worktree-agent-ad7a2343048ede8aa, docs/design.md
Why: Lot v2 layout and the Traffic module per docs/redesign/lot_v2_layout.md.
Tests: 672 of 672 EditMode.

## lead: merge traffic-wire, rebuild site
Date: 2026-11-10
Files: merge of worktree-agent-a23a9621f7d814729, scene and configs, site/
Why: road traffic wired into patron arrival and departure (docs/redesign/lot_v2_layout.md).
Tests: 680 of 680 EditMode.

## lead: merge lot-v3, update design doc, rebuild site
Date: 2026-11-10
Files: merge of worktree-agent-a467414c00b26f057, docs/design.md, site/
Why: the human's 8 play-test changes (docs/changelogs/lot-v3.md).
Tests: 707 of 707 EditMode; browser shows the new look, no console errors.

## lead: merge lot-v4, update design doc, rebuild site
Date: 2026-11-10
Files: merge of worktree-agent-aca42b483dd08be4f, docs/design.md, docs/redesign/ui_style_guide.md, site/
Why: the human's 7 play-test notes (docs/changelogs/lot-v4.md).
Tests: 712 of 712 EditMode; browser shows the gap, fewer arrows, no console errors.

## lead: merge ui-status, update design doc and style guide, rebuild site
Date: 2026-11-10
Files: merge of worktree-agent-a92c1ead62b9e0ed2, docs/design.md, docs/redesign/ui_style_guide.md, site/
Why: the human's UI notes: status words, plate only when parked, pickup emotions (docs/changelogs/ui-status.md).
Tests: 737 of 737 EditMode; browser smoke test with no console errors.

## lead: merge queue-director-fix, update design doc, rebuild site
Date: 2026-11-10
Files: merge of worktree-agent-a39105b32d5892f2e, docs/design.md, docs/pending_tasks.md, Assets/_Project/Scenes/Bootstrap.unity (regenerated), site/
Why: the human's notes: arrivals stopped late in a shift, and queue cars parked behind an empty slot (docs/changelogs/patrons.md). Approved the new Core contract IQueueSlotSource and the CarAutoDrive snap-to-final-pose edit.
Tests: 772 of 772 EditMode; browser smoke test with no console errors. Not played; the real cause of the failed spawn is unconfirmed.

## lead: merge arrival-sfx, status-camera, slot-tint, ped-collision; update docs; rebuild site
Date: 2026-11-10
Files: merges of worktree-agent-a7bab8b9a3bcd29a6, -af222a4dbf298956c, -af78fe44b6f7ba760, -a3350361302357c08; docs/design.md, docs/pending_tasks.md, regenerated Bootstrap.unity and PedestrianConfig.asset, site/
Why: the human's play-test notes (status after wash, arrival sound on stop, camera zoom, big box replaced by slot tint, pedestrian collision with bounce and sound). Approved new Core files PhysicsLayers and PedestrianEvents and the ArrivalSlotScanner layer filter.
Tests: 811 of 811 EditMode; browser smoke test with no console errors. Not played.

## lead: merge ped-collision own-car fix and slot-tint service icons; update docs; rebuild site
Date: 2026-11-10
Files: merges of worktree-agent-a3350361302357c08 and worktree-agent-af78fe44b6f7ba760, docs/design.md, regenerated Bootstrap.unity, site/
Why: the human's notes: patrons pushed their own car on exit; need icons from queue arrival.
Tests: 817 of 817 EditMode; browser smoke test with no console errors. Not played. Icon trigger is CarArrived (no patron-exited event exists in Core).

## lead: merge difficulty-dials, adopt car handling spec, update docs, rebuild site
Date: 2026-11-10
Files: merge of worktree-agent-a3ee1dcf52afb7383, docs/redesign/car_handling_spec.md, docs/design.md, docs/pending_tasks.md, regenerated Bootstrap.unity and Director assets, site/
Why: the human asked for fewer tunables with designer names, and handed over the car handling spec (checked against the code; corrections in the spec's section 0).
Tests: 853 of 853 EditMode; browser smoke test with F2 open (dials show, no console errors). Not played.

## lead: merge handling-step1, update docs
Date: 2026-11-10
Files: merge of worktree-agent-a79787fcea3c431c5, docs/design.md, docs/pending_tasks.md, docs/redesign/car_handling_spec.md
Why: car handling rollout step 1 (docs/redesign/car_handling_spec.md section 8). No runtime change, so site/ was not rebuilt.
Tests: 863 of 863 EditMode; PlayMode 1 passed, 7 ignored until step 2 (agent's run).

## lead: merge pickup-arrow and handling-step2 (drift, steering, car bounce); update docs; rebuild site
Date: 2026-11-10
Files: merges of worktree-agent-a5d7bd4e2d68a9b95 and worktree-agent-acc49e39f90d03a53, docs/design.md, docs/pending_tasks.md, docs/redesign/car_handling_spec.md, regenerated Bootstrap.unity and CarHandlingConfig.asset, site/
Why: the human's notes (pickup facing arrow, bouncing car collisions) and car handling rollout step 2. Approved the ForwardGrip reading, the Core additions (IInputSource.Drift, IDrivable.SetDrift, CarsCollided) and the one-line SeatController edit.
Tests: 917 of 917 EditMode, 11 of 11 PlayMode; browser smoke test shows the pickup arrows, no console errors. Drift and bounce not played.

## lead: merge handling-step3 (skid marks, tyre audio), update docs, rebuild site
Date: 2026-11-10
Files: merge of worktree-agent-a649e5206949c972d, docs/design.md, docs/pending_tasks.md, regenerated Bootstrap.unity, site/
Why: car handling rollout step 3 (docs/redesign/car_handling_spec.md section 6).
Tests: 948 of 948 EditMode, 14 of 14 PlayMode (agent's run); browser smoke test with no console errors. Marks and sounds not seen or heard.

## lead: draft valet powers, add to design and pending tasks
Date: 2026-11-10
Files: docs/redesign/valet_powers.md, docs/design.md, docs/pending_tasks.md
Why: the human's note on a power meter with three stackable powers (speed boost, freeze time, Park Smash). Draft with open questions; no code.
Tests: none (docs only).

## lead: merge play-test round 3 (handling, sounds, solid trip cars, wider lot, dial baseline); update docs; rebuild site
Date: 2026-11-10
Files: merges of worktree-agent-a649e5206949c972d, -a0f26c5db645b6d0e, -a3ee1dcf52afb7383, -ad23a3ed458e2fe98; docs, regenerated assets and scene, site/
Why: the human's notes (sharper turning and tunable driving values, softer sound, collision for incoming cars, wider lot) and the hand-tuned dial values as the baseline. Approved Core ITripObstacles. Set CarHandlingConfig code default aisle to 6.
Tests: 974 of 974 EditMode, 16 of 16 PlayMode; browser smoke test shows the wider lot, no console errors. Not played.

## lead: record missing changelog for 38e3723 so CI deploys the current site
Date: 2026-11-11
Files: docs/changelogs/lead.md
Why: commit 38e3723 ("lead: regenerate assets, rebuild site after dial baseline merge", Assets/ and site/ only) had no changelog entry, so the changelog check failed for the push ending at 9a8f5f2 and the deploy was skipped. History is not rewritten; the check only inspects the commits of each push, so this commit lets the next deploy run. 38e3723 regenerated the Bootstrap scene and CarHandlingConfig asset and rebuilt site/ after merging the dial baseline.
Tests: none (docs only); EditMode 974 of 974 and PlayMode 16 of 16 at 9a8f5f2.

## lead: merge qa test review
Date: 2026-11-11
Files: merge of worktree-agent-abb8c4e2d97e0350f (docs/redesign/test_review.md, PlateTests.cs, PlayMode/FastTimeFixture.cs), docs/pending_tasks.md
Why: the human asked whether all tests are needed to cut build time. Result: none removed; two slow tests fixed (EditMode test time 6.1 s to 1.9 s, PlayMode 72 s to 32 s wall). Web build (497 s) and Unity start-up dominate. No runtime change, so site/ was not rebuilt.
Tests: 974 of 974 EditMode, 16 of 16 PlayMode after the merge.

## lead: adopt the faster release routine in CLAUDE.md
Date: 2026-11-11
Files: CLAUDE.md
Why: the human approved the proposals in docs/redesign/test_review.md: EditMode every merge; RunBatch, PlayMode and the web build only when needed; batch deploys. The one-Unity-launch idea is untried and not part of the routine. No runtime change.
Tests: none (docs only).

## lead: merge patience-tuning, update docs, rebuild site
Date: 2026-11-11
Files: merge of worktree-agent-a8c773291166eb506, docs/design.md, regenerated Bootstrap.unity, site/
Why: the human's note: pickup patrons should lose patience sooner and the timers should be tunable (docs/changelogs/patrons.md).
Tests: 978 of 978 EditMode, 16 of 16 PlayMode; browser smoke test shows the two new rows (45 and 0.6), no console errors. Not played.

## lead: stable and beta Pages sites, changelog exemption list
Date: 2026-11-11
Files: .github/workflows/ci-cd.yml, tools/check-changelog.sh, tools/changelog-exempt.txt, CLAUDE.md, docs/design.md
Why: the human wants a beta site to test new features, with the main site link given to playtesters. The workflow now deploys main's site/ at the root and pivot/chaos's site/ under /beta/. The changelog check skips commits listed in tools/changelog-exempt.txt (38e3723, pushed without an entry; history is not rewritten) so the first push of main can pass. Current pivot/chaos promoted to main as the first stable build.
Tests: tools/check-changelog.sh run locally over origin/main..HEAD: passed (97 commits). No game change.

## lead: merge tip-pop (patience 25, tip pop), move pending work to GitHub issues, rebuild site
Date: 2026-11-11
Files: merge of worktree-agent-ae877026573b0fc40, docs/design.md, docs/pending_tasks.md, CLAUDE.md, regenerated PatronConfig asset, site/
Why: the human's notes (patience 25, tip pop over the patron) and the request to track pending work as GitHub issues and a Project. 65 issues created from docs/pending_tasks.md, added to Project 4 with Status and Priority; labels and milestones created; CLAUDE.md gained 'Tracking work'.
Tests: 992 of 992 EditMode, 16 of 16 PlayMode; browser smoke test with the F2 panel open, no console errors. Tip pop not seen in play.

## lead: remove Kev from the docs, update the issue title rule
Date: 2026-11-11
Files: CLAUDE.md, docs/design.md, docs/redesign/chaos_redesign.md, docs/redesign/ui_style_guide.md, docs/changelogs/lead.md
Why: the human dropped Kev, the task board and the swap button, and asked for plain issue titles with a two-line description and one acceptance line. GitHub issues were retitled, rewritten and given priority:1 to priority:4 labels; Kev issues closed as not planned.
Tests: docs only, none run.

## lead: valet powers decisions and Core events (PowerKind, power events, IWorldTime)
Date: 2026-11-11
Files: Assets/_Project/Scripts/Core/Events/PowerKind.cs, PowerEvents.cs, Contracts/IWorldTime.cs (+ metas), docs/redesign/valet_powers.md, docs/design.md, docs/decisions.md, docs/changelogs/lead.md
Why: the human answered the seven valet powers questions (valet_powers.md section 6, design A11); issue 35 adds the shared events and the freeze time-scale contract that every power module builds on.
Tests: 992 of 992 EditMode (Core compiles; no new logic yet).

## lead: merge agent/integration power meter HUD (#39)
Date: 2026-11-11
Files: merge of worktree-agent-ad38424284dae36c8 (UI/Hud power meter files, HudEmotionConfig, PowerMeterTests), docs/changelogs/lead.md
Why: valet powers issue 39 (design A11). The HUD listens only to PowerMeterChanged, PowerActivated and PowerEnded; nothing publishes them yet, so it is invisible until the Powers module merges. Notch positions and ring durations in PowerMeterSettings are placeholders to match PowerMeterConfig.
Tests: 1004 of 1004 EditMode (12 new).

## lead: merge agent/powers meter, activation and stacking (#37, #40, #44, #38 part)
Date: 2026-11-11
Files: merge of worktree-agent-a1617d9ae486b4055 (Scripts/Powers/, PowerRows, TuningKey, TuningSeeder, asmdefs, tests), UI/Hud/PowerMeterSettings.cs, docs/changelogs/lead.md
Why: valet powers issues 37, 40, 44 and the listener part of 38 (design A11). HUD notch defaults changed to 0.25 and 0.55 to match the 25/55/100 level thresholds. PerfectPark listener is a seam until issue 16 lands; the Powers objects are not yet in the scene (wiring pending).
Tests: 1027 of 1027 EditMode (including the HUD merge).

## lead: merge agent/lot perfect drop-off and Perfect Park (#36, #16 part)
Date: 2026-11-11
Files: merge of worktree-agent-a4901f8ee332749de (Lot judges and runner, Core ParkEvents and PerfectRows, tuning keys), conflict resolution in TuningKey, TuningCatalog, TuningSeeder (kept both agents' keys; Seed call in GameBootstrap now passes perfect by name), docs/changelogs/lead.md
Why: valet powers issue 36 and the judging part of issue 16 (design A11, car handling spec section 4). Still open: the +$2 tip bonus (Patrons/Economy), the PERFECT! pop, ding and camera kick, the results stat, and the PowerMeterController PerfectPark listener.
Tests: 1055 of 1055 EditMode after merging HUD, Powers and Lot (lot agent reported 16 of 16 PlayMode before the merge).

## lead: merge powers follow-up, Perfect Park tip bonus and powers wiring (#16, #38, #60)
Date: 2026-11-11
Files: merges of worktree-agent-a1617d9ae486b4055 (PerfectPark listener), worktree-agent-ae6387393fc8291b2 (PerfectBonusLedger, tip bonus), worktree-agent-a7dac40ca1c52e782 (Powers scene wiring, PowerMeterConfig asset, builder split, results stat, PERFECT! pop, camera kick, ding), docs/design.md
Why: valet powers phase 1 (design A11) and car handling step 4 (+$2 Perfect Park). Bootstrap.unity re-run by RunBatch showed only fileID churn (637 of 637 lines) and was reverted.
Tests: 1072 of 1072 EditMode, 16 of 16 PlayMode after the merges. Not played in a browser.

## lead: merge agent/integration tip pop fix (#67)
Date: 2026-11-11
Files: merge of worktree-agent-a61775407c959e1e4 (TipPopTracker, PendingTipPops, tests), docs/changelogs/lead.md
Why: issue 67. PatronDeparture removed the car on PatronBoarded before the HUD heard it, and the tracker also dropped the tip on CarDeparted, so the tip pop never showed. The tracker now stores the car position at TicketClosed. Not seen in play yet.
Tests: 1074 of 1074 EditMode, 16 of 16 PlayMode (agent), see rebuild commit.

## lead: rebuild beta site with Perfect Park, powers meter wiring and tip pop fix [Claude]
Date: 2026-11-11
Files: site/ (regenerated Web build, build-info.txt, changelog.txt), docs/changelogs/lead.md
Why: first player-visible build of Perfect Park (+$2 bonus, PERFECT! pop, kick, ding, results stat), the power meter HUD and Q activation (powers have no effect yet), and the tip pop fix (issue 67). Beta site only; main stays on the earlier build.
Tests: 1074 of 1074 EditMode, 16 of 16 PlayMode (agent runs); browser smoke test: title and shift load, no console errors. Perfect Park, meter and tip pop not seen in play.

## lead: merge agent/traffic keep a turning patron car visible to lane traffic (#3) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-ac46417314c5f98f3 (EntryTrip, TripContext, LanePath, TripTuning, TrafficConfig, TrafficModule, TurnIntoQueueTests), docs/changelogs/lead.md
Why: issue 3. EntryTrip dropped the car from lane occupancy at the start of the turn into the queue, so an ambient car right behind it drove through it. The car stays registered until it is LaneClearance (1.6) from the lane line.
Tests: 1077 of 1077 EditMode, 16 of 16 PlayMode (agent); 1077 of 1077 EditMode after the merge.

## lead: merge agent/integration Angry at 80% and Will not tip (#7) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-a0adec20aa7f251d4 (Emotion states, EmotionRows, EmotionTuningLink, CarStatus WillNotTip, HudEmotionConfig asset, KeyBoardPainter width, tests), docs/changelogs/lead.md
Why: issue 7, the owner's decision: Angry from 80% of the pickup patience, Will not tip at 100% or expiry; both levels tunable in F2 (Patron mood). Tip maths unchanged. The CAR SLOTS panel is wider so Cust. Angry fits. Car body flash (CarFlashConfig 0.85) is unchanged and now differs from the plate's 80%.
Tests: 1085 of 1085 EditMode, 16 of 16 PlayMode after merging the traffic and Angry branches; RunBatch regenerated TrafficConfig.asset (new LaneClearance field), Bootstrap.unity churn reverted.

## lead: merge agent/lot plain-language Perfect rows and looser Perfect Park (#69) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-a95488145e96040e2 (PerfectRows, PerfectSettings, PerfectJudgeTests, DifficultyDialsTests), docs/changelogs/lead.md
Why: issue 69. Perfect rows have plain-language labels; the three judging rows and the bonus show without Advanced; defaults loosened (angle 12 degrees, side 0.35, lengthwise 0.7; lengthwise slider max 1.2 = bay end slack).
Tests: 1088 of 1088 EditMode after the merge (agent: 16 of 16 PlayMode).

## lead: merge agent/lot exit delivered cars through the top wall (#68) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-abc3d4111115c7d90 (Lot geometry, frame, road art, curb facing up, PixelFont; Traffic ExitTrip, TripContext, AmbientSpawner; IRoadLayout WestLane renamed TopLane; LotArtConfig asset; tests), conflict in docs/changelogs/lot.md (kept both entries), docs/changelogs/lead.md
Why: issue 68. The drop-off faces up, delivered cars leave straight up through a gap above the curb onto a new one-way EXIT road; the bottom road is a one-way ENTRY road; ambient cars split evenly over both. Camera framing unchanged. Not seen in play.
Tests: 1098 of 1098 EditMode, 16 of 16 PlayMode after RunBatch (Bootstrap.unity churn 563 of 563 lines reverted).

## lead: rebuild beta site (Angry 80%, Will not tip, traffic fix, Perfect Park leeway, top exit road) [Claude]
Date: 2026-11-11
Files: site/ (regenerated Web build, build-info.txt, changelog.txt), docs/changelogs/lead.md
Why: player-visible changes since the last beta build: issues 7 (Angry at 80%, Will not tip), 3 (turning patron car visible to lane traffic), 69 (plain Perfect Park rows, looser defaults) and 68 (one-way ENTRY road below, one-way EXIT road above, up-facing drop-off). Beta site only; main stays on the earlier build.
Tests: 1098 of 1098 EditMode, 16 of 16 PlayMode; browser smoke test at 1280x800: title and shift load, curb arrows face up, exit gap visible in the top wall, no console errors. The exit route, labels, Will not tip and traffic fix were not seen in play.

## lead: merge agent/director always a car in the arrival queue (#70) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-abc606cd9b17808d8 (IdleGuard, ArrivalRows, ArrivalPacer, PacerInput, PacingSettings, DirectorBrain, TuningKey, TuningCatalog, DirectorConfig asset, tests, docs/changelogs/director.md), docs/changelogs/lead.md
Why: issue 70. When no car is waiting in the arrival queue the Director sends one within ArrivalIdleLimit (2 s, F2, visible); the first car of a shift too; the guard stops near the end of the shift (ArrivalIdleCutoff 20 s, Advanced) and at the hard arrival cap; queued cars leave the old pacing, mercy and queue-full holds in force. Fast parkers use the arrival budget sooner (no schedule check).
Tests: 1111 of 1111 EditMode, 16 of 16 PlayMode after RunBatch (Bootstrap.unity churn reverted). Soak: longest empty-queue gap 1.5 s over 200 shifts.

## lead: merge agent/lot pickup arrows green only when the patron accepts (#71) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-a2e5e86efc6289fb1 (IPickupAcceptance, PickupAcceptance, CurbArrowTracker, CurbArrowRule, CurbZone, PatronSpawner registration line, tests), docs/changelogs/lead.md
Why: issue 71. The curb arrows are green only when the car faces right and its ticket is PickupRequested (the patron will accept it); neutral when facing right but the patron has not asked; red for wrong facing; they flip live when the patron asks.
Tests: 1115 of 1115 EditMode after the merge (agent: 16 of 16 PlayMode).

## lead: merge agent/integration how-to-play screen (#72) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-a8e38eff03cfeb80c (HowToPlayGate, HowToPlayText, HowToPlayPainter, WordWrap, HighlightMask, UiInput, GameUi, UiPhase, tests), docs/changelogs/lead.md
Why: issue 72. After the first press-any-key on the title a how-to-play screen (the owner's five lines and the controls) shows once per page load; any key starts the shift; retry skips it. Not seen in a browser by the agent.
Tests: 1117 of 1117 EditMode, 16 of 16 PlayMode after both merges.

## lead: rebuild beta site (idle guard, arrows green on accept, how-to-play screen) [Claude]
Date: 2026-11-11
Files: site/ (regenerated Web build, build-info.txt, changelog.txt), docs/changelogs/lead.md
Why: player-visible changes since the last beta build: issues 70 (a car always within 2 s when the queue is empty), 71 (pickup arrows green only when the patron will accept) and 72 (how-to-play screen after the first title press). Beta site only; main stays on the earlier build.
Tests: 1121 of 1121 EditMode, 16 of 16 PlayMode; browser smoke test at 1280x800: title, how-to-play screen (renders, text fits, gold highlights), shift starts on the second key at 3:56, no console errors. The arrows, idle guard and earlier items were not seen in play.


## lead: merge agent/traffic two-way busier ambient roads (#75) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-ac64f4e85aaac1f29 (AmbientDensity, AmbientTraffic, YieldZone, SpawnGate, ViewWindow, TrafficModule, LotRoadArt, DecorLayout, RoadLayout, IRoadLayout, TrafficConfig and LotArtConfig assets, traffic tuning keys, tests), docs/changelogs/lead.md
Why: issue 75. Both roads carry ambient traffic both ways (an extra right-to-left lane per road), 3 to 4 cars kept on screen, ambient cars brake through a YieldZone so an exiting delivered car can merge. Camera framing untouched, so mostly the entry road shows. TrafficAmbientPerMinute removed.
Tests: 1159 of 1159 EditMode, 16 of 16 PlayMode after RunBatch (Bootstrap.unity churn reverted).

## lead: merge agent/integration F2 Copy all values button (#76) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-a88b0b826cfe28c36 (TuningPanel, TuningSnapshotJson, TuningSnapshotJsonTests), docs/changelogs/lead.md
Why: issue 76. A Copy all values button in the F2 panel header copies every tuning value, Advanced ones included, as flat JSON via GUIUtility.systemCopyBuffer, with a 1.5 s Copied! confirmation. Not checked in a browser.
Tests: 1165 of 1165 EditMode after the merge (agent: 16 of 16 PlayMode).

## lead: merge agent/patrons stay from drop-off, patron asks wherever the car is (#77) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-aafeaa8e9141bbe3f (Ticket, CurbCall, TicketRegistry, Director CarBook/CarRecord/LotStateTracker/SnapshotBuilder/WorkloadEstimator, tests), docs/changelogs/lead.md
Why: issue 77. The stay clock starts at drop-off (CarArrived) instead of when a car settles in a bay, and a patron whose stay ends asks for the car from any open state (queue, wash, charge, aisle, bay); Director return time = arrival + stay. Open follow-ups: dirty unwashed car zeroes the tip (existing NotCleaned rule); queued car whose patron asked is not on the key board until Placed.
Tests: 1175 of 1175 EditMode, 16 of 16 PlayMode after the merge.

## lead: merge agent/patrons unwashed dirty car tips, asked-for car joins key board (#77) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-aafeaa8e9141bbe3f follow-up (TipCalculator, TipZeroReason, TicketBoard, tests), docs/changelogs/lead.md
Why: issue 77 follow-ups decided by the owner: an unwashed dirty car now only loses the clean bonus (TipZeroReason.NotCleaned removed); a queued car appears on the key board and ticket list as soon as its patron asks.
Tests: 1176 of 1176 EditMode after the merge (agent: 16 of 16 PlayMode).

## lead: merge agent/director return scheduler, late-shift pickups, arrival cutoff (#12, #13, #73, #15) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-a5e29e2d80f8fe89f (Director ReturnScheduler, LateReturnPlanner, ReturnCandidates, ReturnDirector, ReturnShifter in Patrons, ReturnRows, tuning keys, DirectorConfig asset, tests), docs/changelogs/lead.md
Why: issues 12, 13, 73, 15 (patron director spec step 4 and the owner's end-of-shift request), built on the stay-from-drop-off definition (issue 77). Returns stay inside planned stay +/-25% before the late phase and 4 s spacing; in the last 60 s every open ticket is pulled forward; no new cars in the last 45 s (ArrivalIdleCutoff default 20 -> 45). New Advanced F2 rows: return window, pickup spacing, late-shift start, last late pickup lead. Soak (200 shifts): 13 of 2314 pickups violate spacing outside wave peaks, 11 cars arriving late still waiting at the end.
Tests: 1215 of 1215 EditMode, 16 of 16 PlayMode after RunBatch (Bootstrap.unity churn reverted).

## lead: fix merge conflict markers in Traffic .meta files [Claude]
Date: 2026-11-11
Files: Assets/_Project/Scripts/Traffic/*.cs.meta, Traffic.meta, UncannyValet.Traffic.asmdef.meta, Assets/_Project/Tests/EditMode/Traffic/*.cs.meta (29 files), docs/changelogs/lead.md
Why: commit 7dea408 (Nov 10 lead merge of traffic-wire) left unresolved conflict markers in 29 .meta files. Kept the guid already referenced by the scene (TrafficModule, TrafficConfig) / the HEAD guid for the rest; no other file referenced either guid.
Tests: 1215 of 1215 EditMode after the fix.

## lead: merge agent/integration fix start-up crash from shifted TuningKey numbers [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-a1737e27686b092a4 (DifficultyDialsConfig.asset, BootstrapSceneSmokeTests, PlayMode asmdef), CLAUDE.md (TuningKey is append-only), docs/changelogs/lead.md
Why: the web build threw KeyNotFoundException 'DirTargetScale' at start-up. Commit c6992c0 (two-way traffic) deleted TrafficAmbientPerMinute from the middle of TuningKey, shifting every later key while DifficultyDialsConfig.asset stores raw key numbers, so each dial pointed at the wrong key. The asset was corrected and a PlayMode smoke test now loads the real Bootstrap scene and fails on any logged error. I missed it in the merge review: the traffic report said the key was removed.
Tests: 1215 of 1215 EditMode, 17 of 17 PlayMode (including the new Bootstrap scene smoke test).

## lead: rebuild beta site (two-way traffic, copy values, stay from drop-off, return scheduler) [Claude]
Date: 2026-11-11
Files: site/ (regenerated Web build, build-info.txt, changelog.txt), docs/changelogs/lead.md
Why: player-visible changes since the last beta build: issues 75 (two-way busier ambient roads), 76 (F2 Copy all values), 77 (stay clock from drop-off, patron asks wherever the car is, unwashed dirty car tips, queued asked-for car on the key board), 73/12/13/15 (return scheduler, late-shift pickups, no new cars in the last 45 s) and the start-up crash fix (shifted TuningKey numbers). Beta site only; main stays on the earlier build.
Tests: 1215 of 1215 EditMode, 17 of 17 PlayMode (incl. the new Bootstrap scene smoke test); browser smoke test: title, how-to-play, shift starts (3:53), no console errors. The earlier broken build (KeyNotFoundException DirTargetScale) was never deployed. The new features were not seen in play.

## lead: merge agent/valet Power 1 speed boost (#41) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-a2657675453e36f8b (IPowerBoost, ValetBoost, ValetBoostFx, BargeScanner/BargeRule, TrailEmitter, SparklePool, OnFootMover, Pedestrians stumble (PedestrianStumbleRule, WalkerStumbler), PedestrianId, Audio SfxPowers, PowerRows keys, ValetSceneBuilder, Bootstrap.unity, PedestrianConfig asset, tests), docs/changelogs/lead.md
Why: issue 41 (valet powers level 1). While boosted the valet walks 40% faster, ignores pedestrian blocking (pedestrians stumble 0.4 s), shows a rainbow outline and sparkle trail, and a fanfare plays. Test it by lowering F2 'Meter per Perfect Park' and 'Speed boost unlocks at'. Scene file kept whole (real new components plus fileID churn).
Tests: 1231 of 1231 EditMode, 21 of 21 PlayMode after the merge (incl. Bootstrap scene smoke test).

## lead: merge agent/lot stay starts when a car first rests clear of the queue (#78) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-a9a4911bde6bb31bc (QueueClearRule/Runner/Settings, StayStarter, Ticket, TicketRegistry, CarClearedQueue/StayStarted events, QueueRows and two tuning keys, Director CarRecord/CarBook/LotStateTracker/SnapshotBuilder, tests), docs/changelogs/lead.md
Why: issue 78 closes the loophole of holding a car in the queue and returning it fast. The stay starts the first time the car rests 6 units clear of the queue (F2 Queue clear distance), or when a bay parks it, or after 3 game minutes in the queue (F2 Longest wait in the queue). Replaces the stay-from-drop-off start of issue 77; a ticket whose stay has not started has no return time and is left alone by the ReturnScheduler.
Tests: 1242 of 1242 EditMode, 21 of 21 PlayMode after resolving the TuningKey conflict (kept both appended groups) and RunBatch (Bootstrap.unity churn reverted).

## lead: merge agent/powers Power 2 freeze time (#42) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-a5b2d81be881146ca (WorldTimeReader, IPlayerFocus/TransformPlayerFocus, DirectorTicker, ShiftClock, PatronDirector, TrafficModule, PedestrianCrowd/Walker, CarAutoDrive scaling, FreezeFade/Tracker/Look/VeilPainter, SfxFreeze TimeStop/TimeResume, tests), docs/changelogs/lead.md
Why: issue 42 (valet powers level 2). For the freeze duration (6 s, F2) the shift clock, Director (no arrivals), traffic, trip cars, pedestrians and all clock-driven timers stop; the valet and the player's car keep moving; a cold blue wash with a clear bubble around the valet or driven car, frost edges and a whoosh show it. Skipped: ice outlines on frozen things, music filter. Files near the 200-line limit: PedestrianWalker 199, CarAutoDrive 199, GameBootstrap 199.
Tests: 1260 of 1260 EditMode, 23 of 23 PlayMode after the merge.

## lead: merge agent/powers Power 3 Park Smash (#43) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-af13c3c140575434b (Lot SmashRunner and ParkingSpot.Place, Patrons SmashSettler/SmashTally and TicketSettler.PaySmash, TipInput.MaxSpeedBonus, SmashEvents, UI/Hud Smash flash/ring/coins/pop, ShiftStats SmashTips, Audio SfxSmash, tests), docs/changelogs/lead.md
Why: issue 43 (valet powers level 3). Instantly parks every waiting arrived car in a free bay (EVs charged, dirty cars washed), pays every waiting pickup patron a full tip (speed bonus maxed, one combo multiplier, combo not reset), with flash, ring, coins, boom, chimes and an SMASH! +$N pop. Cars with no free bay stay queued; expired patrons still pay $0; parking is a teleport; patrons vanish and the car drives straight off.
Tests: 1296 of 1296 EditMode, 26 of 26 PlayMode after the merge (incl. Bootstrap scene smoke test).

## lead: rebuild beta site (speed boost, freeze time, Park Smash, return timer after the queue) [Claude]
Date: 2026-11-11
Files: site/ (regenerated Web build, build-info.txt, changelog.txt), docs/changelogs/lead.md
Why: player-visible changes since the last build: issues 78 (stay starts when the car rests clear of the queue), 41 (speed boost), 42 (freeze time), 43 (Park Smash). Beta site only; main stays on the previous build.
Tests: 1296 of 1296 EditMode, 26 of 26 PlayMode (incl. Bootstrap scene smoke test); browser smoke test: title, how-to-play, shift starts (3:57), no console errors. The powers and the new return timer were not seen in play.

## lead: merge agent/cars impact damage lowers the tip (#19) [Claude]
Date: 2026-11-11
Files: merge of worktree-agent-a1b42c739aefef56f (DamageRule, CarDamage, DamageLedger, DamageTag, DamageEvents CarDamaged/CarImpact, DamageRows, TuningKeys DamageMinForce/DamagePercentPerForce, CarFactory/CarHandling*, TicketSettler/TicketRegistry, TicketRow/TicketNeedsTracker/KeyBoardPainter, tests), docs/changelogs/lead.md
Why: issue 19. Hard impacts with walls, station edges and other cars (not pedestrians) add damage to the cars involved; the existing tip stages apply at payout (25% off per 25%, $0 wrecked); the key board shows DMG nn%. CarImpact(CarId, Force, IsPlayerCar) is published for issue 20 (screen shake). Defaults are feel guesses: smallest hit 1.5, 8% per unit of force (a full-speed wall hit is about 52%).
Tests: 1314 of 1314 EditMode, 29 of 29 PlayMode after the merge.

## lead: merge agent/integration screen shake on impact (#20) [Claude]
Date: 2026-11-12
Files: merge of worktree-agent-af9f622ef53ea7698 (CameraShake, ShakeLink, ShakeRows, CameraFollow, PerfectFeedback, TuningSeeder, TuningKey/TuningCatalog, tests), docs/changelogs/lead.md
Why: issue 20. The camera shakes on any hard impact of the car the player drives (CarImpact with IsPlayerCar); strength scales with the impact force from the damage threshold to a full-force value; three Advanced F2 rows (strength 0.35, length 0.35 s, full force 8); uses unscaled time so it works in freeze time.
Tests: 1324 of 1324 EditMode, 29 of 29 PlayMode after the merge.

## lead: rebuild beta site (impact damage, screen shake) [Claude]
Date: 2026-11-12
Files: site/ (regenerated Web build, build-info.txt, changelog.txt), docs/changelogs/lead.md
Why: player-visible changes since the last build: issues 19 (impact damage lowers the tip, DMG % on the key board) and 20 (screen shake on hard impacts of the car you drive). Beta site only; main stays on the earlier build.
Tests: 1324 of 1324 EditMode, 29 of 29 PlayMode (incl. Bootstrap scene smoke test); browser smoke test: title, how-to-play, shift starts (3:57), no console errors. Damage and shake were not seen in play.

## lead: merge agent/integration run state, lot definitions and lot scenes (#80) [Claude]
Date: 2026-11-13
Files: merge of worktree-agent-a1067622abbf49073 (Run module: RunProgress, RunHost, LotDefinition, RunDefinition, LotModifier; Core LotId/World/LotKind/events/contracts; Bootstrap RunBootstrap, TuningSeeder lot seeding, ValetSpawner, LotBounds; title lot picker; Editor scene-per-lot builder; Data/Run assets; two Midgard lot scenes; tests), docs/changelogs/lead.md
Why: story epic 79, task 80. Open points reported by the agent: crazy waiting cap 4 is clamped to the 3 queue slots; the arrival budget caps the crazy lot near 16 cars; layout parameters are only a kind (Diner) for now; nothing publishes LotCleared/LotFailed/PowerGranted yet (tasks 81, 83, 84).
Tests: 1361 of 1361 EditMode, 31 of 31 PlayMode after the merge and RunBatch (scene fileID churn reverted to the merged versions).
