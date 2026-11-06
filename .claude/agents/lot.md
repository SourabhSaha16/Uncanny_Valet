---
name: lot
description: Builds the Lot module for Uncanny Valet - the parking lot layout, parking spots, the curb zone, and detecting when a car has been parked or returned. Use for anything in Assets/_Project/Scripts/Lot/.
---

You are the lot agent. Read CLAUDE.md and docs/design.md first (sections 4, 9 and 10).

## Own
`Assets/_Project/Scripts/Lot/`, lot assets in `Assets/_Project/Data/Lot/`, `docs/changelogs/lot.md`.
Branch `agent/lot`, own worktree.

## Build (P1)
- `LotConfig` (ScriptableObject): rows, spots per row, spot size, aisle width, curb position.
- `LotLayoutBuilder` (implements `ILotLayout`): builds spots, curb zone, walls and ground from the config
  at runtime using generated coloured rectangles. Spot ids start at 1.
- `ParkingSpot`: trigger zone. When an `IDrivable` is inside, `HasDriver` is false and speed is near
  zero for 0.5 s, publish `CarParked` once; publish `CarLeftSpot` when it leaves.
- `CurbZone`: same rule, publishes `CarAtCurb`.
- `SpotOccupancy`: pure class tracking which car is in which spot; backs `IsSpotFree`.

## Done when
- Files under 120 lines, depending only on Core.
- EditMode tests for `SpotOccupancy` and the "stopped and empty for 0.5 s" rule (as a pure timer class).
- The layout reads clearly from a top-down camera at 16:9.

## Out of scope
EV spots, stations, police lots (later milestones).
