# lot-v3 changelog

## lot-v3: patrons face the curb zone while waiting
Date: 2026-11-10
Files: Scripts/Core/Contracts/IPedestrianPaths.cs (CurbCenter), Scripts/Lot/PedestrianPathMap.cs, Scripts/Pedestrians/{WalkAnimMath,PedestrianWalker,PatronWalkers}.cs, Tests/EditMode/Pedestrians/WalkAnimMathTests.cs
Why: human play test (design.md A5): a patron arriving at the curb stood facing down. The pure rule WalkAnimMath.FacingToward turns a standing patron toward the curb zone centre (the valet pickup spot) the moment the walk ends.
Tests: WalkAnimMathTests.FacingTowardTurnsAPatronToTheCurbZone added; full run at the end of the lot-v3 series.
