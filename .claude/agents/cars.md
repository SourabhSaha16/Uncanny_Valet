---
name: cars
description: Builds the Cars module for Uncanny Valet - top-down car movement, car identity, the car factory, and entering/exiting cars. Use for anything in Assets/_Project/Scripts/Cars/.
---

You are the cars agent. Read CLAUDE.md and docs/design.md first (sections 8, 9 and 10).

## Own
`Assets/_Project/Scripts/Cars/`, car assets in `Assets/_Project/Data/Cars/`, `docs/changelogs/cars.md`.
Work on branch `agent/cars` in your own worktree.

## Build (P1)
- `CarDefinition` (ScriptableObject): top speed, acceleration, turn rate, grip, braking, size, colour.
- `CarDriver` (MonoBehaviour, implements `IDrivable`): Rigidbody2D top-down movement with gravity 0;
  throttle accelerates along `transform.up`, steering scales with speed, sideways velocity is damped by
  grip, the car slows to a stop with no driver.
- `CarInteraction` (implements `IInteractable`): prompt "Enter car" while the valet is near and nobody is
  driving; tells the valet to sit through an event, never by referencing Valet code.
- `CarFactory` (MonoBehaviour, implements `ICarFactory`): builds a car GameObject from a definition with a
  generated coloured rectangle sprite, BoxCollider2D and Rigidbody2D; publishes `CarSpawned` only if the
  caller asks (patrons own tickets).
- Publish `CarEntered` / `CarExited` when `SetDriver` changes.

## Done when
- Each file is under 120 lines and depends only on `UncannyValet.Core`.
- EditMode tests cover the pure parts (e.g. a `CarPhysicsMath` class for steering and grip).
- A car spawned in an empty scene drives with keyboard input fed to `SetInput`.

## Out of scope
Faults, fuel, damage, boot, stations (later milestones).
