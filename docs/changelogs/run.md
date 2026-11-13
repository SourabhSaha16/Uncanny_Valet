# Run module changelog

## integration: RunProgress, LotDefinition, lot scenes, picker [Claude]
Date: 2026-11-13
Files: Assets/_Project/Scripts/Run/* (RunProgress, RunProgressLink, RunHost, LotDefinition, LotDefinitionRules, LotPacing, LotModifier, RunDefinition, LotChoices, PowerGrant, DialPosition, LotLayoutKind, asmdef), Assets/_Project/Tests/EditMode/Run/*
Why: Refs #80 (epic #79). New module UncannyValet.Run (references only Core): the in-memory run state, the per-lot description with its validation, the run list in play order and the persistent host that loads lot scenes.
Tests: new EditMode tests for the run state, the run definition, the lot rules, the seeding and the picker; all pass (full runs in the integration changelog).

## integration: queue slots and arrival budget per lot [Claude]
Date: 2026-11-13
Files: Assets/_Project/Scripts/Run/{LotDefinition,LotDefinitionRules,LotPacing}.cs, Data/Run/Midgard{Training,Crazy}Lot.asset
Why: Refs #80 follow-up. LotDefinition gains QueueSlots (1 to 6, training 3, crazy 4) and LotPacing an arrival budget (0 derives it from shift length and mean gap). Details in the integration changelog.
Tests: see the integration changelog entry of the same name.

## powers: shared power bar of three buckets, remove speed boost and Park Smash [Claude]
Date: 2026-11-13
Files: Run/RunProgress.cs (RevokePower), Run/RunProgressLink.cs (PowerRevoked).
Why: The developer power row must be able to take a power away again; story code never revokes. issue #86 of epic #79 (Valet to Valhalla).
Tests: DevPowerLinkTests; EditMode and PlayMode pass.
