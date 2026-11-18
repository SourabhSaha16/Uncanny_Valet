# Hand-made lots: implementation plan (#194)

Decisions (2026-11-18, see docs/decisions.md): alongside the generated lots, sandbox first; Tilemap ground + free scenery prefabs; placeable bays, curb and queue slots, car entry and exit roads, pedestrian layout, solid walls, hazard patches; editor validator that blocks play.

## Approach
Nothing outside Lot reads `LotGeometry`. Consumers take `ILotLayout`, `ISpotGeometry`, `IRoadLayout`, `IPedestrianPaths`, `IMudPatches`. A hand-made lot only supplies new implementations of these; Core, Patrons, Traffic, Pedestrians, Valet, Cars, Director and UI do not change. Do not retrofit `LotGeometry`.

- `Scripts/Lot/Handmade/` (namespace `UncannyValet.Lot.Handmade`): `HandmadeLotData` (plain DTO), `HandmadeLotReader` (scene markers to data), `HandmadeLotRules` (pure validator), `AuthoredRoadLayout`, `AuthoredPedestrianPaths`, `WalkGraph`, `HandmadeLot` (MonoBehaviour implementing `ILotLayout`, `ISpotGeometry`).
- Markers (prefabs with gizmos): `LotBay` (Regular, EV, Wash; number, size, rotation), `CurbZoneMarker`, `QueueSlot`, `CarEntryRoad`, `CarExitRoad`, `AmbientLane`, `LotDoor`, `PatronWaitPoint`, `AmbientSpawnPoint`, `WalkNode`/`WalkLink`, `Crosswalk`, `SolidBlock`, `HazardPatch`, `ValetStart`.
- Tilemap layers: Ground, Sidewalk, Markings, Building (collider), Hedge (collider), Overhead. Tile palette generated from `Art/ValhallaMidgard/Lot`.
- Bootstrap seam `ILotSource` (Parametric and Handmade); lot registration moved out of `GameBootstrap` (at the line limit) into `LotRegistration`.
- Authoring asset: one prefab per lot in `Assets/_Project/Prefabs/HandmadeLots/`; generated `Lot_*.unity` scenes are overwritten by the builder so they never hold authoring.
- Validator (about 25 rules, each a unit test): window `Window > Uncanny Valet > Lot Validator`, play-mode block, builder abort, and a runtime guard.

## Tasks (each shippable, each with a changelog entry)
1. Authored geometry, pure (lot) + validator rule tests, parity test with the Diner.
2. Per-piece size and rotation for bays and curb (old signatures kept as overloads).
3. Markers, reader, `HandmadeLot`, six marker prefabs, PlayMode drive test.
4. Bootstrap seam `ILotSource` (integration), behaviour-preserving.
5. Editor tooling: tile and prefab generator, validator window, play guard.
6. Sandbox integration (world key 5+).
7. Run integration: `LotLayoutKind.Handmade = 2` (append-only), prefab reference on `LotDefinition`.
8. Sample lot as acceptance artefact.
Order: 1, 2, 3, 4, then 5 and 6 in parallel, 7, 8.

## Risks
Tile size (85x88 px at PPU 22 is not square); curb and queue direction locked to unrotated in v1; entry and exit route quality; pedestrian graph vs free routes; sandbox runs no shift; `LotId` slot for a playable run entry; Run/Traffic/Pedestrians ownership missing from the CLAUDE.md table.

## Detail: hard-coded assumptions in Lot today (must be parametrised or locked)
- Every bay is `LotConfig.SpotSize` (`ParkingSpot.Init`, `StationZone.Init`, `SpotBuilder.InnerSize`, `BayFitRule`). Curb uses `config.CurbSize` and `LotCurbArrows.SpanOf(config)`. `CurbFacingRule.RequiredDirection` is "nose up". `ILotLayout.ArrivalRotationDegrees` is one value. Bays are numbered 1..n in reading order, EV ids continue after regular ones; `SpotOccupancy.SetActiveSpots(k)` opens regular ids 1..k (the artist's numbering is the night expansion order). `StationSwitch` opens all wash stations together.
- v1 locks: curb unrotated, all queue slots one shared rotation (validator rules 3 and 5).

## Detail: markers (all `[DisallowMultipleComponent]` sealed, `[SerializeField] private`, `[Min]`, `[Tooltip]`, gizmos via OnDrawGizmos)
`HandmadeLotRoot` (PerfectSettings, optional view rect); `LotBay` (Kind Regular/Charging/Wash append-only, number, size default `LotConfig.SpotSize`, rotation = transform z); `CurbZoneMarker` (size default 3x5); `QueueSlot` (index 0 = valet pickup nearest curb, carSize); `CarEntryRoad` (points in travel direction, offscreen start, queue-turn point, bend points to slot 0); `CarExitRoad` (lane points, curb-exit points, merge point); `AmbientLane` (entry-back, exit-back); `LotDoor` (Main/Side); `PatronWaitPoint` (ordered); `AmbientSpawnPoint`; `WalkNode`/`WalkLink` (crosswalk flag); `Crosswalk`; `SolidBlock` (BoxCollider2D, Wall/Hedge); `HazardPatch` (Mud, Circle/Rect); `ValetStart`; optional `ScenerySolid`.

## Detail: validator rules (E blocks play, W warns). Each is a unit test.
1 E one `HandmadeLot` root. 2 E exactly one curb. 3 E curb rotation 0. 4 E queue slots 1..n contiguous unique, count within `LotDefinition` limits. 5 E one shared slot rotation. 6 E slot 0 touches the curb. 7 E regular bay numbers unique contiguous 1..n; EV bay or wash bay present if the definition's modifier chances need them. 8 E no overlaps between bays, slots, curb (rotated-rect SAT). 9 E bay smaller than a car. 10 E entry road present, 2+ points, start off screen. 11 E queue-turn point on the entry lane; bend reaches slot 0. 12 E exit road and curb-exit route start in the curb and end on the exit lane. 13 E car routes do not cross solids. 14 E ambient lanes present when ambient traffic is on. 15 E main door and patron wait points. 16 E ambient spawn points when ambient pedestrians > 0. 17 E walk graph connected: spawn, wait points, bays, slots reach the main door. 18 E walk links do not cross solids. 19 E valet start present, not inside a solid. 20 E hazard size > 0. 21 W scenery overlaps a driving route without collider. 22 W view smaller than the gameplay markers. 23 W Building or Hedge tilemap lacks a collider. 25 E `Handmade` layout kind has no prefab reference.
Editor window `Window > Uncanny Valet > Lot Validator` (double-click selects and frames; Auto-link walk nodes; Frame all); `[InitializeOnLoad]` play guard on ExitingEditMode; the same check in the scene builders and a runtime guard in `HandmadeLot.Awake`. An EditMode test loads every hand-made prefab asset and asserts zero errors.

## Owner decisions (2026-11-18)
- Re-slice the Valhalla tiles to square 4x4-unit tiles for the Tilemap palette (88x88 px at PPU 22); existing sandbox sprites stay.
- Sandbox verifies layout, driving, bays, curb and camera. Patrons, traffic and pedestrians are verified in the real run.
- First real-run appearance: replace Midgard Training with the hand-made lot on `pivot/chaos` only (after promoting the current `pivot/chaos` to `main`, done 22d1a3f).
