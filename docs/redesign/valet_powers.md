# Valet Powers (binding since 2026-11-11; questions answered in section 6)

Source: the human's note of 2026-11-10. Status: planned, not built. The human answered all questions on 2026-11-11 (section 6); this is now the binding spec.

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

## 6. Decisions (answered by the human, 2026-11-11)

| # | Question | Decision |
| --- | --- | --- |
| Q1 | What counts as a perfect drop-off? | Delivered at the curb facing within about 15 degrees (far inside the 80 degree rule), stopped inside the pickup zone, and delivered before the patron's pickup timer is half gone. All three numbers are tuning values. |
| Q2 | Freeze or slow? | Freeze for 6 s. The time scale is a tunable (0 = freeze, 0.3 = slow) so both can be tried. |
| Q3 | How is a power fired? | One key (Q, gamepad left shoulder) fires the highest level reached. |
| Q4 | Golden Valet overlap | Replaced: the human closed the Golden Valet dash (issue 17) because the level 1 speed boost power replaces it. Perfect Park fills the meter instead of auto-boosting. |
| Q5 | Park Smash pay | Every cleared patron pays a full tip including the speed bonus, as if delivered perfectly fast, times the combo multiplier. The shift combo is not reset. (Scoring change, approved.) |
| Q6 | Does the Director react? | Yes. Park Smash empties the lot so workload drops and the Director sends more cars; during freeze time it sends none. |
| Q7 | Carry-over between shifts | No: the meter starts every shift empty. |
