# Pedestrians changelog

Entries are added by the pedestrians agent, one per commit. Format in docs/changelogs/README.md.

## pedestrians: collision, wait-and-resume, rubber bounce and bump sound
Date: 2026-11-10
Files: Assets/_Project/Scripts/Pedestrians/{PedestrianWalker,PedestrianCrowd,PedestrianConfig,PathFollower,PedestrianBlockRule,PedestrianBlockState,PedestrianBounceRule,WalkerBody,WalkerSensor}.cs, Assets/_Project/Scripts/Core/PhysicsLayers.cs, Assets/_Project/Scripts/Core/Events/PedestrianEvents.cs, Assets/_Project/Scripts/Patrons/ArrivalSlotScanner.cs, Assets/_Project/Scripts/Audio/{AudioDirector,SfxAlerts,SfxEventRouter,SfxId,SfxRecipeBook,SfxRateLimiter}.cs, ProjectSettings/TagManager.asset, Assets/_Project/Tests/EditMode/Pedestrians/PedestrianBlockRuleTests.cs, Assets/_Project/Tests/EditMode/Audio/SfxTests.cs
Why: design.md Part A (pedestrians). Walkers get a kinematic body and circle collider on layer Pedestrian (8). They look ahead, wait when blocked, then pass through after a timeout so no route freezes. A car or the valet bumping one gives a tiny rubber-ball bounce and a PedestrianBumped event, which plays a short boing. The queue slot scan ignores the Pedestrian layer. Core event and Audio edits were a one-off exception granted by the lead.
Tests: 12 new EditMode tests (block rule, bounce rule, router bump and rate limit); full suite 792 passed, 0 failed.
