# Lot-count and pressure balance (simulation)

Written by qa. Analysis only: no game code, scene or Data asset was changed. Everything below is
reproducible with `python tools/sim/lot_sim.py` (Python 3 standard library, fixed seed 20261106).
Based on `main` at `df91473` (round-4 modules: night plans, EV charging, wash).

## 1. The answer in one screen

**Recommended expansion schedule** (Schedule A: "moderate" as defined in the brief, section 5). Competent
valet, 500 simulated nights per row. `MaxWaitingCars` and the gap scale are the existing `NightPlan` fields.

| Night | Spots | Added | Comfort spots* | MaxWaitingCars | ArrivalGapScale | Cars arriving: mean, p95, max | Spots demanded p95, max | Cars in lot p95, max |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 4 | start | 4 | 1 | 3.7 | 4.7, 5, 6 | 2, 3 | 3, 3 |
| 2 | 4 | +0 | 4 | 3 | 3.2 | 5.3, 6, 6 | 3, 3 | 3, 4 |
| 3 | 5 | +1 | 5 | 3 | 2.5 | 6.7, 7, 8 | 4, 5 | 6, 7 |
| 4 | 5 | +0 | 5 | 3 | 2.4 | 6.9, 8, 8 | 4, 5 | 6, 7 |
| 5 | 5 | +0 | 5 | 3 | 2.3 | 7.1, 8, 9 | 4, 5 | 7, 7 |
| 6 | 5 | +0 | 6 | 3 | 2.2 | 7.4, 8, 9 | 5, 5 | 7, 8 |
| 7 | 5 | +0 | 6 | 3 | 2.0 | 8.0, 9, 9 | 5, 6 | 7, 8 |

\* "Comfort" = the smallest lot that is full in at most 5% of nights. Use it if you do not want the lot to
ever be the problem; the tight column is what keeps "lot full" present but not constant.
"Spots demanded" and "cars in lot" were measured with an unlimited (24 spot) lot, so they size the lot.

Outcome per night for novice / competent / expert (same rows):

| Night | Park timeouts % n/c/e | Pickups on time % (competent) | Tips $ per night n/c/e | Valet busy % (competent) | Nights the lot is full % (competent) |
| --- | --- | --- | --- | --- | --- |
| 1 | 0/0/0 | 88 | 27/50/55 | 60 | 0 |
| 2 | 0/0/0 | 88 | 26/55/60 | 68 | 0 |
| 3 | 15/2/0 | 55 | 11/30/53 | 89 | 2 |
| 4 | 17/4/1 | 63 | 15/37/62 | 87 | 2 |
| 5 | 20/6/2 | 44 | 11/22/46 | 91 | 3 |
| 6 | 21/7/2 | 39 | 11/19/41 | 92 | 8 |
| 7 | 24/10/4 | 30 | 9/12/33 | 93 | 13 |

**Five findings**

1. **The current config is far too hard for one valet.** With the real `PatronConfig`, `NightPlanDefaults`
   and 12 spots, a competent valet is busy 98-99% of every night and loses 26-32% of the cars on nights 2-7
   (night 1 loses none but is also at 99% busy). Pickups are almost never on time from night 2 on (5% for
   competent), so tips are about $0-3 per night.
2. **The valet is the bottleneck, not the lot.** At every pace the valet can sustain, the lot never needs more
   than 5-6 spots at the same moment (p95 of spots in use, 24-spot lot). Twelve spots are never full (0.0
   lot-full events per night from 8 spots up, every night, every policy). More spots only add walking and
   driving distance. So adding lots cannot be what makes later nights harder; the arrival pace
   (`ArrivalGapScale`) is. If you want the lot to grow as a progression, treat it as presentation, or see
   section 7 for what would make it matter.
3. **Maximum cars per night.** A competent valet sustains about **8 cars a night** (mean; p95 9, max 9) with
   the moderate band on night 7. At full saturation (gap scale 1.0, cap 3) about 10-11 cars arrive but a
   quarter of them vanish and almost none pay. At the same pace an expert gets the same arrivals but loses 4%
   of the cars instead of 10%.
4. **The debt cannot be reached.** Seven nights of the recommended schedule pay a mean of $225 (competent), $349
   (expert), $109 (novice) against a $1500 debt: 0% of 500 runs clear it. Each paid car is worth $18-25 and at
   most 6-7 cars a night can be delivered, so even a flawless run (every bonus, every pickup inside the speed
   window) would pay roughly $1300 at the very most, and no simulated player came close. Tips scale linearly
   with `BaseTip` and the two bonuses, so either multiply them by about 4-7 or set the debt near $300-400
   (see section 6).
5. **Money falls as pressure rises.** Competent tips per night go $50, $55, $30, $37, $22, $19, $12 across
   the schedule. Above about 85% valet load the parking queue starves the pickups, they go late, and a late
   pickup is worth $0 after 60 game minutes. That is the main reason the "moderate" band on night 7 is
   financially punishing; Schedule B (section 6) is the gentler alternative.

## 2. Units and conversions

| Quantity | Game | Real time |
| --- | --- | --- |
| 1 game minute | 1 | 0.667 s (`GameMinutesPerSecond` = 1.5) |
| A night | 180 min (`NightLengthMinutes`) | 120 s |
| Arrival gap | 8-15 min x `ArrivalGapScale` | 5.3-10 s x scale |
| Stay in a spot | 20-45 min | 13-30 s |
| Park window | 45 min | 30 s |
| Pickup window | 60 min | 40 s |
| Speed-bonus window | 10 min | 6.7 s |
| Charging an EV (20% to full, 5% per min) | 16 min | 10.7 s |
| Washing | 15 min | 10 s |

Note: `docs/design.md` section 10 still says "1 real second = 1 game minute". The code (`RunConfig`) uses
1.5, and so does this simulation. The design doc should be updated by the lead.

## 3. Where every number comes from

All values live in `tools/sim/params.py` (one block, each line tagged REPO or ASSUMPTION).

| Number | Value | Source |
| --- | --- | --- |
| Nights, starting debt, night length, speed | 7, $1500, 180 min, 1.5 min/s | `Data/Economy/RunConfig.asset`, `RunConfig.cs` |
| Arrival gap, stay, base tip, impatient chance | 8-15, 20-45, $20, 25% | `Data/Patrons/PatronConfig.asset` |
| Park / pickup window | 45 / 60 min | `PatronConfig.asset` |
| EV start charge, charge minutes, wash minutes | 0.2, 20, 15 | `PatronConfig.cs` defaults (not in the asset file) |
| Tips | window 10, +2 / +6 impatient, -0.5 per min, +15 charged EV, +15 washed | `Data/Economy/TipConfig.asset`, `TipConfig.cs`, `TipCalculator.cs` |
| Night plans (cap, gap scale, EV and dirty chance, stations) | 7 rows | `Night/NightPlanDefaults.cs` |
| Lot layout | 2 rows x 6, spot 2.6x4.4, aisle 5, curb (-12,0) 3x6, 3 queue slots, 2 charging bays + 1 wash | `Data/Lot/LotConfig.asset`, `LotGeometry.cs` |
| Walk speed, interact range, exit distance | 4 u/s, 2 u, 1.5 u | `OnFootMover.cs`, Bootstrap scene `_speed`/`_range`, `SeatController.cs` |
| Car top speed, acceleration, braking | 8, 6, 12 | `Data/Cars/Sedan.asset`, `CarPhysicsMath.cs` |
| Stop-and-settle time before a spot or the curb fires | 0.5 s | `LotConfig.asset` |
| Enter 1.5 s, exit 1.0 s, park precision 3.0 s, curb stop 2.0 s, bay stop 3.0 s | assumption | the human's example values; not defined by the game |

Game rules reproduced from the code: the waiting cap counts every ticket not yet parked (cars in a bay count)
and also needs a free queue slot (`ArrivalGate`); the park timer pauses while a valet drives the car and while it
is stopped in a station, so a finished EV or wash can sit in its bay without ever timing out; a timed-out car
vanishes with $0; a late pickup is $0 but still owed and blocks its spot; a dirty car handed back unwashed is
$0; the wait for the speed bonus runs from the pickup request to the car settling at the curb; tickets still
open when the night ends pay nothing. Game rules run on whole-minute ticks (registry first, then spawner), as
in `NightClock`. The sim's tip function is checked against the same cases as `TipCalculatorTests`
(`python tools/sim/selftest.py`).

## 4. Model

One night is an event loop over game minutes. Whole-minute ticks run the game rules (arrivals, timers, stay
ends, pickup requests, charge and wash progress); the single valet works in continuous time between ticks.

**Arrivals.** At each tick at or after the next-arrival minute the spawner rolls a patron (stay, impatient,
one roll for EV / dirty / plain). The car spawns only if fewer than `MaxWaitingCars` tickets await parking and
one of the first `min(cap, slots)` queue slots is free; then the next gap is rolled and scaled by
`ArrivalGapScale`. A refused roll is thrown away, as in the game. The queue has 3 slots (`LotConfig`), so the
cap cannot be raised above 3 without a lot change; the sim gives caps above 3 extra slots further left.

**Valet jobs** (one at a time, no interruption):

| Job | Steps | Valet ends at |
| --- | --- | --- |
| Park | walk to the car, enter, drive to a free spot, stop in it, exit | the spot |
| Charge / wash (only if the station is open and free; dirty cars must wash, EV charging is optional) | same, but to a charging bay or the wash slot; the car then charges or washes by itself while the valet works elsewhere | the bay |
| Relocate | a finished bay car is driven on to a free spot | the spot |
| Deliver | walk to the spot, enter, drive to the curb, stop, exit; the ticket pays when the car settles | the curb |

Durations = decide time + walk (straight line, minus the 2 u interact range, at 4 u/s) + enter + drive + stop
precision + exit, each multiplied by a random factor in `1 +- spread`. Driving takes two axis-aligned legs
(Manhattan distance; cars stop to turn) at the policy's fraction of top speed, with the Sedan's acceleration
and braking: a 10 u leg at 6 u/s cruise takes 2.4 s. Spots are numbered column by column from the curb. The
park timer pauses when the valet gets in; a car that times out while the valet is still walking to it cancels
the job. Reference times without spread, competent valet:

| Job | decide / walk / enter / drive+stop / exit (s) | Total |
| --- | --- | --- |
| Park, nearest spot | 0.5 / 1.0 / 1.5 / 5.5 / 1.0 | 9.6 s = 14 game min |
| Park, far spot (column 6) | 0.5 / 1.0 / 1.5 / 7.9 / 1.0 | 11.9 s = 18 game min |
| Deliver, near spot (valet already there) | 0.5 / 0 / 1.5 / 5.2 / 1.0 | 8.2 s = 12 game min |
| Deliver, far spot, valet at the curb | 0.5 / 4.6 / 1.5 / 7.6 / 1.0 | 15.2 s = 23 game min |

So one car costs a competent valet about 18-27 s of the 120 s night, which is the root of finding 2.

## 5. Skill policies, and what "moderately challenging" means here

| Policy | Overheads | Spread | Next job | Spot choice | Drive speed | Decide time | Optional EV charge when waiting cars |
| --- | --- | --- | --- | --- | --- | --- | --- |
| novice | x1.5 | +-45% (1.5x) | readiness order (FIFO) | random free spot | 60% of top | 1.5 s | always |
| competent | x1.0 | +-30% | earliest deadline first | nearest to the car | 75% | 0.5 s | at most 2 |
| expert | x0.7 | +-20% | earliest deadline first | nearest the curb | 90% | 0.3 s | at most 1 |

Deadlines for "earliest deadline first" are the real timers: park deadline = now + 45 - elapsed, pickup
deadline = request + 60. A finished station car gets 20 (competent) or 12 (expert) minutes of slack, or goes
first when the waiting cap is hit.

**Definition of moderate** (judged on the competent valet, tightening each night; full bands are printed under
each schedule table by the script):

| Night | Competent park timeouts % | Valet busy % (at most) | Lot full in at most % of nights | Other |
| --- | --- | --- | --- | --- |
| 1 | 0-1 | 60 | 10 | tutorial, no pressure |
| 2 | 0-2 | 70 | 10 | |
| 3 | 0-3 | 90 | 10 | EVs add work, so the busy cap jumps |
| 4 | 0-5 | 90 | 20 | |
| 5 | 2-8 | 91 | 30 | |
| 6 | 5-12 | 94 | 45 | expert under 8% |
| 7 | 10-15 | 97 | 60 | expert under 5% |

Why these numbers. The brief suggests timeouts of 10-20% for the competent valet on the busiest night, under
5% for the expert (skill must pay) and a lot that is sometimes full. Pressure should ramp (design pillar 3), so
earlier nights get a lower band that rises every night. The busy-percentage ceiling is a guard, not a target:
**timeouts of 10-20% and a valet utilisation of 70-85% cannot both hold in this game.** The 45-minute park
window is generous (30 s), so a competent valet only starts losing cars when busy above about 90%. I chose
timeouts as the main axis because they are what the player sees (the car vanishes); the 70-85% reading is
offered as Schedule B. The brief's "novice above 35%" does not fit either: with these policies the novice is
at 24% when the competent valet is at 10% (the three skill levels are only 2-3x apart in task time), and
pushing the novice to 35% would push the competent past 20%.

How the schedule is found (`tools/sim/schedule.py`): for each night, scan `ArrivalGapScale` from 0.4 to 6 at an
ample lot, keep the paces whose competent (and expert) results fall in the night's band, take the busiest one
that is also at least 0.1 busier than the night before (cars never decrease), then take the smallest spot count
(3-14) whose lot-full frequency and timeouts fit, never lower than the night before.

## 6. Schedules, debt, and the current config

**Schedule A** is the table in section 1 (per-night band above).

**Schedule B: gentler** (competent valet busy 54% on night 1 rising to 85% on night 7, timeouts at most 1% for
the competent valet, lot-full present on later nights). 500 nights per row.

| Night | Spots | Comfort spots | MaxWaitingCars | ArrivalGapScale | Cars arriving: mean, p95, max | Spots demanded p95, max | Cars in lot p95, max | Timeouts % n/c/e | Pickups on time % c | Tips $ n/c/e | Busy % c | Lot full % c |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 4 | 4 | 1 | 4.2 | 4.2, 5, 5 | 2, 2 | 2, 3 | 0/0/0 | 89 | 26/46/52 | 54 | 0 |
| 2 | 4 | 4 | 3 | 3.6 | 4.8, 5, 6 | 2, 3 | 3, 3 | 0/0/0 | 87 | 27/51/55 | 62 | 0 |
| 3 | 4 | 4 | 3 | 3.5 | 5.0, 6, 6 | 3, 4 | 4, 5 | 2/0/0 | 79 | 21/45/58 | 76 | 2 |
| 4 | 4 | 4 | 3 | 3.2 | 5.3, 6, 7 | 3, 4 | 4, 5 | 5/0/0 | 79 | 25/50/65 | 77 | 2 |
| 5 | 4 | 5 | 3 | 3.1 | 5.5, 6, 7 | 4, 4 | 5, 6 | 8/1/0 | 68 | 19/41/61 | 83 | 7 |
| 6 | 4 | 5 | 3 | 3.0 | 5.6, 6, 7 | 4, 5 | 5, 6 | 9/1/0 | 66 | 17/38/60 | 84 | 10 |
| 7 | 4 | 5 | 3 | 2.9 | 5.8, 6, 7 | 4, 5 | 5, 6 | 10/1/0 | 64 | 17/37/59 | 85 | 12 |

Choose B if you want the run to feel fair and the player to earn something while still ramping; choose A if
you want night 7 to bite. In B the lot does not need to grow at all.

**Seven-night tips against the $1500 debt** (500 simulated runs each, mean / p10 / p90):

| Config | Novice | Competent | Expert | Runs clearing $1500 |
| --- | --- | --- | --- | --- |
| Current (12 spots, NightPlanDefaults) | $30 / $8 / $53 | $41 / $15 / $69 | $68 / $34 / $112 | 0% |
| Schedule A | $109 / $79 / $138 | $225 / $176 / $272 | $349 / $294 / $399 | 0% |
| Schedule B | $151 / $115 / $187 | $308 / $262 / $353 | $411 / $364 / $456 | 0% |

To make a competent player clear the debt about half the time on Schedule B the total tip pool must be about
5x larger (for example `BaseTip` 100 and bonuses 75), or the debt about $300. Scale `BaseTip` and every `TipConfig` dollar value
together so the speed and wait rules keep their shape; the lead should decide whether to change the tips or
the debt.

**Verdict on the current config** (12 spots, `NightPlanDefaults`, competent valet, against the Schedule A
bands):

| Night | Current: arrivals / timeouts % / busy % | Verdict | Recommended A (cars / timeouts / busy) |
| --- | --- | --- | --- |
| 1 | 7.9 / 0 / 99 | too hard (busy all night; cars wait ~30 game min for service) | 4.7 / 0 / 60 |
| 2 | 13.5 / 32 / 99 | too hard | 5.3 / 0 / 68 |
| 3 | 11.4 / 26 / 99 | too hard | 6.7 / 2 / 89 |
| 4 | 12.1 / 29 / 98 | too hard | 6.9 / 4 / 87 |
| 5 | 10.9 / 27 / 99 | too hard | 7.1 / 6 / 91 |
| 6 | 11.0 / 27 / 99 | too hard | 7.4 / 7 / 92 |
| 7 | 10.9 / 27 / 99 | too hard | 8.0 / 10 / 93 |

No night is too easy. Night 2 is the worst: dropping the gap scale to 0.7 and the cap to 3 at once jumps from
about 8 to 13.5 arrivals. The gap scale on nights 5-7 (0.6) is far below the 2.0-2.3 that gives a moderate night.
A 12-spot lot is more than double what is ever used.

## 7. Making the lot matter (what would have to change)

Spots only bind when more cars sit in the lot than there are spots. That needs more cars per night than one
valet can handle at these task times, or cars staying longer. Sensitivity of the night-7 pace and lot size
(competent valet; band timeouts 10-15%; 250 nights per cell; "stays" multiplies `StayRange`):

| Variant | Gap scale | Cars/night | Spots (tight) | Spots demanded p95 | Tips $ per night |
| --- | --- | --- | --- | --- | --- |
| base | 1.8 | 8.5 | 5 | 5 | 9 |
| fast hands (all overheads x0.5) | 1.1 | 11.0 | 6 | 6 | 14 |
| slow hands (x1.5) | 2.4 | 6.8 | 4 | 4 | 9 |
| stays x1.5 | 1.7 | 8.8 | 5 | 5 | 5 |
| stays x2 | 1.4 | 9.5 | 6 | 6 | 2 |
| 3-row lot (same spot count) | 1.9 | 8.2 | 5 | 5 | 9 |

Even with all human overheads halved the lot only needs 6 spots, and longer stays collapse income (cars are
still parked when the night ends). Extrapolating (spots demanded p95 is about 0.7 x cars per night), justifying 12 spots would take roughly 18-20
cars a night, i.e. more than
one valet or a much longer night. A layout with three rows changes nothing material (the spot count is what
counts, not the shape).

## 8. Caveats and the assumptions I trust least

1. **Human overheads** (enter 1.5 s, exit 1 s, park precision 3 s, curb 2 s). The brief gives them as
   examples. They set the whole capacity of the valet; section 7 shows the effect (halving them lifts night-7
   cars from 8.5 to 11). A play-test stopwatch would replace the guess in minutes.
2. **Policy realism.** "Earliest deadline first" on the raw timers prefers parking over pickups, so pickups go
   late. A real player balances by tip value, so real income is probably higher than shown and real timeouts
   somewhat lower. The skill gap between the three policies (2-3x in task time) is a model choice.
3. **Driving.** Two axis-aligned legs, each started and stopped from rest, at 60/75/90% of top speed. Real
   cornering is probably faster for experts and slower for novices. No collisions, no blocked cars, no walls.
4. **Walking** is straight-line and unobstructed; the valet is assumed to pause for a decision only at the
   start of a job.
5. **Queue and stations.** The queue is capped at 3 slots by `LotConfig`; station positions use the config
   defaults (charging bays at (-17.5, 6) and (-14.5, 6), wash at (-11.5, 6)) and EV charging is a policy choice
   (the optional charge is skipped when the queue is long).
6. **Out of scope**: mafia jobs, strikes, bombs, story cards, damage (design sections 6, 7, 11).
7. **Statistics.** 500 nights per cell; shared random numbers across policies and spot counts (same seed per
   night), so cell-to-cell differences are cleaner than the standard error suggests (about +-1 point on a
   percentage near 20%). A few table rows are not strictly monotone in S because of this noise.
8. **Utilisation** counts all valet time, including work on cars that cannot pay (arrivals in the last minutes),
   so it reads high late in the night.

## 9. How to rerun

```
python tools/sim/selftest.py                           # tip rules, geometry, motion and invariants
python tools/sim/lot_sim.py                            # everything, 500 nights per cell, about 3 minutes
python tools/sim/lot_sim.py --quick                    # 100 nights per cell, about 40 s
python tools/sim/lot_sim.py --sections grid            # grid | runs | schedule | sens
python tools/sim/lot_sim.py --hands 0.5                # what-if: every human overhead halved
```

Output goes to the console, `tools/sim/results/report.md` (all tables) and `tools/sim/results/grid.csv` (every
night x spots x policy cell with all statistics). Files: `params.py` (all inputs), `rules.py` (tips, patron
rolls), `geometry.py` (lot layout), `policies.py` (skill and action times), `night.py` + `tasks.py` + `valet.py` +
`car.py` (one night), `stats.py` (Monte Carlo), `grid.py`, `runs.py`, `schedule.py` (search), `lot_sim.py` (CLI).

## Appendix: full grid, current `NightPlanDefaults`, spots 4-24

Triples are novice / competent / expert. "Cars/night" and "cars in lot" are for the competent valet; "lot-full
cars/night" counts cars that needed a spot and found none. Source: `tools/sim/results/grid.csv`.

### Night 1

| S | cars/night mean, p95, max | cars in lot mean, p95, max | spots used p95, max | parked % n/c/e | on-time pickups % n/c/e | timeouts % n/c/e | tips $ n/c/e | valet util % n/c/e | lot-full cars/night n/c/e |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 4 | 7.8, 8, 9 | 4.5, 5, 5 | 4, 4 | 75/87/89 | 77/72/80 | 9/0/0 | 17/25/42 | 99/99/98 | 0.1/1.5/3.2 |
| 5 | 7.8, 8, 9 | 4.5, 5, 5 | 4, 4 | 74/87/89 | 74/72/80 | 9/0/0 | 16/25/42 | 99/99/98 | 0.0/0.0/0.0 |
| 6 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 73/87/89 | 73/75/80 | 10/0/0 | 15/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 7 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 73/87/89 | 70/75/80 | 10/0/0 | 14/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 8 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 73/87/89 | 70/73/80 | 10/0/0 | 14/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 9 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 72/87/89 | 70/73/80 | 10/0/0 | 14/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 10 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 72/87/89 | 69/73/80 | 10/0/0 | 13/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 11 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 73/87/89 | 68/73/80 | 10/0/0 | 13/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 12 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 73/87/89 | 66/73/80 | 9/0/0 | 12/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 13 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 73/87/89 | 64/73/80 | 9/0/0 | 12/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 14 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 72/87/89 | 63/73/80 | 9/0/0 | 11/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 15 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 72/87/89 | 63/73/80 | 9/0/0 | 11/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 16 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 72/87/89 | 63/73/80 | 9/0/0 | 10/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 17 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 73/87/89 | 63/73/80 | 9/0/0 | 10/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 18 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 72/87/89 | 62/73/80 | 9/0/0 | 10/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 19 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 72/87/89 | 61/73/80 | 9/0/0 | 9/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 20 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 72/87/89 | 59/73/80 | 9/0/0 | 9/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 21 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 72/87/89 | 58/73/80 | 8/0/0 | 9/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 22 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 72/87/89 | 57/73/80 | 8/0/0 | 8/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 23 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 72/87/89 | 56/73/80 | 8/0/0 | 8/28/42 | 99/99/98 | 0.0/0.0/0.0 |
| 24 | 7.9, 8, 9 | 4.5, 5, 5 | 4, 4 | 72/87/89 | 55/73/80 | 8/0/0 | 7/28/42 | 99/99/98 | 0.0/0.0/0.0 |

### Night 2

| S | cars/night mean, p95, max | cars in lot mean, p95, max | spots used p95, max | parked % n/c/e | on-time pickups % n/c/e | timeouts % n/c/e | tips $ n/c/e | valet util % n/c/e | lot-full cars/night n/c/e |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 4 | 13.5, 14, 14 | 7.0, 7, 7 | 4, 4 | 35/50/59 | 30/39/58 | 42/28/19 | 1/8/21 | 99/99/99 | 5.3/9.9/10.3 |
| 5 | 13.4, 14, 14 | 8.0, 8, 8 | 5, 5 | 34/48/56 | 26/10/28 | 43/30/23 | 0/0/4 | 99/99/99 | 0.2/6.6/8.5 |
| 6 | 13.3, 14, 14 | 8.3, 9, 9 | 6, 6 | 34/46/54 | 24/5/11 | 43/31/25 | 0/0/0 | 99/99/99 | 0.0/3.2/5.8 |
| 7 | 13.3, 14, 14 | 8.3, 9, 10 | 6, 7 | 33/46/52 | 24/5/2 | 44/31/27 | 0/0/0 | 99/99/99 | 0.0/0.1/2.9 |
| 8 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 33/46/53 | 23/5/2 | 44/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |
| 9 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 33/46/53 | 21/5/2 | 44/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |
| 10 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 33/46/53 | 20/5/2 | 44/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |
| 11 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 32/46/53 | 20/5/2 | 45/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |
| 12 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 32/46/53 | 19/5/2 | 45/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |
| 13 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 31/46/53 | 18/5/2 | 45/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |
| 14 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 31/46/53 | 17/5/2 | 45/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |
| 15 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 31/46/53 | 17/5/2 | 45/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |
| 16 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 31/46/53 | 15/5/2 | 45/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |
| 17 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 30/46/53 | 17/5/2 | 46/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |
| 18 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 30/46/53 | 16/5/2 | 46/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |
| 19 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 30/46/53 | 15/5/2 | 46/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |
| 20 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 30/46/53 | 14/5/2 | 46/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |
| 21 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 30/46/53 | 14/5/2 | 46/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |
| 22 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 30/46/53 | 14/5/2 | 47/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |
| 23 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 29/46/53 | 13/5/2 | 47/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |
| 24 | 13.5, 14, 15 | 8.4, 9, 10 | 6, 7 | 29/46/53 | 13/5/2 | 47/32/26 | 0/0/0 | 99/99/99 | 0.0/0.0/0.0 |

### Night 3

| S | cars/night mean, p95, max | cars in lot mean, p95, max | spots used p95, max | parked % n/c/e | on-time pickups % n/c/e | timeouts % n/c/e | tips $ n/c/e | valet util % n/c/e | lot-full cars/night n/c/e |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 4 | 11.2, 13, 14 | 6.9, 7, 7 | 4, 4 | 35/48/58 | 34/17/55 | 36/26/18 | 3/4/30 | 99/99/99 | 1.6/6.1/9.0 |
| 5 | 11.3, 13, 14 | 7.3, 8, 8 | 5, 5 | 35/48/57 | 34/8/27 | 37/26/20 | 3/1/8 | 99/99/99 | 0.0/2.4/7.8 |
| 6 | 11.3, 13, 14 | 7.4, 8, 9 | 6, 6 | 35/48/56 | 32/7/9 | 36/25/22 | 3/1/2 | 99/99/99 | 0.0/0.4/5.1 |
| 7 | 11.3, 13, 14 | 7.4, 8, 9 | 6, 7 | 35/48/55 | 30/7/4 | 37/25/22 | 3/1/1 | 99/99/99 | 0.0/0.0/1.7 |
| 8 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 35/48/56 | 29/7/4 | 37/26/22 | 2/1/1 | 99/99/99 | 0.0/0.0/0.0 |
| 9 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 34/48/56 | 28/7/4 | 37/26/22 | 2/1/1 | 99/99/99 | 0.0/0.0/0.0 |
| 10 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 34/48/56 | 28/7/4 | 38/26/22 | 2/1/1 | 99/99/99 | 0.0/0.0/0.0 |
| 11 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 33/48/56 | 27/7/4 | 38/26/22 | 2/1/1 | 99/99/99 | 0.0/0.0/0.0 |
| 12 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 33/48/56 | 27/7/4 | 39/26/22 | 2/1/1 | 99/99/99 | 0.0/0.0/0.0 |
| 13 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 33/48/56 | 25/7/4 | 39/26/22 | 2/1/1 | 99/99/99 | 0.0/0.0/0.0 |
| 14 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 32/48/56 | 24/7/4 | 39/26/22 | 2/1/1 | 99/99/99 | 0.0/0.0/0.0 |
| 15 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 32/48/56 | 23/7/4 | 39/26/22 | 2/1/1 | 99/99/99 | 0.0/0.0/0.0 |
| 16 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 32/48/56 | 24/7/4 | 39/26/22 | 2/1/1 | 99/99/99 | 0.0/0.0/0.0 |
| 17 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 31/48/56 | 24/7/4 | 40/26/22 | 2/1/1 | 99/99/99 | 0.0/0.0/0.0 |
| 18 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 31/48/56 | 22/7/4 | 40/26/22 | 1/1/1 | 99/99/99 | 0.0/0.0/0.0 |
| 19 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 31/48/56 | 22/7/4 | 40/26/22 | 1/1/1 | 99/99/99 | 0.0/0.0/0.0 |
| 20 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 31/48/56 | 20/7/4 | 40/26/22 | 1/1/1 | 99/99/99 | 0.0/0.0/0.0 |
| 21 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 30/48/56 | 20/7/4 | 40/26/22 | 1/1/1 | 99/99/99 | 0.0/0.0/0.0 |
| 22 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 30/48/56 | 20/7/4 | 41/26/22 | 1/1/1 | 99/99/99 | 0.0/0.0/0.0 |
| 23 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 30/48/56 | 19/7/4 | 41/26/22 | 1/1/1 | 99/99/99 | 0.0/0.0/0.0 |
| 24 | 11.4, 13, 14 | 7.4, 8, 9 | 6, 7 | 30/48/56 | 19/7/4 | 41/26/22 | 1/1/1 | 99/99/99 | 0.0/0.0/0.0 |

### Night 4

| S | cars/night mean, p95, max | cars in lot mean, p95, max | spots used p95, max | parked % n/c/e | on-time pickups % n/c/e | timeouts % n/c/e | tips $ n/c/e | valet util % n/c/e | lot-full cars/night n/c/e |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 4 | 12.0, 13, 14 | 6.9, 7, 7 | 4, 4 | 34/46/53 | 38/25/47 | 39/30/23 | 5/6/18 | 99/98/98 | 1.3/4.1/4.4 |
| 5 | 12.0, 13, 14 | 7.3, 8, 8 | 5, 5 | 33/46/53 | 37/13/27 | 40/30/24 | 5/3/10 | 99/98/98 | 0.0/1.7/2.7 |
| 6 | 12.1, 13, 14 | 7.5, 9, 9 | 6, 6 | 33/46/53 | 36/12/22 | 40/29/24 | 5/3/9 | 99/98/98 | 0.0/0.3/1.1 |
| 7 | 12.1, 13, 14 | 7.5, 9, 9 | 6, 7 | 33/46/53 | 35/12/21 | 41/29/24 | 4/3/9 | 99/98/98 | 0.0/0.0/0.2 |
| 8 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 32/46/53 | 35/12/21 | 41/29/24 | 4/3/9 | 99/98/98 | 0.0/0.0/0.0 |
| 9 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 32/46/53 | 34/12/21 | 41/29/24 | 4/3/9 | 99/98/98 | 0.0/0.0/0.0 |
| 10 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 32/46/53 | 32/12/21 | 41/29/24 | 4/3/9 | 99/98/98 | 0.0/0.0/0.0 |
| 11 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 31/46/53 | 32/12/21 | 42/29/24 | 4/3/9 | 99/98/98 | 0.0/0.0/0.0 |
| 12 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 31/46/53 | 32/12/21 | 42/29/24 | 4/3/9 | 99/98/98 | 0.0/0.0/0.0 |
| 13 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 30/46/53 | 31/12/21 | 43/29/24 | 3/3/9 | 99/98/98 | 0.0/0.0/0.0 |
| 14 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 30/46/53 | 30/12/21 | 43/29/24 | 3/3/9 | 99/98/98 | 0.0/0.0/0.0 |
| 15 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 30/46/53 | 29/12/21 | 43/29/24 | 3/3/9 | 99/98/98 | 0.0/0.0/0.0 |
| 16 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 30/46/53 | 26/12/21 | 43/29/24 | 3/3/9 | 99/98/98 | 0.0/0.0/0.0 |
| 17 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 30/46/53 | 23/12/21 | 43/29/24 | 3/3/9 | 99/98/98 | 0.0/0.0/0.0 |
| 18 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 30/46/53 | 22/12/21 | 44/29/24 | 3/3/9 | 99/98/98 | 0.0/0.0/0.0 |
| 19 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 29/46/53 | 22/12/21 | 44/29/24 | 2/3/9 | 99/98/98 | 0.0/0.0/0.0 |
| 20 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 29/46/53 | 22/12/21 | 44/29/24 | 2/3/9 | 99/98/98 | 0.0/0.0/0.0 |
| 21 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 29/46/53 | 22/12/21 | 45/29/24 | 2/3/9 | 99/98/98 | 0.0/0.0/0.0 |
| 22 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 28/46/53 | 20/12/21 | 45/29/24 | 2/3/9 | 99/98/98 | 0.0/0.0/0.0 |
| 23 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 28/46/53 | 20/12/21 | 45/29/24 | 2/3/9 | 99/98/98 | 0.0/0.0/0.0 |
| 24 | 12.1, 14, 14 | 7.5, 9, 9 | 6, 7 | 28/46/53 | 19/12/21 | 45/29/24 | 2/3/9 | 99/98/98 | 0.0/0.0/0.0 |

### Night 5

| S | cars/night mean, p95, max | cars in lot mean, p95, max | spots used p95, max | parked % n/c/e | on-time pickups % n/c/e | timeouts % n/c/e | tips $ n/c/e | valet util % n/c/e | lot-full cars/night n/c/e |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 4 | 10.9, 13, 14 | 6.8, 7, 7 | 4, 4 | 34/45/54 | 37/14/44 | 36/27/21 | 6/4/20 | 99/99/99 | 0.5/3.2/5.3 |
| 5 | 10.9, 13, 14 | 7.0, 8, 8 | 5, 5 | 34/46/54 | 35/10/21 | 36/27/21 | 6/3/8 | 99/99/99 | 0.0/0.8/3.6 |
| 6 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 33/46/54 | 34/10/16 | 36/27/21 | 5/3/6 | 99/99/99 | 0.0/0.1/1.4 |
| 7 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 33/46/54 | 32/10/14 | 37/27/21 | 5/3/6 | 99/99/99 | 0.0/0.0/0.3 |
| 8 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 33/46/54 | 32/10/14 | 37/27/21 | 5/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 9 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 32/46/54 | 31/10/14 | 37/27/21 | 5/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 10 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 32/46/54 | 30/10/14 | 37/27/21 | 5/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 11 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 32/46/54 | 27/10/14 | 38/27/21 | 4/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 12 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 32/46/54 | 26/10/14 | 38/27/21 | 4/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 13 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 32/46/54 | 26/10/14 | 38/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 14 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 31/46/54 | 25/10/14 | 38/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 15 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 31/46/54 | 25/10/14 | 39/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 16 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 30/46/54 | 24/10/14 | 39/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 17 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 30/46/54 | 24/10/14 | 39/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 18 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 30/46/54 | 24/10/14 | 40/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 19 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 29/46/54 | 23/10/14 | 40/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 20 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 29/46/54 | 22/10/14 | 40/27/21 | 2/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 21 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 29/46/54 | 21/10/14 | 40/27/21 | 2/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 22 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 29/46/54 | 20/10/14 | 40/27/21 | 2/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 23 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 28/46/54 | 19/10/14 | 41/27/21 | 2/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 24 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 28/46/54 | 19/10/14 | 41/27/21 | 2/3/6 | 99/99/99 | 0.0/0.0/0.0 |

### Night 6

| S | cars/night mean, p95, max | cars in lot mean, p95, max | spots used p95, max | parked % n/c/e | on-time pickups % n/c/e | timeouts % n/c/e | tips $ n/c/e | valet util % n/c/e | lot-full cars/night n/c/e |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 4 | 10.9, 13, 14 | 6.8, 7, 7 | 4, 4 | 34/46/54 | 37/15/43 | 36/27/21 | 6/4/20 | 99/99/99 | 0.6/3.2/5.3 |
| 5 | 11.0, 13, 14 | 7.0, 8, 8 | 5, 5 | 33/46/54 | 35/10/23 | 36/27/22 | 5/3/8 | 99/99/99 | 0.0/0.9/3.7 |
| 6 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 33/46/54 | 34/10/16 | 36/27/22 | 5/2/6 | 99/99/99 | 0.0/0.1/1.4 |
| 7 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 33/46/54 | 33/10/15 | 37/27/22 | 5/2/6 | 99/99/99 | 0.0/0.0/0.3 |
| 8 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 33/46/54 | 33/10/15 | 37/27/22 | 5/2/6 | 99/99/99 | 0.0/0.0/0.0 |
| 9 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 33/46/54 | 31/10/15 | 37/27/22 | 4/2/6 | 99/99/99 | 0.0/0.0/0.0 |
| 10 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 32/46/54 | 30/10/15 | 37/27/22 | 4/2/6 | 99/99/99 | 0.0/0.0/0.0 |
| 11 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 32/46/54 | 30/10/15 | 38/27/22 | 4/2/6 | 99/99/99 | 0.0/0.0/0.0 |
| 12 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 31/46/54 | 29/10/15 | 38/27/22 | 4/2/6 | 99/99/99 | 0.0/0.0/0.0 |
| 13 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 31/46/54 | 29/10/15 | 38/27/22 | 4/2/6 | 99/99/99 | 0.0/0.0/0.0 |
| 14 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 31/46/54 | 27/10/15 | 39/27/22 | 3/2/6 | 99/99/99 | 0.0/0.0/0.0 |
| 15 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 30/46/54 | 26/10/15 | 39/27/22 | 3/2/6 | 99/99/99 | 0.0/0.0/0.0 |
| 16 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 31/46/54 | 27/10/15 | 39/27/22 | 3/2/6 | 99/99/99 | 0.0/0.0/0.0 |
| 17 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 30/46/54 | 27/10/15 | 39/27/22 | 3/2/6 | 99/99/99 | 0.0/0.0/0.0 |
| 18 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 30/46/54 | 25/10/15 | 39/27/22 | 3/2/6 | 99/99/99 | 0.0/0.0/0.0 |
| 19 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 30/46/54 | 25/10/15 | 40/27/22 | 3/2/6 | 99/99/99 | 0.0/0.0/0.0 |
| 20 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 29/46/54 | 25/10/15 | 40/27/22 | 3/2/6 | 99/99/99 | 0.0/0.0/0.0 |
| 21 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 29/46/54 | 24/10/15 | 40/27/22 | 3/2/6 | 99/99/99 | 0.0/0.0/0.0 |
| 22 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 29/46/54 | 22/10/15 | 41/27/22 | 2/2/6 | 99/99/99 | 0.0/0.0/0.0 |
| 23 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 29/46/54 | 20/10/15 | 41/27/22 | 2/2/6 | 99/99/99 | 0.0/0.0/0.0 |
| 24 | 11.0, 13, 14 | 7.0, 8, 9 | 5, 6 | 29/46/54 | 20/10/15 | 41/27/22 | 2/2/6 | 99/99/99 | 0.0/0.0/0.0 |

### Night 7

| S | cars/night mean, p95, max | cars in lot mean, p95, max | spots used p95, max | parked % n/c/e | on-time pickups % n/c/e | timeouts % n/c/e | tips $ n/c/e | valet util % n/c/e | lot-full cars/night n/c/e |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 4 | 10.9, 13, 14 | 6.8, 7, 7 | 4, 4 | 34/46/54 | 36/15/43 | 36/27/21 | 6/4/20 | 99/99/99 | 0.6/3.2/5.1 |
| 5 | 10.9, 13, 14 | 7.0, 8, 8 | 5, 5 | 34/46/54 | 35/10/22 | 36/27/21 | 6/3/8 | 99/99/99 | 0.0/0.9/3.6 |
| 6 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 34/46/54 | 35/10/15 | 36/27/21 | 5/3/6 | 99/99/99 | 0.0/0.1/1.5 |
| 7 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 33/46/55 | 32/10/14 | 36/27/21 | 5/3/6 | 99/99/99 | 0.0/0.0/0.3 |
| 8 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 33/46/55 | 32/10/14 | 37/27/21 | 5/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 9 | 10.9, 13, 14 | 7.0, 8, 9 | 5, 6 | 32/46/55 | 33/10/14 | 37/27/21 | 5/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 10 | 10.9, 13, 15 | 7.0, 8, 9 | 5, 6 | 32/46/55 | 32/10/14 | 37/27/21 | 5/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 11 | 10.9, 13, 15 | 7.0, 8, 9 | 5, 6 | 32/46/55 | 30/10/14 | 37/27/21 | 4/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 12 | 10.9, 13, 15 | 7.0, 8, 9 | 5, 6 | 32/46/55 | 28/10/14 | 38/27/21 | 4/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 13 | 10.9, 13, 15 | 7.0, 8, 9 | 5, 6 | 31/46/55 | 27/10/14 | 38/27/21 | 4/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 14 | 10.9, 13, 15 | 7.0, 8, 9 | 5, 6 | 31/46/55 | 26/10/14 | 38/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 15 | 10.9, 13, 15 | 7.0, 8, 9 | 5, 6 | 31/46/55 | 26/10/14 | 38/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 16 | 10.9, 13, 15 | 7.0, 8, 9 | 5, 6 | 31/46/55 | 27/10/14 | 39/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 17 | 10.9, 13, 15 | 7.0, 8, 9 | 5, 6 | 30/46/55 | 26/10/14 | 39/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 18 | 10.9, 13, 15 | 7.0, 8, 9 | 5, 6 | 30/46/55 | 25/10/14 | 39/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 19 | 10.9, 13, 15 | 7.0, 8, 9 | 5, 6 | 30/46/55 | 25/10/14 | 40/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 20 | 10.9, 13, 15 | 7.0, 8, 9 | 5, 6 | 29/46/55 | 24/10/14 | 40/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 21 | 10.9, 13, 15 | 7.0, 8, 9 | 5, 6 | 29/46/55 | 22/10/14 | 40/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 22 | 10.9, 13, 15 | 7.0, 8, 9 | 5, 6 | 29/46/55 | 23/10/14 | 40/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 23 | 10.9, 13, 15 | 7.0, 8, 9 | 5, 6 | 29/46/55 | 22/10/14 | 41/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |
| 24 | 10.9, 13, 15 | 7.0, 8, 9 | 5, 6 | 28/46/55 | 22/10/14 | 41/27/21 | 3/3/6 | 99/99/99 | 0.0/0.0/0.0 |

