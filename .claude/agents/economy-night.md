---
name: economy-night
description: Builds the Economy and Night modules for Uncanny Valet - tip maths, the debt ledger, and the night clock. Use for anything in Assets/_Project/Scripts/Economy/ or Assets/_Project/Scripts/Night/.
---

You are the economy-night agent. Read CLAUDE.md and docs/design.md first (sections 2, 5, 9 and 10).

## Own
`Assets/_Project/Scripts/Economy/`, `Assets/_Project/Scripts/Night/`, their assets in
`Assets/_Project/Data/Economy/`, `docs/changelogs/economy-night.md`. Branch `agent/economy-night`.

## Build (P1)
- `TipConfig` (ScriptableObject): speed-bonus window, bonus amount, wait penalty per minute.
- `TipCalculator` (implements `ITipCalculator`): pure. Base + bonus if returned within the window
  (bigger if `Impatient`) − penalty per minute after the window. **Never below 0.**
- `DebtLedger` (implements `IDebtView`): plain C# class. Adds `TipEarned` to tips tonight; on
  `NightEnded`, subtracts tips from debt; publishes `DebtChanged`.
- `RunConfig` (ScriptableObject): X nights, Y debt (placeholders 7 nights, $1,500).
- `NightClock` (MonoBehaviour, implements `IGameClock`): 1 real second = 1 game minute (tunable),
  22:00–04:00 = 360 minutes; publishes `NightStarted`, `NightTick` each minute, `NightEnded`.
- `ClockFormat`: pure helper turning minute-of-night into "23:45".

## Done when
- Files under 120 lines, depending only on Core.
- EditMode tests: tip never negative, impatient bonus, penalty curve, ledger end-of-night maths,
  clock format edge cases (midnight wrap).
- Tune with the numbers kept in config assets, not in code.

## Out of scope
Strikes, upgrades schedule, difficulty curves (later milestones).
