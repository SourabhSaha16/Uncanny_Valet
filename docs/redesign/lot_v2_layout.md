# Lot v2: layout, road and traffic (human sketch, 2026-11-10)

Sketch: `docs/redesign/lot_v2_sketch.png` (the human's wireframe; it is binding for placement, not for exact pixels).
Binding. Where this conflicts with design.md Part A, this wins; the lead folds it into Part A when merged.

## Layout (screen, left to right, top to bottom)

- **Game walls** frame the whole play area. A **right strip** inside the walls is the outside road and bus stop;
  it is where ambient pedestrians appear and disappear (no longer from the street edges).
- **Diner**: one tall MetroCity building filling the left side. Two doors on its right face:
  - **Side door** (upper): ambient pedestrian door. Ambient walkers go road strip -> crosswalks -> side door, vanish,
    come back out of it and walk back. A crosswalk runs from the side door east then south to the main aisle.
  - **Main door** (middle): patron door. Patrons use it and nothing else.
- **Curb**: a tall zone just right of the main door (waiting spots for patrons, the delivery zone). Painted down arrow
  on the aisle side next to it. Delivery rule unchanged: the car must face down (80 degrees).
- **Bays**: two rows of 4 tall (vertical) bays separated by a wide horizontal aisle that carries a long crosswalk
  across its width toward the right strip.
  - Top row, left to right: LOT, LOT, LOT, LOT (regular spots).
  - Bottom row, left to right: EV, LOT, WASH, LOT. (Total: 6 regular spots, 1 EV bay, 1 wash.)
  - Spot numbering: regular spots 1..6 in reading order from the curb side (top row first, then bottom row skipping
    the EV and wash); the EV bay is id 7. Keep the plug icon under the EV number and the wash icon on the wash.
- **Arrival queue**: 3 slots stacked in a column below the curb (a column of 3 dashed car slots, nose up), with an
  open crossing area to their right. The queue limit becomes **3** (was 4): `PlanMaxWaitingCars` max 3, default 3.
- Crosswalks (dashed in the sketch): side door -> east -> south to the aisle crossing; the long aisle crossing to the
  right strip; the open crossing right of the queue slots; walkers never cross cars' parked footprints.

## Road below the lot (new)

A two-lane road runs along the bottom, outside the lot walls (the camera frame grows downward to include it):
- **Eastbound lane** (the lane next to the lot) carries patrons' cars: they enter from the left edge driving left to
  right, slow down, turn up into the queue column and stop in a slot. Delivered cars leave the curb, drive down to the
  road, **wait/slow for passing traffic**, merge into the eastbound lane and drive off to the right.
- **Westbound lane** (the far lane) carries ambient cars only (visual traffic), right to left.
- Ambient traffic: random cars on both lanes at a modest rate (F2 key, 0 to disable), same car sprites, no tickets,
  no collision with the valet, never stop in the lot.

## Traffic mechanics (City-Skylines / SimCity style)

Lane following with car following: every traffic car follows its lane polyline, keeps a safe gap to the car in front
(brakes smoothly, speeds up again), and a car that wants to join a lane (queue entrance, curb exit) yields until the
gap on that lane is big enough, then merges. Patron arrivals wait off-screen left until the road is clear enough to
enter, never a collision. Pure rules (gap, speed target, yield decision) are plain C# classes with EditMode tests.
Tunables (speed, gap, yield distance, ambient rate) are a ScriptableObject plus F2 keys.

## Pedestrian changes

- Ambient spawner = right strip (road and bus stop); ambient door = side door. Patron door = main door only.
- `IPedestrianPaths` keeps its shape; routes are re-authored for the new layout.

## Decisions (answered by the human, 2026-11-10)

| Question | Answer |
| --- | --- |
| Bay set | Exactly as drawn: 6 regular, 1 EV, 1 wash |
| Queue slots | 3 |
| Car entry | Keep the direction (queue nose up, delivery faces down); a road below with traffic: patron cars come from the left, turn into the queue; delivered cars drive down, slow and wait for a gap, exit right. Random ambient cars both ways. City-builder traffic mechanics |
| Pedestrians | Only the two points as drawn |

## v3: exit from the top (issue #68, 2026-11-11)

Supersedes the "Road below the lot" and "Pickup arrow" parts above where they conflict.

- **Bottom road = ENTRY**, one lane, one way left to right (the westbound lane is gone). Patron cars still enter from
  the left and turn up into the queue column; ambient cars drive it left to right only. A pixel-style `ENTRY` word is
  painted on the lane (`LotRoadArt`, `PixelFont`; word, pixel size and position are `LotArtConfig` fields).
- **Top road = EXIT**, a new one-lane one-way road above the top wall, same style as the entry road (asphalt, edge
  lines, sidewalk on its far side), left to right, with ambient cars at the same rate and speed
  (`TrafficConfig` ambient keys apply to both; `EastShare` 0.5 splits the rate evenly). `EXIT` is painted on it.
- The lot is taller by `SiteLayout.ExitRunway` (1.5) above the side door. The camera code is unchanged: it still frames
  the lot, extended down to the entry road sidewalk; the exit road lies above the frame and may be partly out of view.
- **Drop-off faces up.** `CurbFacingRule.RequiredDegrees` is 0 (nose up), tolerance unchanged (80 degrees; the F2 Perfect
  drop-off angle is measured from up). The curb arrows point up. Patron walk and boarding are unchanged.
- **Exit route.** A delivered car leaves the curb straight up through a gap in the top wall directly above the curb
  (`LotGeometry.ExitGap`, one car wide plus padding), waits at the merge point for a gap on the exit road (same
  `MergeGapRule` as the old curb exit, now on `TrafficLane.Top`), turns right and leaves along the exit road.
  `IRoadLayout.WestLane` became `TopLane`; `CurbExitRoute` and `CurbMergeDistance` now describe this route.
- The south wall opening shrank to just the queue column (the old exit corridor is gone) and the old painted exit-route
  arrows were removed; the ENTRY and EXIT words replace them.
