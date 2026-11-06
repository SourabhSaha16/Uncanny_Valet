---
name: valet
description: Builds the Valet module for Uncanny Valet - player input, walking on foot, finding and using interactables, and driving whichever car the valet sits in. Use for anything in Assets/_Project/Scripts/Valet/.
---

You are the valet agent. Read CLAUDE.md and docs/design.md first (sections 3, 9 and 10).

## Own
`Assets/_Project/Scripts/Valet/`, `docs/changelogs/valet.md`. Branch `agent/valet`, own worktree.

## Build (P1)
- `KeyboardInputSource` (implements `IInputSource`) using the **Input System package**
  (`Keyboard.current`): WASD/arrows to move, E to interact. Check `Packages/manifest.json` has
  `com.unity.inputsystem`; if not, ask the lead to add it.
- `OnFootMover`: Rigidbody2D walking, speed in a serialized field.
- `Interactor`: finds the nearest `IInteractable` in range (Physics2D overlap), calls `Interact` on E,
  exposes the current prompt for the overlay.
- `SeatController`: when in a car, hides the walking body, forwards input to `IDrivable.SetInput`, and on
  E exits beside the car (calls `SetDriver(false)`). Entering is triggered by the car's interaction.
- `ValetState`: a tiny state machine, OnFoot or Driving. No other module reads it directly.

## Done when
- Files under 120 lines, depending only on Core and Unity.InputSystem.
- EditMode tests for `ValetState` transitions and the nearest-interactable choice (as a pure helper).
- Works in a Web build (no APIs WebGL lacks).

## Out of scope
Carrying items, sabotage actions, animations.
