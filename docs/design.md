# Uncanny Valet: Design Document

The single source of truth for every agent. If code and this doc disagree, this doc wins until the lead
changes it. Only the lead edits this file, and every edit gets an entry in `docs/changelogs/lead.md`.

Combined from three design docs written on Nov 2–5, 2026: the Game Design Doc, the Game Object
Architecture, and the Story Cards.

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
| Engine | Unity 6.3 LTS, Universal 2D template |
| Platform | Web (WebGL) first, deployed to GitHub Pages |
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
| EV charge | EV lots | Park in an EV spot | Required if on ticket |

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

1. A night runs on a clock (1 real second = 1 game minute, 22:00–04:00).
2. Patrons arrive at the curb every so often; each car gets a ticket with a stay length.
3. The valet walks (WASD/arrows), presses **E** to enter the car, drives it into a free spot, stops, and
   presses **E** to exit. The spot records the car as parked.
4. When the stay ends, a pickup is requested. The valet drives the car back to the curb and exits.
5. The ticket closes; the tip (base + speed bonus − wait penalty, never below $0) goes to the ledger.
6. The car drives off (despawns). The overlay shows clock, debt left, tips tonight and open tickets.

**Out of scope for P1:** mafia jobs, stations, faults, damage, raids, chase, story cards, pixel art,
audio, menus. Placeholder art is coloured rectangles generated in code.

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
