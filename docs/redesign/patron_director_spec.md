# Uncanny Valet: Patron Director Spec (Nov 9, 2026, @Sourabh Saha)

Spec for a Patron Director: the system that decides when the next car arrives and what kind of car it is, by reading the lot and the player's workload. It replaces today's fixed 15-25 s arrival timer. Written for the Claude Code agents on pivot/chaos.

## 1. Goal

A pacing AI in the spirit of Left 4 Dead's director: it watches how busy the player is and shapes the next few seconds to hit a target intensity. Three jobs:
1. Pace the shift. Build up, peak, give a breather, peak harder. Never a flat drip of cars.
2. Match the player. Ease off when they are drowning, push when they are idle or cruising.
3. Create decisions, not just load. Deliberately send the car that collides with what the player already did: a dirty car when a washed car is still sitting in the wash slot, an EV when both chargers hold finished EVs. The player must stop and choose: move the old car out, queue the new one, or park it and come back.

Good feels like: the player rarely has nothing to do for more than 5 seconds, rarely has more than two urgent things at once, and two or three times per shift says "oh no" because of something they set up themselves.
Never: create a problem with no solution, punish the player twice for one mistake, or make the star score depend on luck.

## 2. Inputs

The director never queries scene objects. A `LotStateTracker` rule system listens to existing car and ticket events and keeps a read-only lot snapshot; the director reads that snapshot once per tick (every 0.5 s).

### Car states (every open ticket's car is in exactly one)
| State | Meaning |
| --- | --- |
| Arriving | Driving in to the arrival slot |
| WaitingToPark | Stopped at the arrival slot, park timer running |
| Driven | The valet is in it |
| Parked | In a regular spot, stay not over |
| InStation | In the wash slot or an EV bay (see station states) |
| PickupRequested | Patron at the curb, pickup timer running |

Each car also carries its needs: NeedsWash, NeedsCharge (EV below full), and whether those are done.

### Station states (wash slot, EV bays 13 and 14), each in one of five states
| Station state | Meaning | Decision it can create |
| --- | --- | --- |
| Free | Empty | None |
| Working | Holds a car that needs it and is mid-service (washing, charging) | Queue: a second car needing it must wait or park elsewhere first |
| Blocked | Holds a car whose service is done (washed car, fully charged EV) | Evict: move the finished car out before the new one can go in |
| Squatted | Holds a car that does not need it (a plain car in an EV bay) | Evict, and a lesson about using bays as overflow |
| Reserved | A car is being driven toward it right now | None yet |

Regular spots: the snapshot counts FreeSpots and TotalSpots.

### Player workload
Workload W is the seconds of work the player owes, divided by a 30-second horizon. W = 1.0 means "exactly busy"; above 1.3 is drowning; below 0.3 is idle.

| Owed task | Estimated cost (s) |
| --- | --- |
| Park a waiting car | 8 |
| Deliver a requested pickup | 10 |
| Wash still needed (drive to wash + wash time left + drive out) | 6 + wash left |
| Charge still needed (drive to bay; charging runs on its own) | 6 |
| Evict a blocking or squatting car | 6 |
| Parked car whose pickup is due within 20 s | 5 (work about to land) |

Costs are tuning values, later measured from play-test telemetry.

Pressure P = 1 - (smallest patience fraction among waiting and pickup tickets): how close the nearest patron is to anger. W says how much work there is; P says how urgent it is.

Form F is a slow-moving read of how well the player is doing, updated every 15 s: tip rate against the 3-star pace, angry patrons in the last 60 s, and current combo. Range -1 (struggling) to +1 (cruising); moves at most 0.25 per update.

## 3. When: intensity curve and arrival pacing

Each lot has a target intensity curve T(t): the workload the director aims for at each second of the shift. It is an AnimationCurve in the lot's DirectorProfile. Shape: build-up, peak, breather, harder peak, breather, finale.

Every tick: gap = W - (T + 0.15 x F).

| Situation | Rule | Next arrival |
| --- | --- | --- |
| Idle | W < 0.3 for 5 s | Spawn now (no dead air) |
| Under target | gap < -0.15 | Shorten the countdown: interval x 0.6 |
| On target | -0.15 <= gap <= 0.15 | Base interval (15-25 s, from ShiftConfig) |
| Over target | gap > 0.15 | Lengthen: interval x 1.5 |
| Drowning | W > 1.4, or P > 0.85, or 2 angry patrons in 30 s | Relief: hold all arrivals for 10 s and send only plain cars for the next 2 |
| Queue full | 3 cars already waiting | Hold (existing cap) |

The countdown is re-scaled smoothly each tick rather than reset. Relief is the most important rule: it turns a death spiral into a comeback.

## 4. What: car type and dilemma rules

When an arrival is due, the director picks an archetype by weighted random. Weights start from the lot's profile and are then multiplied by rules that read the snapshot.

| Archetype | Needs | Base weight (Diner) | Load |
| --- | --- | --- | --- |
| Plain | Park, return | 50 | Low |
| Dirty | Wash, then park | 20 | High: occupies the single wash slot |
| EV | Charge before pickup for a bonus | 20 | Medium |
| Dirty EV | Wash and charge | 10 | Highest: two stations in sequence |
| Impatient (later) | Short stay, fast patience drain | - | Urgency, not volume |

An archetype only enters the draw if the lot has unlocked its station.

Load scaling before the dilemma rules: under target, Dirty and Dirty EV weights x 1.5; over target, x 0.5 and Plain x 1.5. In relief mode only Plain is allowed.

### Dilemma rules (each a small class behind IDilemmaRule)
| Rule | Trigger | Effect | Decision forced |
| --- | --- | --- | --- |
| Wash blocked | Wash slot Blocked (washed car still in it) | Dirty x 3 | Move the clean car out to make room |
| Wash busy | Wash slot Working, nothing queued | Dirty x 1.5 | Queue it at the arrival slot, or park it and come back later |
| Bays blocked | Every EV bay Blocked (fully charged EVs) | EV x 3 | Evict a charged EV to a regular spot |
| Bay squatted | An EV bay Squatted by a non-EV | EV x 2.5 | Pay for using the bay as overflow |
| Last bay | Exactly one bay Free, the other Working | EV x 1.5 | Use the last bay now or save it |
| Lot tight | Free regular spots <= 2 | Plain x 1.5, and bring one parked patron's return forward | Deliver first to free a spot |
| Chain | Both stations Blocked | Dirty EV x 2 | Plan the order: wash first or charge first |

### Dilemma budget
- One at a time: a rule boost applies only when no director-made dilemma is still unresolved. It resolves when the blocked station frees, or when the new car is parked anywhere.
- Cooldown of 25 s after a dilemma resolves.
- Only with headroom or in a wave: the pacing gap must be <= 0.15, or the curve must be rising. Never during relief.
- Must be solvable: at least one free regular spot, or a pickup due within 10 s that will free one. Otherwise the rule does not fire.
- No streaks: at most two Dirty (or two EV) arrivals in a row.

When a boosted draw actually picks the boosted archetype, publish `DilemmaCreated` with the rule's name (debug overlay, later a results stat "Dilemmas solved").

Worked example (Diner, 1:05): wash slot Blocked by a clean car, W = 0.6, T = 0.8 rising, 4 free spots, no open dilemma, cooldown clear. Weights: Plain 50, Dirty 20 x 1.5 (under target) x 3 (wash blocked) = 90, EV 20, Dirty EV 15. The draw picks Dirty; DilemmaCreated(WashBlocked) fires. If the player had ignored it and W crossed 1.4, relief would hold arrivals instead.

## 5. Patron return timing and guardrails

The director may nudge when the patron leaves the diner, within limits:
- Each patron has a return window: planned stay +/- 25%. The director can only move the return inside it, and only before the patron has left the diner.
- Bring forward one return when the lot is tight (free spots <= 2) or the player is under target.
- Hold back returns when over target or in relief.
- Spread pickups: no two patrons reach the curb within 4 s of each other unless the curve is in a wave peak.

| Guardrail | Rule |
| --- | --- |
| Bounded arrivals | Each lot sets a base arrival count (Diner: 14). The director may deliver 14 +/- 2, never more |
| Bounded mix | Over a shift each archetype's share stays within +/-10 points of its base share |
| Fair stars | Star thresholds assume the base count and mix, not the director's output |
| Solvable | A dilemma needs a free spot or an imminent pickup |
| No double punishment | After an angry patron, no dilemma for 20 s |
| Deterministic | All randomness uses one seeded RNG per shift; the director reads only the snapshot |
| Assist hook | Assist mode clamps form F at 0 and halves dilemma weights |

The director is a conductor, not a judge: the score is the same size for everyone.

## 6. Architecture for agents

A new `Director` assembly referencing only Core. Every decision is a pure class with EditMode tests; one thin MonoBehaviour ticks it. It replaces the fixed arrival timer in the existing patron spawner, which becomes a dumb executor of `PatronScheduled`.

| Part | Kind | Responsibility |
| --- | --- | --- |
| LotStateTracker | Rule system (owner) | Listens to car, ticket and station events; owns the LotSnapshot. Only writer |
| LotSnapshot | Read-only struct | Cars by state and needs, station slot states, free spots, open tickets with patience fractions |
| WorkloadEstimator | Pure | Snapshot -> W and P (cost table from config) |
| FormTracker | Rule system | Rolling F from score, combo and complaint events |
| ArrivalPacer | Pure | (W, P, T, F, queue, budget left, time left) -> seconds until next arrival, or Hold |
| ArchetypePicker | Pure | Snapshot + profile + pacing gap + active dilemma rules + RNG -> archetype and the rule that boosted it |
| IDilemmaRule | Interface, one class per rule | Evaluate(snapshot) -> (archetype, multiplier, isSolvable) |
| ReturnScheduler | Pure | Adjusts return times of patrons still inside the diner |
| PatronDirector | MonoBehaviour shell | Ticks every 0.5 s, calls the above, publishes events |
| DirectorProfile | ScriptableObject per lot | Curve, base weights, arrival budget, rule list and multipliers |
| DirectorConfig | ScriptableObject | Task costs, thresholds, cooldowns; every value an ITuning key for the F2 panel |

Budget-aware pacing: ArrivalPacer also spreads the remaining arrival budget over the remaining area under T.

New events (Core/Events): PatronScheduled(archetype, reason), DirectorPhaseChanged(phase), ReliefStarted, ReliefEnded, DilemmaCreated(rule, carId), DilemmaResolved(rule, seconds), PatronReturnShifted(ticketId, deltaSeconds). Station events for the tracker: StationEntered, StationServiceDone, StationExited, if not already published.

Sketch (shape, not final code; match the project's naming, event bus and Services patterns):

```csharp
public enum StationSlotState { Free, Working, Blocked, Squatted, Reserved }
public enum CarArchetype { Plain, Dirty, Ev, DirtyEv }

public interface IDilemmaRule
{
    string Id { get; }
    DilemmaBoost? Evaluate(in LotSnapshot lot); // null when the trigger does not hold
}

public readonly struct DilemmaBoost
{
    public readonly CarArchetype Archetype;
    public readonly float Multiplier;
    public readonly bool Solvable;
}

public sealed class WashBlockedRule : IDilemmaRule
{
    // Wash.State == Blocked -> Dirty x mult; solvable if FreeSpots > 0 || SecondsToNextPickup < 10
}

public sealed class ArchetypePicker
{
    // unlocked-only weights -> ScaleForGap -> (if gate open) multiply solvable rule boosts
    // -> ApplyStreakLimit(recent, 2) -> Draw(rng); report the boosting rule id if the boosted archetype won.
}
```

## 7. Tuning, tests and rollout

Starting values (all ITuning keys): tick 0.5 s; workload horizon 30 s; on-target band +/-0.15; form influence 0.15; idle spawn W < 0.3 for 5 s; relief trigger W > 1.4, P > 0.85 or 2 angry in 30 s; relief length 10 s hold then 2 plain cars; dilemma cooldown 25 s (20 s blackout after an angry patron); streak limit 2; return window +/-25%; pickup spacing 4 s; Diner arrival budget 14 +/- 2.

Tests (EditMode):
- WorkloadEstimator: each task type adds its cost; an empty lot gives W = 0.
- ArrivalPacer: idle spawns now; drowning holds; the queue cap holds; the budget is never exceeded.
- Each IDilemmaRule: fires on its trigger state, not others; reports unsolvable with zero free spots and no pickup due.
- ArchetypePicker: no locked archetypes; relief gives only Plain; the streak limit holds; at most one dilemma open; same seed same picks.
- ReturnScheduler: never moves a return outside its window or after the patron left the diner.
- Soak test: a scripted bot plays 200 seeded shifts (always parks in the nearest spot, delivers oldest first). Assert the arrival count stays within budget, every dilemma was solvable when made, and no shift has more than 20 s without an arrival or open task.

Debug overlay (F3), next to the F2 tuning panel: live W against T with phase name and relief periods shaded; each station slot with its state colour; the last five director decisions with reasons (for example "1:05 sent Dirty (WashBlocked x3, under target)"); counters: arrivals used / budget, dilemmas made / resolved, average resolve time.

Rollout (each step behind a `DirectorConfig.Enabled` flag so a play-test can compare director on and off in the same build):
1. Snapshot first: LotStateTracker and station slot states, shown in the F3 overlay. No behaviour change.
2. Pacing: WorkloadEstimator, FormTracker, ArrivalPacer; the archetype stays random as today.
3. Dilemmas: ArchetypePicker with the wash and EV rules, then the budget and guardrails.
4. Returns: ReturnScheduler last.
5. Measure: log each shift's decisions and real task times; replace cost table guesses with measured averages.
