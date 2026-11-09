# Uncanny Valet: UI and art style guide (from the Scene Art Prompt Deck, Nov 9, 2026)

Source: the human's "Scene Art Prompt Deck" (14 target scenes). This file keeps the parts that drive the in-game UI.
The scene images themselves are not produced yet.

## Style bible

| Element | Rule |
| --- | --- |
| Camera | Straight top-down, early-GTA style. Buildings show a little of their front face. No perspective tilt |
| Framing | The whole play area fits on screen with a 1-2 tile border of street or scenery. No scrolling |
| Scale | MetroCity native tile (32 px) is the grid. A car is about 1 tile wide and 2 long; a person about half a tile |
| Outlines | Dark near-black outlines on cars, people and props. Terrain has no outline |
| Palette | Warm saturated daytime base. Each venue adds one accent colour (diner: mint and cherry red). Hazards always use the alarm colours |
| Alarm colours | Fire and danger: orange-red. Warnings and timers running out: amber. Good and done: green. Info and pickups: cyan |
| Motion | 1-pixel speed lines, tyre marks, dust puffs, small debris. No motion blur |
| Effects | Smoke, fire, sparks, water, confetti as chunky 2-4 colour pixel shapes |
| World UI | Tickets, patience bars and icons float above cars and patrons, drawn at the same pixel scale |
| Screen UI | Thick-bordered panels, chunky pixel font, big icons. At screen edges, never covering the curb or the exit |
| Text | Pixel font only. Short: numbers, names, a few words |
| Render | Native 480x270 canvas, integer upscaled with nearest-neighbour. No smoothing, no anti-aliased edges |

## Cast (keep identical everywhere)

- The valet (player): red valet jacket, black trousers, small cap.
- Kev (rookie AI valet): same uniform in teal, cap on backwards, slightly too big.
- Mrs. Okafor (boss): bright patterned dress, glasses, clipboard (results screen, title, diner).
- ParkCorp robots: small grey boxy robots with one cyan eye light.
- Every patron car carries a small coloured key tag icon matching its spot.

## Scene 10: In-game HUD (defines the HUD layout)

The busy diner lot with the full HUD in chunky pixel UI:
- Top centre: shift clock counting down from 4:00 (example 1:12).
- Top left: tip score with a coin icon and three star markers on a progress bar (two lit).
- Top right: combo badge showing x3 with a wobble effect.
- Bottom left: a small key board panel listing open tickets as coloured key tags with car icons and patience bars.
- Floating world UI over cars and patrons: tickets with request icons (wash, tyre, plug), patience bars, a pickup bell.
- Nothing covers the curb or the exit lane.

## Scene 11: Results screen

Pixel results screen over a dimmed view of the lot. Big panel with a stamp-style reveal of three stars (the third landing with a burst). Total tips with a coin pile. Funny stats with small icons (hubcaps lost, cars set on fire, pedestrians honked at, perfect parks, metres driven in reverse). Mrs. Okafor in the corner giving a thumbs up with a speech bubble. Two big buttons: Retry and Next Lot.

## Scene 13: Title screen

Title UNCANNY VALET in big chunky pixel letters with a key-tag shape dotting the i, over a top-down view of the diner lot in mid-chaos. The red valet holds up a key ring; Kev stands behind him holding the wrong keys. Blinking PRESS ANY KEY prompt at the bottom.

## Tone and rules

Bright, silly cartoon. Chaos is funny, never violent: no injuries, blood or weapons. Original work only: no copied characters, logos or UI from other games; invent all brand names; no real car brands.

## Revision 1 (2026-11-09, human feedback after the first UI build)

These override anything above that conflicts.

- **No letters on cars.** Cars carry no letter label. Each open ticket has a colour code (key tag colour); the car shows it as a small tag or stripe, and the key board row, floating info box and tag all use the same colour.
- **Pickup urgency is a flash.** A car whose patron wants it back flashes (colour tag or body outline). The flash frequency rises as the pickup patience runs down: slow while calm, fast near zero. Alarm colours still apply (amber while running low, orange-red when expired).
- **Info box above the car** shows only what applies: a dirty icon with a dirt meter, a plug icon with a charge meter. No letter. Dirt keeps its dark-brown tint on the car body as well. A car with no needs shows no box.
- **Timing.** A car's info box appears only after the car has finished driving in and stopped; never before it is visible.
- **Camera.** Tighter framing than the first build: the play area still fits with a small street border, but zoomed in further so cars and people read bigger.
- **Pedestrians and crosswalks.** Real car-park feel (see the human's aerial reference photos): zebra crosswalks across the drive aisles and between bay rows, painted arrows on the aisles, pedestrians walking on crosswalks and sidewalks in civilian outfits. Pedestrians are scenery for now (no collision or honk yet).

## Revision 2 (2026-11-09, human feedback)

Overrides earlier text where it conflicts.

- **Legacy car meters removed.** No charge slider on the car and no timer fill bar under the car. The info box above the car is the only place for dirt and charge meters.
- **Licence plate.** Each patron car shows a small licence-plate box under it: 3 letters then 3 numbers (for example ABC 123), pixel font on a light plate with a dark border. The plate is the car's identity: the key board rows show the same plate text. Plates are unique among open tickets.
- **No colour band on the roof.** Instead the car sprite itself is tinted to a per-ticket colour (from the tag palette, softened so the outline and windows stay readable). The key board swatch uses the same colour. The pickup flash uses an outline/glow, not a roof stripe. The dirt tint stays.
- **Car variety.** Use many different car sprites from the MetroCity car pack (not just a handful); EVs keep their own sprites.

## Revision 3 (2026-11-09, human play-test notes)

Overrides earlier text where it conflicts.

- **Licence plate.** 2 letters + 1 digit (for example KT 4), same restricted alphabet (BCDFGHJKLMNPRSTVWXZ), 19 x 19 x 10 = 3610 combinations, unique among open tickets (if all are taken, a duplicate is issued rather than failing). The plate box under a car is as wide as the car (plate width = car width). The pixel font cannot shrink, so the smallest readable box (text plus border, narrow word gap) is used when the car is narrower; at the default car width the plate is slightly wider than the car.
- **Pickup is shown by a bell on the key board row, no banner.** The pickup banner under the clock is gone. A requested pickup puts a bell and a cyan highlight on that ticket's key board row, flashing with the same urgency flash as the plate (slow while calm, fast near zero; amber, then orange-red).
- **Zoom.** About 12-15% closer than before, with the whole lot (walls included) still in frame.
- **People flows.** Ambient walkers come from the road side (off-screen), cross at the road crosswalk, walk north through the lot along the open strip beside the curb (over the aisle crossing) and along the north sidewalk to the diner door, vanish inside, and after a while come out and walk away the same way back. Nobody has a collider.
- **Patron flow.** When a patron car has finished driving in (CarArrived) the patron gets out, walks via the crosswalks to the diner door and goes inside. When the stay is over the patron comes out, crosses via the crosswalk to the curb and stands there; only then is the pickup requested and its patience started (the stay timer stays in game minutes; the walk is natural extra delay). On delivery the patron walks into the car and disappears, then the car drives away. If the ticket ends without a delivery the patron vanishes quietly.
- **Face-down delivery rule.** The patron only accepts the car when it is parked in the curb zone with its nose pointing down the screen (towards the road) within a tolerance (default 35 degrees, LotConfig). A wrongly facing car in the zone is not accepted and a small amber hint appears over the patron: PLEASE PARK IT PROPERLY (two lines, pixel font). It hides when the car is turned or leaves.
- **Curb markings.** The curb zone carries two painted down arrows, each with a chevron under its tip, in the aisle-arrow style, so the player knows which way to face.
- **Headlights.** Every car shows two small bright yellow-white dots near its front with a soft warm glow behind them, drawn above the body and rotating with the car. They are symmetric, so a mirrored body sprite never moves them to the back.
- **Audio.** Sound effects are about 2 dB quieter (0.5 to 0.4) and the music/ambient layer about 2.5 dB louder (0.15 to 0.2).
