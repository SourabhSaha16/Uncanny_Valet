# Uncanny Valet: Design Document

The single source of truth for every agent. If code and this doc disagree, this doc wins until the lead
changes it. Only the lead edits this file, and every edit gets an entry in `docs/changelogs/lead.md`.

Combined from three design docs written on Nov 2–5, 2026: the Game Design Doc, the Game Object
Architecture, and the Story Cards.


> **LIVING DOC.** The lead updates Part A after every merge that changes the game (see the Design log at the
> end of Part A). Part B is the pre-pivot design, kept for history; where it conflicts with Part A, Part A wins,
> and `docs/redesign/chaos_redesign.md` is the long-term target for what is not built yet.

# Part A. Current design: chaos redesign (branch `pivot/chaos`)

Status: a playable slice of one lot (Grandma's Diner) is live on GitHub Pages from `pivot/chaos`. Items marked
(planned) come from the redesign doc and are not built; items marked (in progress) are with an agent now.

## A1. Overview

Uncanny Valet is a level-based chaos game in the Overcooked mould: short frantic shifts, three-star scores,
simple verbs layered into juggling, and failures that are funny rather than punishing. You are a valet at a
roadside diner: park patrons' cars, service them, and hand each car back when its owner returns. No mafia,
debt, strikes or boss orders (all removed).

| Field | Value |
| --- | --- |
| Perspective | Top-down pixel art, MetroCity tileset (CC0, JIK-A-4), 32 px tiles, PPU 32, Point filtering |
| Session shape | One 4-minute shift per lot (240 s, last 30 s tense), instant retry |
| Platform | Unity Web (WebGL), built locally with `tools/build-web.ps1`, deployed to GitHub Pages from `site/` |
| Branch | `pivot/chaos` deploys to the live site; `main` still holds the old night/debt game |
| Boss, rival | Mrs. Okafor (placeholder box on the results screen); ParkCorp rival is planned |

## A2. The shift

1. Title screen: "UNCANNY VALET", blinking PRESS ANY KEY. The clock does not start until a key press (also
   unlocks browser audio).
2. A car drives in to one of 3 arrival slots stacked under the curb and stops (CarArrived); only then does its info box
   appear. Any waiting car can be taken. When all 3 slots are full no new car arrives. Waiting cars have no timer
   and never leave on their own.
3. The valet walks to it (E to enter), drives it to a free numbered spot or a station (the EV charging bay 7, the wash slot) and parks it. The wash slot holds one car: a second car is physically blocked and an OCCUPIED
   hint shows.
4. The patron returns, the pickup is requested, the valet drives the car to the curb, tip is paid.
5. At 0:00 the results screen shows stars, score and Retry (click, Enter or Space). Next Lot is disabled.

Only the pickup timer exists (game minutes; 1 game minute is 1 real second); it drives the tip and the angry
patron penalty. Arrivals come from the Patron Director (A10).

## A3. Scoring

- Tip = base + speed bonus + patience bonus (up to $10) + charge bonus + clean bonus, then damage stages
  (25% off per 25% damage, $0 at 100%; no damage source yet), then times the combo.
- Combo 1x to 4x (cap in `ShiftConfig`): each happy delivery raises it; a $0 tip or an angry patron resets it.
- Angry patron: the pickup timer running out costs $15 from the score (there is no park timer). The score never
  goes below $0.
- Stars at $150, $300, $450 (placeholders, `ShiftConfig`).

## A4. What the player sees

Car presentation (all per ticket):
- A MetroCity top-down car sprite (14 regular and 2 EV cells), picked by a stable hash of the car id, half of
  them mirrored. The body is tinted to the ticket's colour (softened; EVs lightly). Dirty cars keep the
  dark-brown mud tint with a left-to-right wipe as they are washed.
- Licence plate under the car, the car's identity: 2 letters + 1 digit (for example ZR 9), sized to the car's
  width but never smaller than the smallest readable box (about 8 px wider than the car at the current zoom).
  Alphabet BCDFGHJKLMNPRSTVWXZ, unique among open tickets (a duplicate is issued if the 3610 combinations run out).
- An info box above the car, only when something applies: dirty icon with a dirt meter, plug icon with a
  charge meter (amber at full). It appears only after the car has arrived.
- Pickup emotion: from the pickup patience fraction, Happy (green, under 60%), Impatient (yellow, from 60%) or Angry
  (red, from 85% or expired). The plate border flashes in that colour at 1.5, 3 and 7 Hz (`HudEmotionConfig`), a
  small chat bubble with a face floats over the waiting patron in the same colour, and the CAR SLOTS row and bell
  use it too. The car body tint pulse still uses the old cyan/amber/orange-red colours (known mismatch).
- The plate shows under a car only once it is parked in a spot or station.
- No letters on cars, no roof band, no charge slider, no timer bar (all removed).
- Headlight highlights (two bright dots and a warm glow) at the nose of every car so the player can read the
  facing; tunables in `CarFlashConfig` (small at the current zoom, to be checked in play).

HUD (480x270 virtual canvas, integer-scaled, pixel font): shift clock top centre (amber at 30 s, blinking
orange-red at 10 s); a small coin score with three stars top left; a small combo badge top right; key board bottom left
listing open tickets with colour tag, plate, spot number and patience bar. F2 opens the tuning panel. The panel's "Pedestrians" slider (0 to 10, starts at 3) sets how many ambient people walk; people above the count finish their walk, then stay out of sight.
Pickup is shown on the key board row with a bell and cyan highlight, flashing with the urgency maths; the
separate pickup banner is gone.

## A5. The lot and people

> **Lot v2 (2026-11-10), layout and traffic merged.** Spec: `docs/redesign/lot_v2_layout.md` (binding).

- Lot layout (v2): diner on the left with a side door (ambient people) and a main door (patrons); the curb touches
  the diner right of the main door; a 3-slot arrival queue sits in a column under the curb (nose up); two rows of 4
  tall bays (2.6 x 4.4, pitch 3.2): top row regular spots 1 to 4, bottom row EV bay (id 7), regular 5, wash, regular 6;
  a wide aisle with a long crosswalk between the rows; a right strip (x 10.7 to 14.2) is the road and bus stop where
  ambient walkers appear. The frame is fixed (spots open and close but the walls never move). Below the lot a
  two-lane road: eastbound lane (next to the lot, patron cars) and westbound lane (ambient cars). `IRoadLayout` (Core)
  exposes the lanes, the queue turn, the curb merge and the entry and exit routes. The diner is placeholder
  a red-roof MetroCity building (roof tiles, window band, striped awnings over both doors, small DINER sign) and a BUS shelter.
- CAR SLOTS status word (2026-11-10): each row shows Parked, Washing, Washed, Charging, Charged, Pick Up or Cust.
  Angry (shortened to Angry when it does not fit) instead of the timer bar. A new status is one enum id, one rule
  class and one row in the label/colour table (`Scripts/UI/Status/`), in priority order Cust. Angry > Pick Up >
  Washing > Charging > Washed > Charged > Parked.
- Lot v4 (2026-11-10): the VALET PICKUP zone sits 1.75 units right of the diner wall (`SiteLayout.CurbGap`);
  patrons wait in that gap and always face east to the zone (set in `WalkerRouter.Apply` while Waiting); the queue
  column stays under the zone; the exit route has about 4 arrows (5.5 units apart). The key board panel is titled
  CAR SLOTS and lists a car only once it is parked in a spot or station (it stays until its ticket closes). The
  results screen no longer shows cars returned. Traffic defaults from the human's F2 tuning: 13 ambient cars a
  minute, ambient speed scale 1.25, entry speed 7.3 and exit speed 5 units/s (the panel value is the real speed).
- Lot v3 look (2026-11-10): the curb zone is labelled VALET PICKUP (two stacked columns); patrons at the curb face it
  (`IPedestrianPaths.CurbCenter`); the arrival queue is three dotted car-sized boxes (no arrows); crosswalks are white
  zebra stripes; the ground outside the lot is dark asphalt (no grass or trees); small arrows along the delivered
  car's exit route (curb, down the corridor, east onto the road) show where cars will drive off.
- Lot art: grass, sidewalks, road, the tan green-roof building as the diner, trees; the old placeholder
  rectangles are gone. Spots are painted bays; the curb is a cobbled zone with CURB text.
- Zebra crosswalks across the drive aisle and between bay rows with painted arrows; a crossing on the road.
- 8 ambient pedestrians loop on fixed routes (MetroCity character parts, 4-direction run cycles). Scenery only
  for now: no collision, no honk.
- Patron flow: the patron steps out of the arrived car, crosses to the diner and goes inside; when the stay
  ends they come out (PatronSummoned), cross to the curb and wait. The pickup and its patience timer start only
  when they stand at the curb (PatronReachedCurb; fallback after 40 game minutes). Ambient walkers come from
  the road sides, walk to the diner door, vanish inside and come out again. Walkers have no colliders, so routes
  may cut through hedges and bays until tuned.
- Delivery rule: the patron accepts the car only when it faces down the screen toward the road within a
  tolerance (80 degrees from straight down, `LotConfig`); otherwise an amber hint over the patron reads PLEASE PARK / IT PROPERLY.
  The curb zone has two big painted down arrows with chevrons, and the arrival slots have matching up arrows. On
  delivery the patron walks to the car and boards, then the car drives away (boarding timeout 6 game minutes).

## A6. Audio

Synthesised music and effects (no external assets), driven by events. Mix: sound effects 0.4 (was 0.5), music 0.2 (was 0.15) in `AudioConfig`. Combo stems, last-30-seconds tension and the title groove are planned.

## A7. Architecture today

Unchanged principles: four layers, typed event bus, `Services` as the only registry, one owner per piece of
state, pure rules with EditMode tests, assemblies referencing only Core. Changes in the pivot:
- Run state: `ShiftClock` (Night assembly) behind `IGameClock`; `ShiftScore` behind `IShiftScoreView`
  (Economy); `ShiftConfig` (Core/Config). `NightClock`, `DebtLedger`, `StrikeTracker`, `IDebtView`, night
  plans and the Orders module are deleted.
- New module: `Pedestrians` (config, crowd, route walker). UI is a pixel kit (`UI/Kit`), HUD painters
  (`UI/Hud`), results and title screens (`UI/Screens`), one `GameUi` component.
- Events added: `ShiftPlanChanged`, `ShiftStarted`, `ShiftTick`, `ShiftLast30Seconds`, `ShiftEnded`,
  `ScoreChanged`, `ComplaintFiled`, `ShiftResultReady`, `RetryShiftRequested`, `CarArrived`, `CarTagAssigned`,
  `CarPlateAssigned`, `TitleDismissed`. The night events became shift events.
- Art pipeline: `Editor/Import/ArtPostprocessor` sets Point filter, no compression, PPU 32 and slices the
  sheets (`docs/changelogs/pivot-art.md` has the layout). Sources are the `.rar` files in `art-src/MetroCity/`.
- Tests: EditMode suite (917 at the last full run, plus 11 PlayMode). New Core contracts: `IPedestrianPaths`, `IPatronLocator`, `ICurbFacing`; events `PatronSummoned`, `PatronReachedCurb`, `PatronBoarded`, `CurbFacingChanged`.

## A8. Build state and next steps

| Area | State |
| --- | --- |
| Shift clock, score, combo, stars, results, retry | done |
| Title screen, pixel HUD, key board, floating info boxes | done |
| Diner lot art, car sprites and tint, valet animation, crosswalks, ambient pedestrians | done |
| Plates (2+1), headlights, face-down delivery rule, patron walk flow, bell on key board, audio mix, more zoom | done, first browser check only (curb arrows and headlights hard to see; full flow not played) |
| Car controller upgrade: drift (vectored inertia, speed-dependent grip, Shift handbrake), turning radius from the lot, Perfect Park (+$2), Golden Valet boost, skid marks and tyre audio, heavy EVs | spec written and binding: `docs/redesign/car_handling_spec.md` (lead corrections in its section 0); steps 1 and 2 done (measured; `TurningRadius`; drift and steering models, Shift drift, `IDriveModifier` seam, F2 Car handling rows; all 7 PlayMode bay entries pass; not played); steps 3-6 not started; build order in its section 8 |
| Impact damage, shake, smoke, debris | planned |
| Pedestrian reactions: honk, tumble, courtesy tips | planned |
| Kev the rookie AI valet, task board, swap button | planned (P2) |
| Patron Director steps 1-3 (snapshot, pacing, archetype picker, dilemmas) and the soak test | done; step 4 (return scheduler) planned |
| Lot v2: new layout (6 regular, 1 EV, 1 wash, queue of 3), road below, new ambient spawn points, director retuned | done (layout and road drawn; not played) |
| Lot v3 polish: patrons face the pickup, VALET PICKUP label, dotted queue boxes, zebra crosswalks, real building, asphalt surround, exit-route arrows, F2 entry and exit speed | done, first browser check only |
| Lot v4: pickup zone gap, CAR SLOTS board (parked cars only), fewer arrows, traffic defaults, results without cars returned | done, first browser check only |
| CAR SLOTS status words, plate only when parked, pickup emotions (patron bubble, plate flash colours and speeds) | done, not played |
| Traffic module (lane following, car following, yielding, ambient cars), wired into patron arrival and departure; F2 Traffic rows (ambient rate and speed, entry speed, exit speed) | done; first browser check showed a patron car entering up the queue column, road mostly cropped by the camera |
| Faults, mechanic bay, fire spread, more venues, ParkCorp showdowns, world map | planned |

Open questions (from the redesign): keep or rename the title; single-valet hard mode; cliff car rule; local
co-op input. Known gaps: only 14 civilian car sprites exist in the pack (variety comes from tint and mirroring);
the title screen lacks the valet, Kev and the key-tag dot on the i; Mrs. Okafor is a placeholder box.

## A10. Patron Director

Spec: `docs/redesign/patron_director_spec.md` (binding). A pacing AI replaces the fixed 15-25 s arrival timer when
`DirEnabled` is on (F2 key; the old timer stays as the fallback so play-tests can compare).
- Built (rollout steps 1 and 2): `LotStateTracker` keeps a read-only `LotSnapshot` (car states and needs, wash and
  EV bay slot states Free/Working/Blocked/Squatted/Reserved, free spots, open tickets with patience);
  `WorkloadEstimator` gives workload W and pressure P from a task-cost table; `FormTracker` gives form F (every
  15 s, max 0.25 step, 0 for the first 30 s); `ArrivalPacer` turns W, P, the intensity curve T(t) (fraction of the
  shift, `DirectorProfile`) and F into the next arrival or a hold, with idle spawn, relief (hold 10 s then 2 plain
  cars, 20 s cooldown) and a 14 +/- 2 arrival budget. `PatronDirector` ticks every 0.5 s and publishes
  `PatronScheduled`; `PatronSpawner` just executes it. One seeded `ShiftRandom` per shift (`DirSeed` 0 = fresh).
- F3 overlay: station slot colours, car counts per state, free spots, phase, W/T/P/F, arrivals used/budget,
  next arrival in, relief flag, last five decisions with reasons. All numbers are F2 keys under "Director".
- Built (rollout step 3 and the soak test): `ArchetypePicker` draws Plain, Dirty, EV or Dirty EV by weight (50, 20,
  20, 10 in the Diner profile), only for archetypes whose station the lot has, scaled by the pacing gap, Plain only
  in relief, at most two Dirty or two EV in a row, and each archetype's share kept within +/-10 points of its base
  share of the 14-car budget. Seven `IDilemmaRule` classes (WashBlocked x3, WashBusy x1.5, BaysBlocked x3,
  BaySquatted x2.5, LastBay x1.5, LotTight Plain x1.5, Chain Dirty EV x2; `DilemmaKind` enum ids) boost the draw
  when their trigger holds and is solvable. `DilemmaGate`: one open at a time, 25 s cooldown, only with headroom
  (gap <= 0.15 or the curve rising), never in relief, solvable (a free regular spot or a pickup due within 10 s),
  20 s blackout after an angry patron. Events `DilemmaCreated`, `DilemmaResolved`, `ReturnForwardWanted`. Assist
  mode (config bool) clamps form F at 0 and halves the dilemma boosts toward 1. `PatronScheduled` now always
  carries the chosen archetype; the old EV/dirty chances run only when the director is off.
- F3 overlay also shows the open dilemma, cooldown, made/resolved counters with average resolve time, and each
  decision with its boost reasons.
- Soak test: a scripted bot plays 200 seeded shifts (about 0.7 s): 12 to 14 arrivals, about 1.3 dilemmas made and
  1.2 resolved per shift (the spec hopes for 2 to 3; the bot is not a real player), longest quiet stretch 15.5 s.
- Not built yet: `ReturnScheduler` (the Lot tight rule only raises `ReturnForwardWanted`, nothing acts on it),
  measuring real task times.
- Queue changes now in: cap 4 (PlanMaxWaitingCars), no park timer so snapshot patience and pressure P use pickup
  tickets only; waiting cars still add park work to W.

**Difficulty dials.** The F2 panel shows nine designer dials (0 to 10, default 5): Overall Intensity, Ramp Over the Shift, Late-Shift Rush, Car Mix Dirty %, Car Mix EV %, Dilemma Frequency, Mercy, Skill Reward and Randomness Seed. They derive every underlying Director and plan value (`Director/DifficultyDials`, anchors in `DifficultyDialsConfig`; tables in `docs/redesign/difficulty_dials.md`). The default position reproduces the old defaults exactly. All other rows sit behind the panel's Advanced toggle (off by default) with plain-language names. A dial move re-derives all driven rows, overwriting hand edits.

## A9. Design log

| Date | Change |
| --- | --- |
| 2026-11-09 | Adopted the chaos redesign on `pivot/chaos`; debt, strikes, boss orders removed; 4-minute shifts with score, combo, stars |
| 2026-11-09 | MetroCity art imported at PPU 32; diner lot, car sprites, valet animation |
| 2026-11-09 | Pixel UI kit, HUD, title and results screens (style guide `docs/redesign/ui_style_guide.md`) |
| 2026-11-09 | Letters on cars replaced by ticket colour tags; pickup flash; info box after arrival; zoom in; crosswalks and pedestrians |
| 2026-11-09 | Licence plates; legacy car meters and roof band removed; body tint; police sprites added |
| 2026-11-09 | Patron Director spec adopted; steps 1 and 2 built (snapshot, F3 overlay, workload, form, pacer, relief, budget) |
| 2026-11-09 | Patron Director step 3: archetype picker, seven dilemma rules, dilemma gate, soak test |
| 2026-11-09 | Stations moved into the spot grid (EV bays 10 and 11 in column 1, wash in column 2, 9 regular spots), wash icon, much smaller score and combo HUD |
| 2026-11-09 | Queue of 4 with any car takeable, park timer removed (pickup timer only), wash slot single occupancy with OCCUPIED hint, plug icons under EV bays, delivery accepts any down-facing car (80 degrees), big curb and arrival arrows |
| 2026-11-09 | Play-test notes built: plate 2+1, more zoom, patron and ambient people flows, bell on key board instead of banner, audio mix, face-down delivery with hint, curb arrows, headlights |
| 2026-11-10 | Pedestrian count added to the F2 panel (0 to 10, starts at 3); the human likes the director pacing |
| 2026-11-10 | Lot v2 from the human's sketch (`docs/redesign/lot_v2_layout.md`): 6 regular + 1 EV (id 7) + 1 wash, queue of 3 under the curb, road below, side and main doors, right strip as ambient spawn, fixed frame; director retuned (LotTight 1, LastBay for one bay); Traffic module built (arrival and departure wiring in progress); 672 tests |
| 2026-11-10 | Lot v3 polish (8 items from play-test) merged: see A5; 707 tests |
| 2026-11-10 | Lot v4 play-test fixes merged (pickup zone gap, CAR SLOTS only parked cars, fewer exit arrows, traffic defaults, no cars returned stat, patron facing fix); 712 tests |
| 2026-11-10 | UI status and emotions merged: CAR SLOTS status words (extensible), plate only when parked, patron mood bubble, green/yellow/red plate flash at 1.5/3/7 Hz; 737 tests |
| 2026-11-10 | Queue and Director fixes: an arriving patron car re-checks its queue slot every 0.2 s and always takes the first free slot (it moves up when a car ahead leaves; never two cars in one slot; slot reservations are per car and released on arrival, ticket close and shift change; Core `IQueueSlotSource`). The Director pacer re-sends an arrival that was never acknowledged after 8 s, so a failed spawn can no longer stall arrivals (it showed as 'next in 0.0 s' with RELIEF stuck). Late arrivals get a shorter stay (`StayFit`) so every patron can ask for the car before the shift ends. Cause of the failed spawn not confirmed; slot scan now ignores trigger colliders; 772 tests |
| 2026-11-10 | Play-test round: CAR SLOTS Washed and Charged are short-lived flags set on `CarCleaned` / `CarFullyCharged` and cleared on `CarParked` (a washed car now reads Parked once parked; a full EV never charged here reads Parked); the car-arrived sound plays on `CarArrived` (car stopped in its queue slot), not at spawn; camera fit tightness 0.87 to 0.78 (play area never cropped); the big world box over cars is gone: the wash slot and charging bays light up while serving (aqua for wash, lime for charge, brighter with progress, flash then 1.5 s fade when done) and a small droplet or plug icon sits beside the plate; pedestrians are solid (kinematic body, circle collider on physics layer Pedestrian, wait 1.5-3 s then walk through for 1.5 s, never freezes a route, standing patrons not solid), a car or the valet bumping one gives a tiny rubber bounce (0.2 units, 0.25 s) and a quiet boing (`PedestrianBumped`, rate-limited); 811 tests |
| 2026-11-10 | Play-test fixes: a patron on foot ignores its own car (no collision, bounce, bump sound or waiting; other cars and the valet unchanged); the droplet (needs wash) and plug (needs charge) icons now appear as soon as the car has arrived in its queue slot (patron steps out) and stay until the service finishes or the ticket closes, beside the plate when it shows, otherwise just under the car; 817 tests |
| 2026-11-10 | Difficulty dials: nine designer dials in F2 drive every Director and plan value; the old rows are behind an Advanced toggle with plain names; F3 overlay in plain words; 7 new tuning keys (curve scale, ramp, late boost, car-mix weights); default position unchanged; 853 tests |
| 2026-11-10 | Car handling and Perfect Park spec adopted as `docs/redesign/car_handling_spec.md`, with lead corrections: EV bay id is 7 not 13/14; aisle is 4.6 (R_min about 3.3); no `IDriveModifier`, dash, `DecalPool` or PlayMode tests exist yet and are built as part of the rollout; barge-through uses the pedestrian collision layer; $2 Perfect Park is the only scoring change |
| 2026-11-10 | Car handling step 1 merged: measured aisle 4.6, bays 2.6 x 4.4 pitch 3.2, EV bay id 7, real car 1 x 2 (collider matches length, about 0.1 per side narrower than the sprite); R_min 3.3 and MaxTurnRate 1.212 rad/s (69.5 deg/s) from `TurningRadius`; pull-out rule passes (needs 4.1 to 4.2, aisle 4.6) so no widening; PlayMode test assembly added, the 7 bay-entry cases are ignored until step 2 (today's driver turns at a radius of about 1.2 and misses the bay); 863 EditMode tests |
| 2026-11-10 | Car handling step 2 and car bounce merged: `DriftModel`/`SteeringModel` run in `CarDriver`; Shift (or gamepad right shoulder) is the drift handbrake (grip x0.25); ForwardGrip hands back the speed the sideways damping removes (not literally 'sideways speed removed', which made cars gain speed in corners); `CarHandlingConfig` and F2 'Car handling' rows (Advanced); aisle width synced from the lot with a loud error if it differs. Cars bounce off each other (rule `CarBounceRule`, restitution 0.4, parked cars count 8x heavier, trip-driven cars excluded) with a thud (`CarsCollided`, `SfxId.CarBump`). A facing arrow pair now sits in the VALET PICKUP zone (white idle, yellow wrong facing, green right; direction and 80 degree tolerance from `CurbFacingRule`); 917 EditMode and 11 PlayMode tests |

---

# Part B. Pre-pivot design (superseded, kept for history)

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
