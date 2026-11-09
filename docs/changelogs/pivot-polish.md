# Pivot polish changelog

Entries are added by the pivot-polish agent, one per commit. Format in docs/changelogs/README.md.
Source: docs/redesign/ui_style_guide.md, "Revision 1" (binding).

## pivot-polish: tag colours, flash, info box, zoom, pedestrians
Date: 2026-11-09
Files:
- Core: added Scripts/Core/Config/TagPalette.cs (12 colours); Events/CarEvents.cs (CarLetterAssigned replaced by CarTagAssigned; new CarArrived).
- Patrons: LetterPool replaced by TagPool; Ticket.Letter -> Ticket.Tag; TicketBook/TicketRegistry publish CarTagAssigned.
- Cars: removed CarLabel, CarLabelMath; added CarKeyTag, CarFlashConfig, FlashLevel, PickupFlashMath; CarSorting (Label out, Outline/TagBacking/Tag in); CarFactory (flash config, key tag instead of label); CarAutoDrive publishes CarArrived when the drive-in ends.
- UI: TicketRow (Tag, Arrived, Clean01, meter helpers), TicketBoard (tag, arrival, clean progress), PickupBannerFormatter (no letter; AppendAll), Hud/{WorldTicketLayout,WorldTicketPainter,KeyBoardPainter,NoticePainter}, Kit/PixelPalette (TagColor, Mud), GameUi.
- Lot: added WalkwayStyle, WalkwayLayout, PaintedArrow, LotWalkwayArt; LotArtConfig (walkway style, camera view shares); LotDecor (aisle crossings and arrows, tighter ExtendView, WalkRoutes).
- Pedestrians (new assembly UncannyValet.Pedestrians, references Core only): PedestrianConfig, PedestrianCrowd, PedestrianWalker, PedestrianPlan, PedestrianLook, RouteWalker, WalkAnimMath, WalkFacing.
- Bootstrap: GameBootstrap (starts the crowd with LotDecor.WalkRoutes), CameraFollow (max zoom 16 -> 12.5, margin 2 -> 1); asmdef references.
- Editor: BootstrapSceneBuilder (CarFlashConfig asset, Pedestrians object), Art/PedestrianArtBuilder; Editor and test asmdefs reference Pedestrians.
- Tests: added Patrons/TagPoolTests, Cars/PickupFlashMathTests, Lot/WalkwayLayoutTests, Pedestrians/{RouteWalker,PedestrianPlan,WalkAnimMath}Tests; updated TicketRegistry*Tests, TicketBoardTests, PickupBannerTests, HudRulesTests; removed LetterPoolTests, CarLabelMathTests.
Why: ui_style_guide.md Revision 1. Cars carry no letters: each open ticket gets the lowest unused colour of a 12-colour palette, drawn as a roof stripe on the car, on the floating info box, the key board row and the pickup banner. A requested pickup flashes the stripe and a ring around the car, 1 Hz rising to 6 Hz with the elapsed patience (real time), calm cyan, amber from 60% and orange-red from 85% or when expired (all in CarFlashConfig). The info box shows only needs (dirt meter that drains while washing, charge meter that fills and turns amber at full) and only after CarArrived until CarDeparted/ticket close. Camera zoom is about 22% closer. Pedestrians walk back and forth on routes computed by the lot (aisle crossings, the road crosswalk, the north sidewalk); scenery only, no collider.
Tests: new EditMode fixtures listed above. NOT run: Unity could not be launched in this sandbox, so compilation and the suite are unverified. New .meta files (Pedestrians folder, asmdef, scripts, tests) are not committed; Unity creates them on first import.
