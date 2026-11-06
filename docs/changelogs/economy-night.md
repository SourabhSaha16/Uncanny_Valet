# Economy and Night changelog

Entries are added by the economy-night agent, one per commit. Format in docs/changelogs/README.md.

## economy-night: add tips, debt ledger and night clock
Date: 2026-11-05
Files: Assets/_Project/Scripts/Economy/{TipConfig,RunConfig,TipCalculator,DebtLedger}.cs (+ metas), Assets/_Project/Scripts/Night/{ClockFormat,NightClock}.cs, Assets/_Project/Data/Economy/{TipConfig,RunConfig}.asset, Assets/_Project/Tests/EditMode/{Economy,Night}/*, removed the EconomyModule and NightModule markers
Why: P1 build of design.md sections 2, 5 and 9. Tuning numbers live in config assets.
Tests: EditMode tests for tip floor, impatient bonus, penalty curve, ledger night-end maths and clock format incl. midnight wrap.
