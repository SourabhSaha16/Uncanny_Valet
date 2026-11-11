# Difficulty dials

The F2 panel opens with nine dials (group "Difficulty") plus the two clock rows. Every other number sits behind the
panel's **Advanced** toggle (off at every launch). A dial move re-derives every number it drives; the director reads
only the underlying keys, exactly as before.

## How the mapping works

- Each dial is 0..10 in 0.5 steps and starts at **5**, where every underlying number keeps the value its config asset
  authors. So the game plays exactly as before until a dial moves.
- Each row below is an anchor table (`DifficultyDialsConfig`, defaults in `DialAnchors`): the effect at dial 0, at
  dial 5 (always "no change") and at dial 10. Between anchors the value is linear (piecewise linear overall).
- **Scale** anchors multiply the key's authored value (a boost multiplier is scaled around 1, so 1 stays "no boost").
  **Add** anchors add to it. Several dials driving one key multiply (Scale) or sum (Add).
- Whole-number keys round; every key stays inside its panel range.
- A hand-edit of an Advanced row sticks until the next dial move (any dial move re-derives all driven keys).
  Reset on an Advanced row returns it to its authored value.
- Direction: higher Overall Intensity, Ramp, Late-Shift Rush, Dirty %, EV %, Dilemma Frequency and Skill Reward are
  harder; higher **Mercy** is easier (more relief). Tests check each key moves monotonically the stated way.

Values below are for the default authored numbers: dial 0 / dial 5 / dial 10.

## 1. Overall Intensity (higher = harder)

| Key (panel label) | 0 | 5 | 10 | Direction |
| --- | --- | --- | --- | --- |
| DirTargetScale (Target workload, x curve) | 0.5 | 1 | 1.4 | up |
| DirUnderFactor (how much faster cars arrive when under-worked) | 0.78 | 0.6 | 0.39 | down = faster |
| DirOverFactor (how much slower cars arrive when overwhelmed) | 2.1 | 1.5 | 1.2 | down = less breathing room |
| DirMinInterval (shortest wait, s) | 7.5 | 5 | 3 | down |
| DirMaxInterval (longest wait, s) | 56 | 40 | 30 | down |
| DirArrivalBudget (cars per shift) | 9 | 14 | 20 | up |
| DirBudgetSlack (extra cars) | 1 | 2 | 4 | up |
| PlanMaxWaitingCars | 2 | 3 | 3 | up |

## 2. Ramp Over the Shift (steeper = harder finale, easier start)

| Key | 0 | 5 | 10 | Direction |
| --- | --- | --- | --- | --- |
| DirRampShape (how much the curve rises and dips; 0 = flat at its average) | 0 | 1 | 1.6 | up |

## 3. Late-Shift Rush (higher = harder)

| Key | 0 | 5 | 10 | Direction |
| --- | --- | --- | --- | --- |
| PlanLateEvChance (also x the EV % dial) | 0.2 | 0.5 | 0.8 | up |
| PlanLateEvStart (surge starts at) | 0.78 | 0.6 | 0.42 | down = earlier |
| DirLateTargetBoost (extra target at shift end, added) | -0.3 | 0 | +0.4 | up |

## 4. Car Mix: Dirty Cars % (higher = more washing)

| Key | 0 | 5 | 10 | Direction |
| --- | --- | --- | --- | --- |
| PlanDirtyChance | 0 | 0.3 | 0.6 | up |
| DirWeightDirty (base weight) | 0 | 20 | 50 | up |
| DirWeightDirtyEv (also x the EV % dial) | 0 | 10 | 25 | up |
| DirStreakLimit (dirty or EV in a row) | 1 | 2 | 3 | up |

## 5. Car Mix: EV Cars % (higher = more charging)

| Key | 0 | 5 | 10 | Direction |
| --- | --- | --- | --- | --- |
| PlanEvChance | 0 | 0.3 | 0.6 | up |
| PlanLateEvChance (also x the Late Rush dial) | 0 | 0.5 | 1 (capped) | up |
| DirWeightEv (base weight) | 0 | 20 | 50 | up |
| DirWeightDirtyEv (also x the Dirty % dial) | 0 | 10 | 25 | up |

## 6. Dilemma Frequency (higher = more squeeze moments)

| Key | 0 | 5 | 10 | Direction |
| --- | --- | --- | --- | --- |
| DirDilemmaWashBlocked / WashBusy / BaysBlocked / BaySquatted / LastBay / LotTight / Chain (boost multipliers, scaled around 1) | 1 (no boost) | 3 / 1.5 / 3 / 2.5 / 1.5 / 1.5 / 2 | 4.4 / 1.85 / 4.4 / 3.55 / 1.85 / 1.85 / 2.7 | up |
| DirLotTightSpots (lot is tight at this many free spots) | 0 | 1 | 2 | up |
| DirDilemmaCooldown (s) | 75 | 25 | 10 | down |
| DirDilemmaBlackout (s after an angry patron) | 50 | 20 | 8 | down |
| DirHeadroomGap | 0.045 | 0.15 | 0.3 | up |

## 7. Mercy (higher = easier)

| Key | 0 | 5 | 10 | Direction |
| --- | --- | --- | --- | --- |
| DirReliefWorkload (back off above this workload) | 2.8 | 1.4 | 0.84 | down = sooner |
| DirReliefPressure | 0.98 | 0.85 | 0.6 | down |
| DirReliefAngryCount | 4 | 2 | 1 | down |
| DirReliefAngryWindow (s) | 15 | 30 | 60 | up |
| DirReliefHold (s of no arrivals) | 4 | 10 | 20 | up |
| DirReliefPlainCars | 0 | 2 | 4 | up |
| DirReliefCooldown (s) | 60 | 20 | 8 | down |
| DirAssistDilemmaScale (dilemma strength kept in assist) | 0.8 | 0.5 | 0.2 | down |
| DirAssist (assist mode, added) | 0 | 0 | 1 | on above dial 7.5 |

## 8. Skill Reward (higher = good play raises difficulty more)

| Key | 0 | 5 | 10 | Direction |
| --- | --- | --- | --- | --- |
| DirFormInfluence | 0 | 0.15 | 0.33 | up |
| DirFormTipWeight | 0.18 | 0.6 | 0.96 | up |
| DirFormAngryWeight | 0.5 | 1 | 1.5 | up |
| DirFormComboWeight | 0.12 | 0.4 | 0.8 | up |
| DirFormWindow (s looked back) | 90 | 60 | 36 | down = faster reaction |
| DirFormWarmup (s grace) | 60 | 30 | 9 | down |
| DirFormMaxStep | 0.1 | 0.25 | 0.5 | up |

## 9. Randomness Seed

Not mapped: it is the existing `DirSeed` key (0 = new random shift, otherwise the same shift replays), shown as a dial.

## Not in the dials (patron-side pacing, Advanced only)

Pickup window, stay times, impatient chance, wash and charge minutes, tips, the old fixed timer gaps, pedestrians and
traffic. Only **Game minutes per second** and **Shift length** stay visible besides the dials.

## How to add a dial

1. Add a `Dial...` key at the end of `TuningKey`, a row in `DifficultyRows` (label, one-line tooltip) and the key to
   `DifficultyDials.DialKeys`.
2. Add its rows to the anchor table (`DialAnchors` or a sibling file): `new DialLink(dial, derivedKey, atZero, atTen)`,
   with `DialEffect.Add` for keys whose authored value is 0, or a pivot of 1 for boost multipliers.
3. If a number the director reads has no tuning key yet, add the key, seed it in the owner's `Seed`, read it with
   `TuningRead.Float` in the owner's `Read`, and give it a row in the matching `Director*Rows` file.
4. Add a monotone-direction `TestCase` to `DifficultyDialsTests`, then run `-executeMethod
   UncannyValet.EditorTools.ProjectSetup.RunBatch` so a new config asset picks up the rows (an existing asset keeps
   its own list: edit it or delete it so setup recreates it).
