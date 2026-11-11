# Valet Powers (draft, not binding until the open questions are answered)

Source: the human's note of 2026-11-10. Status: planned, not built. The lead turns this into a binding spec when the open questions in section 6 are answered.

## 1. The idea

A vertical **power meter** on the side of the screen fills as the valet does impressive things. It listens to events only (no module polls it). At set levels of the meter the player can spend it on a **power**. Spending resets the meter to empty. Powers stack: a still-running earlier power keeps going while the player refills the meter and fires a higher one.

## 2. What fills the meter (the meter listens to events)

| Source | Event | Fill |
| --- | --- | --- |
| Perfect Park into a bay | `PerfectPark` (car handling spec section 4) | High |
| Perfect drop-off at the curb | `PerfectDropOff` (new event, see open question Q1) | Medium |
| Drifting | `CarSlideStarted` / `CarSlideEnded`, or fill per second of sliding | Very slight (per second of drift, capped per drift) |

Every amount is a tuning value in a `PowerMeterConfig` ScriptableObject and an F2 key (Advanced, plain label). Proposed starting numbers (meter 0..100): Perfect Park +18, Perfect drop-off +10, drift +0.6 per second of sliding (cap +4 per slide). Level thresholds: level 1 at 25, level 2 at 55, level 3 at 100 (the longest to fill).

## 3. The three powers

| Level | Power | Effect (starting design) |
| --- | --- | --- |
| 1 | **Speed boost** (Mario-star style) | Valet on foot is faster for a few seconds with the rainbow outline, sparkle trail and the barge-through rule from `car_handling_spec.md` section 5 (+40% walk and run, 4 s). |
| 2 | **Freeze time** | The shift clock, patience and pickup timers, wash and charge progress, Director arrivals and ambient traffic and pedestrians stop for a short time (proposed 6 s). The valet and the car the player drives keep moving. |
| 3 | **Park Smash** (max level) | A release of energy that completes everything pending at once: arriving cars are parked correctly in bays, EVs are charged, dirty cars are washed and placed in a bay, waiting pickup patrons get their cars and leave. Gives the money for those patrons. A clean slate. |

Each power has its own art and sound (section 4).

## 4. Art and feedback (each power is distinct)

- **Speed boost:** rainbow palette-cycle on the valet outline, sparkle trail, boost bar under the valet, short original fanfare stinger (not Nintendo's star theme), music nudged up 6 BPM.
- **Freeze time:** cold blue desaturated tint over the world, frost vignette at the screen edge, frozen cars and people keep a faint ice outline, a clock icon with a countdown ring, a low "time stop" whoosh, music filtered and slowed; the valet and driven car stay in full colour.
- **Park Smash:** white screen flash, shockwave ring expanding from the valet across the lot, cars slide into their bays with sparkle puffs, a burst of coins and confetti, a heavy impact boom and a rising chime per car completed, a results pop "SMASH! +$N".
- **Meter:** vertical bar on the side of the HUD in the 480x270 pixel style, three notches (one per level) in three colours (gold, ice blue, hot pink), a pulse and ready icon at each level reached, the bar fills smoothly and pulses on every event that fills it.

## 5. Rules

- Spending a power resets the meter to empty (note 4).
- Powers stack (note 5): an active power continues. Example: use speed boost at level 1, the meter resets, refill quickly to level 2 while the boost still runs, use freeze time on top of it. Same for level 3. Two uses of the same power refresh its timer (no double strength).
- One activate key (proposed: Q or the gamepad left shoulder). It fires the highest level reached. Open question Q3.
- Money: Park Smash pays tips (see Q5); no other power gives money.
- Architecture (per CLAUDE.md): `PowerMeter` pure rule class (add, level thresholds, reset) with EditMode tests; `PowerMeterConfig` ScriptableObject; `PowerMeterModule` assembly `UncannyValet.Powers` referencing only Core; Core events `PowerMeterChanged(level, fill01)`, `PowerActivated(power)`, `PowerEnded(power)`, `PerfectDropOff(carId, ticketId)`; each power is a class behind `IPowerEffect` (open for extension); other modules react to `PowerActivated`/`PowerEnded` (clock freezes itself, Director pauses itself, audio/HUD/art listen). Freeze time needs a Core `ITimeScale`-style contract so modules scale their own time without static state. Park Smash needs the Lot and Patrons modules to complete their own pending work when they hear `PowerActivated(ParkSmash)` (no module reaches into another).

## 6. Open questions (answers needed before building)

| # | Question | Lead's suggestion |
| --- | --- | --- |
| Q1 | What counts as a "perfect drop-off"? | Delivered at the curb facing within a tight angle (for example 15 degrees, far inside the 80 degree rule), stopped inside the pickup zone, and delivered before the patron's pickup timer is half gone. |
| Q2 | Your list says "freeze time" for level 2 but your stacking example says "slow time". Which? | Freeze (as in your power list) for 6 s; make the time scale a tunable (0 = freeze, 0.3 = slow) so both can be tried. |
| Q3 | How is a power fired when the meter is above a level? | One key fires the highest level reached. Alternative: number keys 1 to 3 choose any unlocked level. |
| Q4 | The car handling spec already has an automatic Golden Valet speed boost after every Perfect Park. Keep both, or replace the automatic boost with the meter's level 1 power? | Replace: Perfect Park fills the meter instead of auto-boosting. One mechanism, and the boost becomes a choice. The Golden Valet streak step (+60%) becomes an option of the level 1 power or is dropped. |
| Q5 | How much money does Park Smash pay? | The normal tip for each cleared patron at base value plus the combo multiplier, no speed bonus, and the shift combo is not reset. This is a scoring change: needs your approval. |
| Q6 | Does the Patron Director react? | Park Smash empties the lot, so the Director's workload drops and it sends more cars, as intended. During freeze time it sends none. |
| Q7 | Can the meter carry over between shifts? | No: it starts each shift empty. |
