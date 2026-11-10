# Pivot stations changelog

Source: the human's play-test notes (lead brief), docs/design.md A2, A4, A5, A10 and docs/redesign/ui_style_guide.md Revisions 4 and 5.

## pivot-stations: stations in the spot grid, smaller HUD trackers, wash icon
Date: 2026-11-09
Files:
- Core: ILotLayout (RegularSpotCount, ChargingSpotCount added), WashIconArt (new shared bitmap), TuningCatalog (open spots range 2-9), ShiftConfig (tooltip).
- Lot: LotGeometry (grid cells: 2 charging bays, the wash, then 9 regular spots; FrameFor(columns), ColumnsFor counts the station cells), LotFrameRule and LotExpander (no station extras), LotLayoutBuilder (counts), LotConfig (station size/gap/first-centre removed: stations use the spot size), StationBuilder (wash icon, blocker sized to the cell), StationZone, PixelDecal (new, replaces PlugDecal), SpotBuilder, WalkwayLayout and WalkwayStyle (GapEntry: the crossing between the first bay columns stops at the aisle edge).
- UI: HudLayout (new), HudPainter, ComboWobble (amplitude 2), PixelIconData (wash bitmap from WashIconArt).
- Bootstrap: DirectorBootstrap (regular/charging counts from the lot), GameBootstrap (spot seed 9).
- Tests: LotGeometryTests, LotExpansionTests, WalkwayLayoutTests, LabelFitTests, LotStateTrackerTests (9 regular spots), TrackerTestBase, TuningSeederTests, new HudLayoutTests.
- Docs: ui_style_guide.md Revision 5.
Why: Part A2/A4/A5/A10 play-test changes: the wash slot shows a wash icon, the star tracker and combo badge are about half size, and the EV bays and the wash slot move into the regular spot grid (nine regular spots, ids 1-9; bays 10 and 11).
Tests: NOT run: Unity could not be launched in this sandbox, so compilation and the suite are unverified. New .meta files are not committed.
