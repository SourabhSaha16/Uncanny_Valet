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
