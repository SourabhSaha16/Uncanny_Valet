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
