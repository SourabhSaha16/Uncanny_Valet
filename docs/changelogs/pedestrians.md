# Pedestrians changelog

Entries are added by the pedestrians agent, one per commit. Format in docs/changelogs/README.md.

## pedestrians: only the driven car knocks people, walkers keep clear of cars, Refs #192 [Claude]
Date: 2026-11-18
Files: Assets/_Project/Scripts/Pedestrians/{PedestrianWalker,WalkerBody,PathFollower,PedestrianCrowd,PedestrianConfig,WanderPicker,KeepOutAreas}.cs, new {PedestrianHitRule,WalkerSolidRule,ILegBlocker,LegBlockers,CarLegProbe}.cs, Data/Pedestrians/PedestrianConfig.asset (_lookAhead 0.35 to 0.55), Tests/EditMode/Pedestrians/PedestrianCarContactTests.cs, Tests/PlayMode/PedestrianHitTests.cs
Why: issue #192. Causes: (1) OnCollisionEnter2D knocked a person for any dynamic car, and the walker's own walking speed counted in the hit speed; now only a car with HasDriver knocks or bounces (PedestrianHitRule), speed minus own walking speed. (2) KeepOutAreas was never filled, so roam legs crossed bays; roam legs are now also rejected by a body-wide CarLegProbe sweep (ILegBlocker, LegBlockers). (3) The look-ahead cast only along the current heading; it now aims at PathFollower.PointAhead so it sees round route corners, and LookAhead is 0.55. (4) After passing through a blocker the collider turned solid inside the car; WalkerBody.SetSolid now stays off until clear of cars (WalkerSolidRule).
Tests: PedestrianCarContactTests (7 EditMode), PlayMode UndrivenCar_NeverKnocksOrBouncesAWalker; EditMode 2106 passed, PedestrianHitTests PlayMode 4 passed.

## pedestrians: collision, wait-and-resume, rubber bounce and bump sound
Date: 2026-11-10
Files: Assets/_Project/Scripts/Pedestrians/{PedestrianWalker,PedestrianCrowd,PedestrianConfig,PathFollower,PedestrianBlockRule,PedestrianBlockState,PedestrianBounceRule,WalkerBody,WalkerSensor}.cs, Assets/_Project/Scripts/Core/PhysicsLayers.cs, Assets/_Project/Scripts/Core/Events/PedestrianEvents.cs, Assets/_Project/Scripts/Patrons/ArrivalSlotScanner.cs, Assets/_Project/Scripts/Audio/{AudioDirector,SfxAlerts,SfxEventRouter,SfxId,SfxRecipeBook,SfxRateLimiter}.cs, ProjectSettings/TagManager.asset, Assets/_Project/Tests/EditMode/Pedestrians/PedestrianBlockRuleTests.cs, Assets/_Project/Tests/EditMode/Audio/SfxTests.cs
Why: design.md Part A (pedestrians). Walkers get a kinematic body and circle collider on layer Pedestrian (8). They look ahead, wait when blocked, then pass through after a timeout so no route freezes. A car or the valet bumping one gives a tiny rubber-ball bounce and a PedestrianBumped event, which plays a short boing. The queue slot scan ignores the Pedestrian layer. Core event and Audio edits were a one-off exception granted by the lead.
Tests: 12 new EditMode tests (block rule, bounce rule, router bump and rate limit); full suite 792 passed, 0 failed.

## pedestrians: patrons ignore their own car (no push, wait or bounce)
Date: 2026-11-10
Files: Assets/_Project/Scripts/Pedestrians/{OwnCarRule,WalkerSprites,WalkerBody,WalkerSensor,PedestrianWalker,PatronWalkers}.cs, Assets/_Project/Tests/EditMode/Pedestrians/PedestrianBlockRuleTests.cs
Why: play-test bug: a patron stepping out of the car pushed it sideways. A patron now ignores the colliders of the car tied to their ticket (Physics2D.IgnoreCollision, re-applied when the collider or object is switched back on, released on despawn), the look-ahead skips that car, and a touch from it gives no bounce and no PedestrianBumped. Other cars and the valet behave as before. Sprite layers moved to WalkerSprites to keep PedestrianWalker under 200 lines.
Tests: 2 new EditMode tests for OwnCarRule (own car, other car, ambient person with no car); full suite 813 passed, 0 failed.

## powers: shared power bar of three buckets, remove speed boost and Park Smash [Claude]
Date: 2026-11-13
Files: Pedestrians: WalkerStumbler (no bus; TryStart for the knockback power), PedestrianWalker, PedestrianConfig, PedestrianStumbleRule comments.
Why: Barge-specific listening removed; the stumble itself stays for task #93 (issue #86 decision 9). issue #86 of epic #79 (Valet to Valhalla).
Tests: StumbleTests updated; suites pass.

## pedestrians: knocked pedestrians walk on from where they land [Claude]
Date: 2026-11-14
Files: Assets/_Project/Scripts/Pedestrians/{PedestrianBounceRule,PedestrianStumbleRule,PathFollower,PedestrianWalker}.cs, Assets/_Project/Tests/EditMode/Pedestrians/{PedestrianBlockRuleTests,StumbleTests,PedestrianKnockbackTests,PathFollowerTests}.cs
Why: issue #125. The bounce and stumble offsets used to ease back to zero against a frozen route point, so a pushed pedestrian snapped back to the crosswalk like a rubber band. Replaced on purpose: PedestrianBounceRule.Shape (peak and return) is now Slide (ease-out that lands and stays), the offset stays at the landing spot, and PathFollower.Displace re-points the walk straight at its destination from there. Shape and its test were deleted (no other users).
Tests: new PathFollower Displace tests (off-route and standing); bounce, stumble and knockback rule tests changed to expect the landing offset; EditMode suite run.

## pedestrians: free direct routes to the diner on crazy lots [Claude]
Date: 2026-11-14
Files: Assets/_Project/Scripts/Pedestrians/{WalkerRouteRule,WalkerRouter,PedestrianCrowd}.cs, Core/Events/TuningKey.cs (append PedestrianFreeRoutes), Core/Contracts/PedestrianHitRows.cs, Run/LotDefinition.cs (_freePedestrianRoutes), Bootstrap/TuningSeeder.cs, Editor/{RunAssetSetup,VanaheimWorldSetup}.cs, Data/Run/{Midgard,Vanaheim}CrazyLot.asset, Tests/EditMode/Pedestrians/WalkerRouteRuleTests.cs, Tests/EditMode/Run/{LotSeedingTests,FreePedestrianRoutesAssetTests}.cs
Why: issue #124. On crazy lots pedestrians queued behind each other at the crosswalk. A per-lot LotDefinition switch seeds the new tuning key PedestrianFreeRoutes (appended at the end of TuningKey); the pure WalkerRouteRule keeps only the first and last point of a road or to-door route when it is on, so people walk the straight line to the diner. Training lots keep the crosswalks. Additive Core change: one TuningKey, one row.
Tests: WalkerRouteRuleTests (4), LotSeedingTests (1), FreePedestrianRoutesAssetTests (4 lots).

## pedestrians: ambient people roam, then visit the diner at random [Claude]
Date: 2026-11-14
Files: Assets/_Project/Scripts/Pedestrians/{PedestrianBrain,PedestrianPhase,PedestrianConfig,PedestrianCrowd,PedestrianWalker,WalkerRouter}.cs, new {RoamPath,KeepOutAreas,WanderPicker,EnterInterval}.cs, Tests/EditMode/Pedestrians/{PedestrianBrainTests,WanderPickerTests,EnterIntervalTests}.cs, Tests/PlayMode/PedestrianHitTests.cs
Why: issue #130. All ambient people walked straight to the one diner door. Now each appears at the road side and roams along its sidewalk/crosswalk route (random spots, never a spot inside a KeepOutAreas rectangle, legs that cross one are rejected); after a random PedestrianConfig.RoamSeconds interval (new, min/max) it finishes the leg, walks to the door, goes in, comes out later and roams on. The off-screen Leaving walk of ambient people is gone. Patron walkers are untouched. KeepOutAreas is pure but nothing fills it yet: no Core contract exposes spot rectangles (needs a Lot-side addition, asked of the lead). PedestrianConfig.asset needs no change (new fields use their defaults).
Tests: WanderPickerTests (7: targets and legs never in a keep-out rectangle, min leg, determinism, slicing), EnterIntervalTests (3: range, variation, walkers not all due together), PedestrianBrainTests updated (roam, enter, come out, vary). Full EditMode suite: 1822 passed, 0 failed.
