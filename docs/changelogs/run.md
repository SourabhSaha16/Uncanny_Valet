# Run module changelog

## integration: RunProgress, LotDefinition, lot scenes, picker [Claude]
Date: 2026-11-13
Files: Assets/_Project/Scripts/Run/* (RunProgress, RunProgressLink, RunHost, LotDefinition, LotDefinitionRules, LotPacing, LotModifier, RunDefinition, LotChoices, PowerGrant, DialPosition, LotLayoutKind, asmdef), Assets/_Project/Tests/EditMode/Run/*
Why: Refs #80 (epic #79). New module UncannyValet.Run (references only Core): the in-memory run state, the per-lot description with its validation, the run list in play order and the persistent host that loads lot scenes.
Tests: new EditMode tests for the run state, the run definition, the lot rules, the seeding and the picker; all pass (full runs in the integration changelog).
