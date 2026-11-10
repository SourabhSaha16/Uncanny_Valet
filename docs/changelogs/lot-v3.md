# lot-v3 changelog

## lot-v3: patrons face the curb zone while waiting
Date: 2026-11-10
Files: Scripts/Core/Contracts/IPedestrianPaths.cs (CurbCenter), Scripts/Lot/PedestrianPathMap.cs, Scripts/Pedestrians/{WalkAnimMath,PedestrianWalker,PatronWalkers}.cs, Tests/EditMode/Pedestrians/WalkAnimMathTests.cs
Why: human play test (design.md A5): a patron arriving at the curb stood facing down. The pure rule WalkAnimMath.FacingToward turns a standing patron toward the curb zone centre (the valet pickup spot) the moment the walk ends.
Tests: WalkAnimMathTests.FacingTowardTurnsAPatronToTheCurbZone added; full run at the end of the lot-v3 series.

## lot-v3: label the curb zone VALET PICKUP
Date: 2026-11-10
Files: Scripts/Lot/{StackedText,LotCurbArt,LotLayoutBuilder,LotConfig,LotArtConfig}.cs, Data/Lot/LotArtConfig.asset, Tests/EditMode/Lot/StackedTextTests.cs
Why: human play test (design.md A5): the curb text CURB becomes VALET PICKUP, each word stacked vertically in its own column (VALET left, PICKUP right) so 6 letters fit the 5-unit curb; the plate behind it is widened to cover both columns. Words, size and column spacing are LotConfig fields.
Tests: StackedTextTests added (stacking, column offsets).

## lot-v3: dotted slot boxes replace the arrival arrows
Date: 2026-11-10
Files: Scripts/Lot/{DashedOutline,LotQueueArt,LotArrowMarks,LotLayoutBuilder,WalkwayStyle}.cs, Data/Lot/LotArtConfig.asset, Tests/EditMode/Lot/DashedOutlineTests.cs
Why: human play test (design.md A5, sketch lot_v2_sketch.png): the arrival column shows three dotted outline boxes, one per car slot (nose-up car footprint), instead of up arrows. Dash, gap, line and inset are WalkwayStyle fields; the dash layout is a pure tested class.
Tests: DashedOutlineTests added (dashes inside the box, edges start and end on a dash, gap size, short run).

## lot-v3: crosswalks become white zebra stripes
Date: 2026-11-10
Files: Scripts/Lot/{ZebraStripes,WalkwayLayout,LotWalkwayArt,WalkwayStyle}.cs, Data/Lot/LotArtConfig.asset, Tests/EditMode/Lot/ZebraStripesTests.cs
Why: human play test (design.md A5): crosswalks read as grey boxes. Every crosswalk (side-door leg, north, aisle, corridor/south) is now white zebra bars across the walking direction, painted on the asphalt. WalkwayLayout.Crosswalks is the non-overlapping cut of the bands for painting (Bands is unchanged for the walking routes). The old band fill and edge lines are gone.
Tests: ZebraStripesTests added (bar orientation, count, centring, gap, no overlapping crosswalks).

## lot-v3: real diner building from MetroCity sprites
Date: 2026-11-10
Files: Scripts/Lot/{DinerSlices,LotDinerArt,LotArtConfig}.cs, Editor/Art/LotArtBuilder.cs, Data/Lot/LotArtConfig.asset, Tests/EditMode/Lot/DinerSlicesTests.cs
Why: human play test (design.md A5): the green DINER rectangle is replaced by a building cut from the tall MetroCity sprite Environment/Stations/Building.png (found in the sheets; the tan green-roof house is not tall enough): a tiled red roof, a front face of windows along the bottom, a striped awning over each door, the two doors (still at the pedestrian door points) and a small DINER sign. Slice rectangles, band count and awning/sign values are LotArtConfig fields; the slices are cut at runtime with Sprite.Create (tiled draw mode, point filtered). The old wall/roof colour fields are removed.
Tests: DinerSlicesTests added (texture rect flip, pixels per unit, whole facade bands). ProjectSetup.RunBatch ran clean; scene churn reverted.

## lot-v3: dark asphalt outside the lot instead of grass
Date: 2026-11-10
Files: Scripts/Lot/{DecorLayout,LotDecor,LotArtConfig,SpriteTiler}.cs, Editor/Art/LotArtBuilder.cs, Data/Lot/LotArtConfig.asset, Tests/EditMode/Lot/DecorLayoutTests.cs
Why: human play test (design.md A5): the ground outside the lot (left, right, top, around the bus stop strip and the road) is now darker asphalt (LotArtConfig.OutsideColor, 0.36) instead of grass. The road keeps the lot asphalt (0.53) with its dashed lane line and edge lines, so it reads as a road and the lot as a lot. Trees and grass tufts are removed (kept clean); the hedge walls stay.
Tests: DecorLayoutTests: outside ground covers lot, road and far sidewalk and reaches past the walls; the tree and tuft tests are removed with the code.

## lot-v3: painted arrows along the car exit route
Date: 2026-11-10
Files: Scripts/Lot/{RouteMark,RouteMarks,LotRouteArrows,LotCurbArt,LotLayoutBuilder,WalkwayStyle}.cs (LotArrowMarks removed), Data/Lot/LotArtConfig.asset, Tests/EditMode/Lot/RouteMarksTests.cs
Why: human play test (design.md A5): the curb's two down arrows are gone. Small outlined arrows are painted every 2.4 units along RoadLayout.CurbExitRoute (leaving the curb zone, down the corridor, round the turn east onto the road), each pointing along the direction of travel there, so the valet can keep clear of cars driving off. Spacing, size and start distance are WalkwayStyle fields; the mark placement is a pure tested class.
Tests: RouteMarksTests added (spacing, carry round a corner, no spacing, the real curb exit route runs down the corridor and turns east).

## lot-v3: entry and exit car speed in the F2 panel
Date: 2026-11-10
Files: Scripts/Core/Events/TuningKey.cs, Scripts/Core/Contracts/TuningCatalog.cs, Scripts/Bootstrap/{TuningSeeder,TrafficTuningLink,GameBootstrap}.cs, Scripts/Traffic/{TripSpeeds,TripContext,EntryTrip,ExitTrip,TrafficModule}.cs, Tests/EditMode/{Traffic/TripTests,Integration/TrafficTuningLinkTests,Integration/TuningSeederTests}.cs, Tests/EditMode/Lot/ZebraStripesTests.cs (test fix)
Why: human play test (design.md A5): TrafficEntrySpeed and TrafficExitSpeed appear in the F2 panel (group Traffic), seeded from TrafficConfig (cruise speed and curb route speed) and forwarded by TrafficTuningLink to TrafficModule. TripSpeeds turns them into live scales: entry scales the lane cruise and the turn into the queue, exit scales the curb route and the lane after merging; the config values give scale 1, so behaviour at the defaults is unchanged. TrafficSpeedScale (ambient) is kept.
Tests: TripTests (raised or lowered entry and exit speeds change the trip time and the turn speed; they do not affect each other; scale maths), TrafficTuningLinkTests (forwarding, null setters), TuningSeederTests (seeding, plus the existing every-key test). Full EditMode run in Unity 6000.2.5f1 batch mode after the whole lot-v3 series: 707 passed, 0 failed. ProjectSetup.RunBatch ran clean (scene and art meta churn reverted; LotArtConfig.asset kept).
