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
