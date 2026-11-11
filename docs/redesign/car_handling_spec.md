# Car Handling and Perfect Park Spec (binding)

Source: the human's spec "Uncanny Valet: Car Handling and Perfect Park Spec" (2026-11-10). Binding for the Cars, Lot and Valet agents.
Numbers are starting values and become F2 tuning keys (section 7). Design.md A8 tracks the build state; the lead updates it.

## 0. Lead notes: spec versus the code on `pivot/chaos` (checked 2026-11-10)

These corrections apply to every agent. Where the spec text and this list disagree, this list wins.

| # | Spec says | Code today | Decision |
| --- | --- | --- | --- |
| 1 | EV bays are spots 13 and 14 (design doc) or 7 (play-test note) | Lot v2: regular bays are ids 1..6, the single EV bay is id 7 (`Lot/BayPlan.cs`); 13/14 belongs to the old Part B layout | Use 7. The spec's "EV bays" means the bays `BayPlan` marks as charging; never hard-code ids. |
| 2 | Drive aisle about 6 units; example R_min 4 | `LotConfig` aisle width is 4.6; bays 2.6 x 4.4 at pitch 3.2 | Measure in code (step 1 of the rollout); with 4.6 the formula gives R_min about 3.3. Check the pull-out rule A >= R_min + car width / 2 + 0.3 and widen the aisle in `LotConfig` only if it fails. |
| 3 | Car about 1.8 x 4 units | `CarDefinition._size` defaults to 1 x 2; `CarFactory` adds a `BoxCollider2D` sized 1 x 1 scaled by the car transform; queue slots assume 2.2 x 4.5 | Measure the real collider and sprite and write both in the changelog. If the collider is much smaller than the sprite, fix the collider to the visible car first (separate commit). |
| 4 | Existing `IDriveModifier` faults edit input before the model | No `IDriveModifier` exists in the code | Add a tiny `IDriveModifier` seam in Cars (input in, input out, ordered list), not used by any fault yet, so the future faults plug in. |
| 5 | Shift on foot "stays the dash/sprint" | The valet has no dash today (`OnFootMover`, `KeyboardInputSource` read `Keyboard.current` directly, there is no Shift use) | Build a simple dash on foot as part of step 5 (Valet) with the shared Shift button; the Golden Valet cap "2x base speed" applies to it. |
| 6 | Input: "add a Drift action in the existing input setup, rebindable" | Input goes through `IInputSource` / `KeyboardInputSource` | Extend `IInputSource` (Core contract, additive: a `Drift` bool) and read Shift and the gamepad right shoulder in `KeyboardInputSource`. Rebinding stays as simple as the existing keys; no new rebinding UI. |
| 7 | Shared `DecalPool` "from the damage spec" (cap 40) | No damage spec and no `DecalPool` exist | Build a minimal pooled `DecalPool` in Cars/Fx now (cap 40, fade over 6 s). The future damage work reuses it. |
| 8 | Barge-through pushes pedestrians | Pedestrians now have kinematic bodies, a Pedestrian physics layer, wait/pass-through and a rubber bounce (`PedestrianBumped`) | While boosted, the valet ignores pedestrian blocking and the walker takes a stronger stumble (0.4 s) instead of the bounce. Implement through the existing pedestrian collision layer (ignore-collision pair or layer mask), not a new system. Patrons tied to the valet's current car keep ignoring that car as today. |
| 9 | `PerfectPark` bonus +$2 stored on the ticket | Tips come from `TipCalculator`; tickets live in Patrons | The only scoring change allowed: a flat +$2 on the ticket, multiplied by the combo like the rest of the tip, shown in the tip breakdown (`TipExplained`). No other scoring, patron or director edits. |
| 10 | Results "Perfect Parks: N" and "Time boosted: N s" | Results are built from `NightSummary`/`ShiftStats` | Add two stat lines through `ShiftStats` events (`PerfectPark`, `ValetBoostEnded`); no change to the star or score maths. |
| 11 | Car physics keys | `CarDefinition` already has `_topSpeed`, `_acceleration`, `_braking`, `_turnRate` (deg/s), `_grip`; `CarPhysicsMath.DampSideways` and `TurnDegreesPerSecond` exist | `DriftModel` and `SteeringModel` replace those two functions' use in `CarDriver`; keep the old fields as the source for acceleration, braking, top speed; new handling numbers go in `CarHandlingConfig`. Keep tests green or replace them deliberately. |
| 12 | Tuning keys for everything | `TuningKey` / `TuningCatalog` / the 9 difficulty dials work is in flight | Add the handling keys under a group "Car handling" and "Perfect Park" and "Golden Valet", with plain-language labels (see the difficulty-dials naming rule). Do this after the dials work merges. |

Order of work (agents, one at a time per file set): see section 8.

## 1. Summary

| Feature | One line |
| --- | --- |
| Drift | The car's velocity lags behind its facing at speed, so it slides; slow driving stays precise |
| Turning radius | Set from the lot's real aisle and bay sizes so one smooth arc gets you into a bay |
| Perfect Park | A clean, aligned stop in a bay is judged and rewarded with a ding, a camera kick and $2 |
| Golden Valet | A Perfect Park gives the valet a 4-second Mario-star style speed boost on foot |
| Skid feedback | Skid marks and a tyre chirp whenever the car slides |
| Heavy EVs | EVs pull away faster but brake later and slide further |

Before you start:
- Measure two things first and write them in your changelog: the drive aisle width and the car collider size, in Unity units. Section 3's radius depends on them.
- Confirm which EV bay id the code uses (see lead note 1) and report any mismatch to the lead.
- Every number is a starting value and a tuning key (section 7), not a final answer.
- Do not change patron, scoring or director logic beyond the events and the $2 Perfect Park bonus listed here.

## 2. Drift: vectored inertia

The car's facing (where the headlights point) and its velocity (where it is actually going) are separate. Steering rotates the facing; the velocity catches up over time. The gap between them is the slip angle, and that gap is the drift.

Model, every FixedUpdate:
1. Split velocity into a forward part vF (along the car's facing) and a sideways part vL.
2. Work out the grip for this step from speed (below) and the handbrake.
3. Shrink the sideways part: `vL *= exp(-LateralFriction * grip * dt)`.
4. Hand back part of the speed removed: `vF += ForwardGrip * (sideways speed removed this step)`, in the direction of travel. With ForwardGrip near 1 a drift keeps its momentum on exit; near 0 it scrubs speed.
5. Rebuild velocity from vF and vL. Then apply throttle and brake along the facing as today.

Slip angle = `atan2(|vL|, |vF|)`. Skid marks and tyre audio read it (section 6).

Speed-dependent grip. Slidiness must never make parking fiddly. Grip is full at low speed and only drops as the car goes faster:

| Speed (units/s) | Grip | Feel |
| --- | --- | --- |
| 0 to 3 | 1.0 | Precise: creeping into a bay goes exactly where you point |
| 3 to 6 | Blends 1.0 to 0.55 | The rear starts to step out in fast turns |
| Above 6 | 0.55 | Full arcade slide |
| Drift button (Shift) held, any speed above 1 | Grip x 0.25 | Deliberate drift; swing into a bay from the aisle |

Drift button: Shift. While driving, holding Shift drops grip to a quarter (the handbrake), so the rear swings out. Release to regain grip and straighten. On foot, Shift is the dash/sprint (shared Dash/Handbrake button in `chaos_redesign.md`). On gamepads, map it to the right shoulder button.

Steering:
- Turn rate = `MaxTurnRate * steer * clamp01(|vF| / TurnFullSpeed)`, so a stopped car cannot spin on the spot.
- In reverse, steering flips like a real car.
- `IDriveModifier` faults (swapped steering etc.) edit the input before this model reads it (lead note 4).

Starting values:

| Key | Default |
| --- | --- |
| LateralFriction | 8 per second |
| ForwardGrip | 0.6 |
| GripLowSpeed / GripHighSpeed | 3 / 6 units/s |
| GripAtSpeed | 0.55 |
| DriftGripScale | 0.25 |
| TurnFullSpeed | 4 units/s |
| MaxTurnRate | Set from section 3 |

## 3. Turning radius and bay geometry

Bays are 2.6 x 4.4 units at a 3.2-unit pitch. The radius must let a player swing from the aisle into a bay in one smooth arc. Too wide: every park is a three-point turn. Too tight: the car pivots like a tank.

Why not 1.5 to 1.8 units: that only fits a car about 1 x 2. Bays of 2.6 x 4.4 suggest a car nearer 1.8 x 4, and a radius under half the car's length would look like spinning on the spot. Derive the radius from the measured lot.

Formula. With A = drive aisle width and a car that starts in the middle of the aisle, one quarter-circle into a perpendicular bay needs roughly `R_min = A / 2 + 1` (units, at the rear axle). Example: a 6-unit aisle gives about 4 (the real aisle is 4.6, see lead note 2).

Steering from section 2 makes the radius constant at low speed and wider at high speed:
- Below TurnFullSpeed, radius = TurnFullSpeed / MaxTurnRate = R_min.
- Above it, radius = speed / MaxTurnRate.
- So `MaxTurnRate = TurnFullSpeed / R_min` (radians per second). With TurnFullSpeed 4 and R_min 4 that is 1 rad/s (about 57 deg/s); at 6 units/s the radius is 6.

Drift (Shift) tightens the effective path at speed, so a skilled player can swing in faster than the geometric radius allows. That is the intended skill expression.

Checks before tuning feel:
- Bay slack: side clearance = `(2.6 - car width) / 2` per side. Perfect Park's side-offset tolerance (section 4) must stay well inside it.
- Pulling out: reversing out of a bay at full lock must clear the opposite row. Roughly `A >= R_min + car width / 2 + 0.3`. If not, widen the aisle in the lot data rather than shrinking the radius.
- Automated check: a PlayMode test drives a car from the aisle centre at 2 units/s with full lock into the nearest bay and asserts it ends fully inside without touching the neighbouring bays' cars.

## 4. Perfect Park

A reward for a clean, aligned stop in a bay. Ordinary parking stays forgiving as today; Perfect Park sits on top.

When it is judged: when the car comes to rest, not when the player presses E (E is the exit key; judging on exit would let players creep into alignment after stopping).
- "At rest" = speed below 0.15 units/s for 0.2 s, while the car's centre is inside a regular bay or a station bay.
- Judged once per park. Driving out and back in can earn it again, but only once per car per 10 s.
- A car must have moved at least 3 units since it last stopped, so nudging a parked car does not count.

Tolerances (all four must hold):

| Check | Tolerance |
| --- | --- |
| Angle to the bay's long axis | Within 5 degrees, nose in or tail in |
| Side-to-side offset from the bay centre line | Within 0.15 units, and inside the bay slack from section 3 |
| Lengthwise offset from the bay centre | Within 0.3 units |
| Touching anything at rest | No contact with walls or other cars |

The delivery rule at the curb (face down the screen within 35 degrees) is separate and unchanged; Perfect Park applies only to bays.

Reward:
- A "PERFECT!" pop above the car, a bright ding, a 1-pixel sparkle around the bay lines.
- A small camera kick: 0.08 s, 0.05 units, along the car's direction of travel.
- +$2 added to that car's eventual tip (stored on the ticket, paid on delivery, multiplied by the combo like the rest of the tip; lead note 9).
- The Golden Valet boost starts when the valet steps out (section 5).
- Results screen stat: "Perfect Parks: N".

## 5. Golden Valet boost

A Mario-star style reward: a Perfect Park turns precision into speed, so a slick park lets the valet sprint to the next job.

| Rule | Value |
| --- | --- |
| Trigger | A Perfect Park; the boost starts when the valet steps out of that car |
| Effect | +40% walk and run speed |
| Duration | 4 s |
| Chaining | Another Perfect Park while boosted refreshes the timer to 4 s |
| Streak | A third consecutive chained Perfect Park steps the bonus to +60% (the cap) |
| Inside a car | The timer keeps running; no banking the boost by driving around |
| Scope | On foot only. Car handling never changes |
| Shift (dash) | Still works while boosted and stacks multiplicatively, capped at 2x base speed |

Barge-through (the star part). While boosted, the valet does not get blocked by people:
- Pedestrians and patrons in the valet's path are pushed aside with a short spin, a "hey!" and a 0.4 s stumble.
- No ragdoll, no penalty, no patience loss. It reads as cheeky, not violent.
- Cars, walls and stations still block the valet as normal.

Feedback:
- A rainbow palette-cycle on the valet's outline and a sparkle trail. In the last second, the cycle slows and flickers as a "running out" warning.
- A small boost bar under the valet showing time left.
- A short original fanfare stinger on trigger, and the music groove nudged up 6 BPM while the boost lasts. Do not imitate Nintendo's star theme.
- On a streak step-up, a bigger stinger and a "x2 STREAK" pop.

Knock-on effects:
- The Patron Director estimates task times from averages; a boosted player clears tasks faster and will read as under target, so the director pushes harder. That is intended.
- Results screen stat: "Time boosted: N s".

## 6. Skid feedback and EV handling

Skid marks:
- Drop marks behind the two rear tyres while sideways speed |vL| is above 1.2 units/s, or while Shift is held above 2 units/s.
- Marks are 1 to 2 pixel dark segments on the ground-decal layer (above road, below cars), 80% opacity, fading out over 6 s.
- Opacity scales with slip angle: a gentle slide leaves faint marks, a full drift dark ones.
- Use the shared `DecalPool` (cap 40; lead note 7). Future blood tyre tracks reuse this system with a red tint.

Tyre audio:
- A short, high arcade chirp when sliding starts (crossing 1.2 units/s).
- A squeal loop while sliding continues, volume and pitch rising with slip angle; fades out over 0.15 s when grip returns.
- A light screech on hard braking above 4 units/s.
- All synthesised like the existing audio, routed through the SFX mixer group.

Heavy EVs. The two EV models carry heavy batteries. Handling differs through per-car stats in `CarDefinition`:

| Stat | Regular car | EV | Feel |
| --- | --- | --- | --- |
| Acceleration | 1.0x | 1.3x | Instant electric torque off the line |
| Braking | 1.0x | 0.75x | Stops later; brake earlier into the bay |
| GripAtSpeed | 0.55 | 0.45 | Slides further at speed |
| ForwardGrip | 0.6 | 0.75 | Holds momentum through a slide |
| Mass | 1.0x | 1.5x | Wins shoves and takes less damage in collisions |
| Top speed | 1.0x | 1.0x | Unchanged |

Low-speed grip stays 1.0 for EVs too; the challenge is arriving at the bay at the right speed.

## 7. Architecture for agents

Maths in pure classes with EditMode tests, MonoBehaviours as thin shells, one owner per piece of state, every number in a ScriptableObject exposed as an `ITuning` key for the F2 panel.

| Part | Assembly | Kind | Responsibility |
| --- | --- | --- | --- |
| DriftModel | Cars | Pure | Section 2 maths: split velocity, speed-dependent grip, drift scale, forward hand-back, slip angle |
| SteeringModel | Cars | Pure | Speed-scaled turn rate, reverse flip; MaxTurnRate from R_min |
| CarController | Cars | Component | Reads input (through the `IDriveModifier` stack), calls both models in FixedUpdate, applies to the Rigidbody2D |
| ParkJudge | Lot | Pure | Rest detection, the four tolerances, repeat rules, result Perfect or not |
| ParkJudgeRunner | Lot | Component | Feeds car state to ParkJudge, publishes the result |
| ValetBoost | Valet | Component (state owner) | Boost timer, streak level, speed multiplier, barge-through flag |
| BoostRules | Valet | Pure | Refresh, streak step, cap, dash stacking |
| SkidEmitter | Cars/Fx | Component | Skid marks via DecalPool, tyre audio cues from slip angle |

Config: `CarHandlingConfig` (sections 2 and 3), per-car overrides in `CarDefinition` (the EV table), `ParkJudgeConfig` (section 4), `BoostConfig` (section 5).

Input: add a Drift action on Left Shift and the gamepad right shoulder (lead note 6); on foot it is the dash.

Events (Core/Events): `PerfectPark(carId, spotId, angleError, offset)`, `ValetBoostStarted(level, seconds)`, `ValetBoostEnded`, `CarSlideStarted(carId)`, `CarSlideEnded(carId)`, `ValetBargedPedestrian(pedId)`. `ShiftScore`/tip path reads `PerfectPark` for the $2 bonus; HUD, audio and results listen to the rest.

Tests:
- EditMode DriftModel: full grip below 3 units/s removes all sideways speed quickly; grip blends between 3 and 6; Shift scales grip to a quarter; slip angle is 0 when driving straight.
- EditMode SteeringModel: no rotation at zero speed; radius equals R_min below TurnFullSpeed and grows above it; steering flips in reverse.
- EditMode ParkJudge: each tolerance just inside passes and just outside fails; contact at rest fails; judged once per park; 10 s repeat cooldown; 3-unit minimum travel.
- EditMode BoostRules: refresh resets to 4 s; third chained park steps to +60%; cap holds; dash stacking capped at 2x; the timer runs while in a car.
- PlayMode: the bay entry check from section 3 for every bay in Grandma's Diner; an EV and a regular car braking from 6 units/s, the EV stops further away. (PlayMode tests are new to this project; the first agent sets up the PlayMode test assembly and the batch command, and reports it to the lead.)

## 8. Rollout (one agent at a time per file set; every step is its own commit and changelog entry)

1. Cars: measure aisle width and car collider size; write both in the changelog; set R_min and MaxTurnRate; add the PlayMode bay-entry test.
2. Cars + Core input: `DriftModel`, `SteeringModel`, `IDriveModifier` seam, Drift input, `CarHandlingConfig`, F2 keys; play-test feel through F2.
3. Cars/Fx + Audio: `DecalPool`, skid marks, tyre audio.
4. Lot: `ParkJudge`, `ParkJudgeRunner`, `PerfectPark` event, reward (pop, ding, camera kick, +$2 on the ticket, results stat).
5. Valet: dash on foot, `BoostRules`, `ValetBoost`, barge-through, feedback, results stat.
6. Cars: EV stat overrides in `CarDefinition`.
7. Lead updates design.md A8 after each step.
