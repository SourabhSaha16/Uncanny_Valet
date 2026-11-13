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

## services: one model for what a car needs (service modifiers) [Claude]
Date: 2026-11-13
Files: Assets/_Project/Scripts/Run/{ModifierChance,LotDefinition,LotDefinitionRules,DialPosition}.cs, Data/Run/Midgard{Training,Crazy}Lot.asset, Tests/EditMode/Run/*
Why: Issue #121. A LotDefinition lists (modifier, chance) pairs, seeded with the other per-lot pacing values before the dials and validated (no duplicates, 0 to 1 each, total at most 1); both Midgard lots carry today's diner chances (needs charge 0.3 then Muddy 0.3). DialPosition no longer accepts the retired Car Mix slots.
Tests: see docs/changelogs/services.md (EditMode and PlayMode all green).

## blessings: tips become blessings, favour, curses, lot threshold [Claude]
Date: 2026-11-13
Files: see docs/changelogs/blessings.md (this module's part of the same commit).
Why: Issue #81 of epic #79: tips, combo and complaints are renamed to blessings, favour and curses; stars are replaced by the lot's blessing threshold.
Tests: see docs/changelogs/blessings.md (EditMode and PlayMode all pass).

## powers: power framework with PowerDefinition levels, cooldowns and HUD icons [Claude]
Date: 2026-11-13
Files: see docs/changelogs/powers.md (this module's part: LotPowerFill, LotDefinition, LotDefinitionRules, RunProgress.FlashedLevel and MarkFlashed, RunProgressLink).
Why: Issue #87 of epic #79: per-lot power bar fill amounts as LotDefinition overrides, and the run state remembers which power icon flashed.
Tests: see docs/changelogs/powers.md.
