---
name: integration
description: Wires Uncanny Valet together - the Bootstrap scene, service registration, camera, debug overlay, Web build settings, CI workflow and tools. Use for Assets/_Project/Scripts/Bootstrap/, Scripts/UI/, Scenes/, Editor/, .github/ and tools/.
---

You are the integration agent. Read CLAUDE.md and docs/design.md first (sections 9 and 10).

## Own
`Assets/_Project/Scripts/Bootstrap/`, `Assets/_Project/Scripts/UI/`, `Assets/_Project/Scenes/`,
`Assets/_Project/Editor/`, `.github/`, `tools/`, `docs/changelogs/integration.md`.
Branch `agent/integration`, own worktree. Starts in P2, once the module branches are merged.

## Build (P2)
- `GameBootstrap` (MonoBehaviour): creates the `EventBus`, registers every service in `Services` in a
  fixed order, builds the lot, spawns the valet at `ILotLayout.ValetStartPoint`, starts night 1.
  `Services.Reset()` on destroy. This is the only place concrete types meet.
- `CameraFollow`: follows the valet, or the car he's driving; pixel-perfect friendly orthographic size.
- `DebugOverlay` (IMGUI `OnGUI`, no UI assets): clock, debt left, tips tonight, open tickets with state,
  the current interact prompt, and a "pickup requested" highlight.
- `Assets/_Project/Scenes/Bootstrap.unity` holding one GameObject with `GameBootstrap`; add it to Build
  Settings as scene 0 and commit `ProjectSettings/EditorBuildSettings.asset`.
- Keep `Editor/WebBuild.cs`, `tools/build-web.ps1` and `.github/workflows/ci-cd.yml` working; run the build
  script locally and check the build loads in a browser before asking qa to review. Do not commit `site/`;
  the lead does that at publish points (CI only publishes the committed `site/`).

## Done when
- The P1 loop in design.md section 10 plays end to end in a local Web build.
- CI's changelog check passes on a pull request; a push to `main` publishes the committed `site/` to GitHub Pages.

## Out of scope
Final UI, menus, phone UI, art.
