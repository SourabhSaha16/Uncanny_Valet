# Uncanny Valet: Design Document

The single source of truth for every agent. If code and this doc disagree, this doc wins until the lead
changes it. Only the lead edits this file, and every edit gets an entry in `docs/changelogs/lead.md`.

Combined from three design docs written on Nov 2–5, 2026: the Game Design Doc, the Game Object
Architecture, and the Story Cards.


> **CHAOS REDESIGN (branch `pivot/chaos`, from 2026-11-09).** `docs/redesign/chaos_redesign.md` overrides this
> document wherever they conflict. The mafia, debt, strikes, boss orders and phone UI are cut; the game becomes
> 4-minute level-based shifts scored by tips, combo and stars (Overcooked style). Sections below describe the
> old game until the lead folds the redesign in; agents follow the redesign first.

---

## 1. Overview

You are a casino valet working off a mafia debt. Every night you park cars for tips, and you also carry
out the boss's dirty jobs, which pay nothing but get you killed if you botch them.

| Field | Value |
| --- | --- |
| Working title | Uncanny Valet |
| Genre | Arcade time-management + driving, with a light crime layer |
| Perspective | Top-down, early-GTA style |
| Art style | 2D pixel art |
| Engine | Unity 6000.2.5f1, Universal 2D template |
| Platform | Web (WebGL) first, built locally and deployed to GitHub Pages from `site/` |
| Session shape | One night = one level; a run = X nights |

**The title:** the valet is not what he seems. To the patrons he is a smiling, helpful valet; behind their
backs he sabotages their cars. The double life shows **only in words**, never in facial expressions or
sprite changes: when you hand back a sabotaged car, the goodbye line is a pun on what you did to it.

| Sabotage | Goodbye line (example) |
| --- | --- |
| Timed bomb | "Have a blast tonight!" |
| Drugs in boot | "Enjoy the trip home!" |
| Fuel drain | "Don't run out on us now!" |
| Engine or part swap | "She'll run like a whole new car." |

## 2. Core story, win and lose

You owe a mafia boss **Y dollars** and have **X nights** to repay it. Patron tips are your only income.
Tips are deducted from the debt automatically at the end of each night and can't be spent on anything.

**The run is lost** when night X ends with debt left, or on the **3rd strike** (strikes count across the
whole run and never reset). When the run is lost, a **surprise final chase** may trigger if hidden
conditions are met (conditions TBD, never shown to the player).

| Ending | How you get it |
| --- | --- |
| Paid off (win) | Clear the debt by the end of night X |
| You got away (win) | Lose the run, the chase triggers, and you escape the boss |
| Death | Lose the run, and either the chase doesn't trigger or you're caught in it |

**Bomb in the lot:** a bomb that goes off while its car is still in the lot is a botched job (a strike) and
damages one car or every car in a radius, depending on the bomb. Bombs get bigger as the game goes on.

## 3. Design pillars

1. **Patron jobs never cost you money.** The tip ranges from $0 upward, never below.
2. **Mafia jobs pay nothing and must be done right.** Compulsory, hard criteria; failure is a strike.
3. **Pressure ramps up.** Exponential with a long, flat start. A bell curve per unlock is the alternative
   to explore in playtests.
4. **Top-down, early-GTA readability.** You walk between cars on foot.
5. **Driving keeps you on your toes.** Each car handles differently; faults (e.g. swapped steering) change
   the controls.

## 4. Core loop (one night)

Boss assigns jobs → patron hands over keys → park and service the car → (sabotage if marked) → return the
car on pickup → earn tip → next patron. End of night: tips pay the debt, next night is harder.

## 5. Patron jobs and tips

A patron hands over keys with a **ticket** listing every request and any fault. Tip = base, raised for
speed and requests met, lowered for misses and damage, never below $0.

| Job type | Unlocked by | What the player does | Tip effect |
| --- | --- | --- | --- |
| Regular parking | Start | Park the car, bring it back on request | Base tip |
| Impatient trait | Start | Same job; trait shown on ticket | Bigger bonus if returned within Z minutes |
| Dirty car | Cleaning station | Clean before return | Required if on ticket |
| Faulty car | Mechanic station | Fix before return | Required if on ticket |
| EV charge | Charging station (night 3) | Charge the EV at a station before pickup | Charge bonus if fully charged |

- Ticket requests are requirements; a miss lowers the tip, possibly to $0.
- Damage is a 0–100% meter. Higher damage, lower tip. The mechanic repairs it.

## 6. Mafia jobs and strikes

The boss marks **plain** cars (no ticket requests). Orders arrive on a small **phone UI** that also shows
the time. A failed job is a strike.

| Job | Unlocked by | Pass criteria | Failure |
| --- | --- | --- | --- |
| Drugs in boot | Start | Package in target's boot when it leaves | Wrong car |
| Fuel drain | Start | Fuel at X when returned on time | Too low: dies in lot. Too high: fails |
| Part swap | Mechanic | Boss's part fitted before pickup | Wrong part / late |
| Timed bomb | Mechanic | Goes off after the car leaves | In the lot: strike + damage |

Detection (guards, cameras with vision cones) is an optional, later module.

## 7. Mini-games

**Police raid:** at random points (not after every job), police are tipped to a lot number. Swap a clean car
into that lot before they arrive. Failure is a strike (the boss has to pay off the police).

**Final chase:** see section 2. Pick a fast car and outrun the boss while being shot at. Faults from that
night's tickets carry over into the chase.

## 8. Upgrades, driving, difficulty

- The casino buys upgrades on a **fixed schedule** (nights TBD): cleaning station, mechanic station
  (also unlocks part swaps and bombs), EV lots.
- Handling stats per car: top speed, acceleration, turn rate, grip, braking. The ticket shows any fault.
- Difficulty dials: patrons per night, preferences, mafia complexity, car handling.

## 9. Architecture

Four layers; systems talk only through a typed event bus. The car is the hub.

1. **Definitions:** ScriptableObject assets, no runtime state (CarDefinition, PatronDefinition, ...).
2. **World entities:** GameObjects built from small components (car: driver, faults, fuel, damage, boot).
3. **Rule systems:** react to events, change entities (TicketRegistry, TipCalculator, ...).
4. **Run state owners:** one owner per value (NightClock, DebtLedger, StrikeTracker).

Principles: data separate from behaviour; composition over inheritance; one owner per piece of state;
systems talk through events; new rules are new classes behind an interface; rules are pure and
unit-testable; optional features are modules.

Patron requests and mafia jobs share one **objective** model: a list of conditions checked against a car
plus an outcome policy (tip effect, or strike on fail). The police raid reuses the same conditions.

### Prototype contracts (Assets/_Project/Scripts/Core)

| Contract | Implemented by | Purpose |
| --- | --- | --- |
| `IEventBus` / `EventBus` | Core | Typed publish/subscribe |
| `Services` | Core | Registry filled by the bootstrap; no other singletons |
| `IDrivable` | Cars | A car the valet can drive; exposes speed and driver state |
| `ICarFactory` | Cars | Spawns and despawns cars by `CarId` |
| `IInteractable` | Cars (and later stations) | Anything the valet can use with the interact key |
| `IInputSource` | Valet | Move vector + interact press, shared by walking and driving |
| `ILotLayout` | Lot | Arrival point, curb zone, spot list |
| `IGameClock` | Night | Night number and minute of night |
| `ITipCalculator` | Economy | Pure tip maths, clamped at $0 |
| `IDebtView` | Economy | Read-only debt and tips tonight |

Events live in `Core/Events`: car events (spawned, entered, exited, parked, left spot, at curb,
departed), ticket events (opened, pickup requested, closed), economy events (tip earned, debt changed),
night events (started, tick, ended).

## 10. Prototype scope (milestone P1)

Goal: **pick up and drop off, end to end, in a browser.** No UI beyond a debug overlay.

1. A night runs on a clock (1.5 game minutes per real second, 22:00–01:00, about 2 real minutes).
2. Patrons arrive at the curb every so often; each car gets a ticket with a stay length.
3. The valet walks (WASD/arrows), presses **E** to enter the car, drives it into a free spot, stops, and
   presses **E** to exit. The spot records the car as parked.
4. When the stay ends, a pickup is requested. The valet drives the car back to the curb and exits.
5. The ticket closes; the tip (base + speed bonus − wait penalty, never below $0) goes to the ledger.
6. The car drives off (despawns). The overlay shows clock, debt left, tips tonight and open tickets.

**Pressure rules (play-test round 2):**

- **Letters and numbers:** each open ticket's car shows a letter (A, B, C: the lowest free one, kept until
  the ticket closes). Parking spots show numbers. The overlay pairs them ("A in spot 3").
- **Queue:** up to `MaxWaitingCars` (3) cars can wait for parking at once, in queue slots behind the
  arrival point. A new car arrives only when a slot is free.
- **Park timer:** starts when the car arrives (`ParkWindowMinutes`, default 45). It pauses while the valet
  is driving that car and stops when the car is parked. If it runs out, the patron takes the car and
  leaves: the car vanishes, tip $0, no strike, no penalty (pillar 1).
- **Pickup timer:** starts when the pickup is requested (`PickupWindowMinutes`, default 60). If it runs
  out, the car must still be delivered (it blocks its spot until then) but the tip is $0.
- **Timer visual:** a dark fill rises over the car as its timer runs out (bottom to top); the pickup timer
  fill is red-tinted.
- Cars are heavy: the valet on foot cannot push them.

**Night length and summary (play-test round 3):** a night is 180 game minutes (3 real minutes, 22:00 to
01:00), tunable in `RunConfig`. When it ends, every open ticket closes with $0 and its car leaves, the
clock stops and a **summary card** shows tips collected, cars parked, cars not parked and debt left, with a
**Next** button. After the final night the card says whether the debt is paid off and the button starts a
new run. Cars not parked = cars that vanished or were still waiting when the night ended.

**Audio:** a quiet, generated funk groove loops under the game (low volume) and short generated sound
effects mark key events (enter or exit car, parked, tip, pickup request, car vanishes, night end, button).
All audio is synthesised in code for now, behind an `AudioConfig` (volumes, tempo); real files can replace
it later. M mutes.

**Night length (play-test round 4):** a night is still 22:00 to 01:00 on the clock but runs in 2 real
minutes (clock at 1.5 game minutes per second, `RunConfig`). Park and pickup timers stay in game minutes,
so they shrink in real seconds by the same ratio (park 45 min = 30 s, pickup 60 min = 40 s).

**Challenge schedule (7 nights, one new thing at a time).** Driven by a `NightPlan` per night (Core struct,
data in `NightPlanConfig`). What is new on night N+1 is listed on night N's end card, under the tip
calculation (base, speed, charge bonus, clean bonus, zeroed tips).

| Night | Challenge |
| --- | --- |
| 1 | Simple: one car at a time. A new car arrives only after the previous one is parked |
| 2 | Messy: up to 3 cars waiting, parallel pickups and drop-offs, busier arrivals |
| 3 | EV charging: some cars are EVs (blue body, small blue charge bar). A charging station (2 bays) is unlocked |
| 4 | Dirty cars (muddy) must be washed, then parked. One car wash slot, with a timer. Charging and EVs stay on (unlocks layer, never leave) |
| 5 | Everything at once: parallel cars, several EVs and several dirty cars |
| 6 | Same as night 5 |
| 7 | Same as night 5 |

- **EV:** an ordinary car, but handed back with a full charge it earns a **charge bonus**. It charges only
  while stopped in a charging bay (timed, about 20 game minutes from empty; the bar shows progress).
  No penalty for an uncharged EV.
- **Dirty car:** must be washed in the single wash slot (about 15 game minutes) and then parked. Washed =
  **clean bonus**. Handed back dirty = $0 tip (design.md section 5: required, a miss can zero the tip). The
  slot frees as soon as the washed car drives out; the next dirty car can then go in.
- The park timer pauses while a car is being driven or is stopped in a station, and resumes after.
- **Pacing and lot size (round 5, tunable live):** per night, arrival gap scale / waiting cap / open regular
  spots: N1 2.0/1/4, N2 1.5/3/5, N3 1.3/3/6, N4 1.2/3/6, N5 1.1/3/7, N6 1.0/3/7, N7 0.9/3/8. These sit between
  the too-easy balance.md Schedule A and the too-hard original; the debug panel (F2) tunes them. Spots that are not open
  do not exist: the lot and its walls grow when more open, numbered column by column from the curb. Starting debt is $500 (round 6, pacing unchanged).
- **Charging spots:** two numbered spots (ids after the regular ones) while charging is unlocked, not counted in the
  open-spot number. Any car may park in one; only EVs charge there. A charged EV must be moved out to free it.
- **EV surge:** from `LateEvStart` (0.6 of the night) the EV chance ramps from `EvChance` to `LateEvChance` (0.75)
  on nights 3-7, so the player has to move a charged EV to a regular spot.
- **Urgency audio:** the music tempo rises with the number of cars waiting to be parked or picked up.
- **Live tuning:** every pacing number is an `ITuning` key; F2 opens a panel over the running game (sliders, per-row reset, Reset all).
- Sabotage jobs stay off.
- **Mirrors:** every car has two small side mirrors at the front as a visual cue for which end is the front.

**Out of scope for P1:** mafia jobs, stations, faults, damage, raids, chase, story cards, pixel art,
recorded audio, menus. Placeholder art is coloured rectangles generated in code.

**Web rules:** no threads, no `System.IO` file access, no blocking calls; build compression disabled so
GitHub Pages can serve it.

## 11. Story cards (later milestones)

A story card is a scripted scene inside a night: a named patron and car, a ticket, sometimes a boss
order, a twist and a goodbye line. Each night draws **5 cards (tunable)** from its act's deck; procedural
patrons fill the rest. Chain flags reset every run. Bomb targets always leave at their stated time.
Story-critical cards are marked *guaranteed*. Cards randomise looks but keep dialogue and objectives.

Card parameters: act / required unlock, night window, frequency, chain; patron name, traits, arrival and
pickup, arrival line; car definition, faults, starting state; ticket requests; boss order, criteria,
briefing line, goodbye pun; other patrons, raid; twist.

| # | Card | Teaches / remixes |
| --- | --- | --- |
| 1 | First Shift | Parking (Mrs. Okafor, sets *met Okafor*) |
| 2 | The Dentist | Impatient trait |
| 3 | Special Delivery | Drugs in boot |
| 4 | Mirror, Mirror | Swapped-steering fault |
| 5 | Running on Fumes | Fuel drain |
| 6 | Twins | Read the plate; drugs in the right boot |
| 7 | Rush Hour | Juggling three patrons |
| 8 | Anonymous Tip | First scripted raid |
| 9 | Mud Season | Cleaning station |
| 10 | Lemon | Mechanic repair |
| 11 | Heart Transplant | Part swap |
| 12 | Have a Blast | Single-car bomb |
| 13 | Double Shift | Two orders in one night |
| 14 | The Critic | Impatient + clean |
| 15 | Hot Lot | Raid on a live bomb |
| 16 | Family Business | Fuel drain on the boss's nephew |
| 17 | Green Light | EV charging |
| 18 | Full Service | Clean + repair + charge |
| 19 | Blast Radius | Area bomb |
| 20 | Gridlock | Five patrons, raids allowed |
| 21 | Inside Job | Part swap + area bomb |
| 22 | The Regular Returns | Okafor callback, drugs |
| 23 | The Boss's Own | Boss's limo; rival tips police |

## 12. Open questions

Target X nights and Y debt; unlock nights; value of Z; damage-to-tip curve; hidden chase conditions;
guaranteed cards; whether card 23's rival move needs its own mechanic; raid false alarms; save system.
