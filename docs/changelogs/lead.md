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
