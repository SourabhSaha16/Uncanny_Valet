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
