# Changelog: pedestrians-patrons

## pedestrians: size the patron pool by spots plus queue slots [Claude]
Date: 2026-11-14
Files: Assets/_Project/Scripts/Pedestrians/{PatronPoolSizing,PatronPool,PendingStepOuts,PatronWalkers,PedestrianCrowd,PedestrianConfig}.cs, Assets/_Project/Tests/EditMode/Pedestrians/PatronPoolTests.cs
Why: issue #128. The reserved patron pool was a fixed 6, so a 7th open ticket got no walker, PatronEnteredDiner never fired and the stay timer stalled. The pool is now max(PatronPoolSize floor, regular + charging spots + arrival queue slots), read from ILotLayout (no Core change). If a person is still missing, PatronWalkers logs one error naming the car and defers the step-out (and any summon) until one is released. Ambient people and the patron pool are separate lists.
Tests: PatronPoolTests (sizing rule, every open ticket gets a person, deferred once and served on return, pool never hands out ambient people); EditMode suite run.
