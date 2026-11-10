# lot-v2 changelog

## lot-v2: rebuild lot to the sketch, road below, new routes, retune
Date: 2026-11-10
Files: Assets/_Project/Scripts/Lot/ (BayPlan, SiteLayout, RoadLayout, LotDinerArt new; LotConfig, LotGeometry, LotFrame, LotExpander,
LotLayoutBuilder, LotDecor, DecorLayout, WalkwayLayout, WalkwayStyle, LotWalkwayArt, PedestrianPathMap, LotArtConfig, ArrivalQueue changed;
LotFrameRule removed), Scripts/Patrons/ (ArrivalReach new, ArrivalReservations, PatronConfig), Scripts/Core/ (TuningCatalog, ShiftConfig,
IRoadLayout.cs.meta), Scripts/Director/ (DirectorBrain, DilemmaRuleSettings, LastBayRule), Scripts/Bootstrap/ (GameBootstrap, CameraFollow),
Editor/Art/LotArtBuilder.cs, Data/Lot/*.asset, Data/Patrons/PatronConfig.asset, Data/Shift/ShiftConfig.asset, Scenes/Bootstrap.unity (camera max zoom),
Tests/EditMode/ Lot, Patrons, Director, Integration.
Why: docs/redesign/lot_v2_layout.md (design.md A5): diner left with side and main doors, curb beside the main door, 2 x 4 bays (6 regular, EV 7,
wash), a column of 3 arrival slots under the curb, a right strip for ambient people, a two-lane road below with IRoadLayout; queue limit 3;
director retuned for one EV bay, six spots, lot tight at 1 free spot.
Tests: new and rewritten EditMode tests (LotGeometry, RoadLayout, PedestrianPathMap, WalkwayLayout, DecorLayout, ArrivalReach, LastBay single bay).
Full EditMode run in Unity 6000.2.5f1 batch mode: 636 passed, 0 failed (includes the director soak test). The Bootstrap scene was regenerated with
ProjectSetup.RunBatch (no errors) and then reverted to the committed scene plus the camera max zoom change, because regeneration only reordered ids.
