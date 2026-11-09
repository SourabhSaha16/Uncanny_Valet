# Decisions log

Every question an agent asked, and the answer. Newest at the bottom.

| Date | Asked by | Question | Answer | Source |
| --- | --- | --- | --- | --- |
| 2026-11-05 | lead | Input system? | Input System package, behind `IInputSource` | Web build rules |
| 2026-11-05 | lead | Build compression? | Disabled, so GitHub Pages can serve the build | GitHub Pages headers |
| 2026-11-05 | lead | Prototype UI? | IMGUI debug overlay only | design.md section 10 |
| 2026-11-05 | lead | Which Unity version? | Unity 6000.2.5f1 (installed on the dev PC), not 6.3 LTS | Human, chat |
| 2026-11-05 | lead | How is the Web build made and published? | Built locally with tools/build-web.ps1 into the committed site/ folder; CI checks changelogs and publishes site/ to Pages. No Unity in CI, no secrets (the owner signs in to Unity with Google, so there is no password for CI activation) | Human, chat |
| 2026-11-05 | human | Play-test notes on the first prototype | Handling and camera are fine. Car stacking becomes overwhelming: add a rule so the player is not overwhelmed. Number the lots so each car can be tracked to its spot | Human, chat |
| 2026-11-05 | lead | What rule limits arrivals? | An `ArrivalGate` in Patrons: a new car arrives only when fewer than `MaxWaitingCars` (default 2) tickets are still awaiting parking AND the arrival point is clear of cars. If blocked, retry next game minute (no new random gap). Tunable in PatronConfig; it is the "patrons per night" difficulty dial in design.md section 8 | lead, from play-test note 3 |
| 2026-11-05 | lead | How are spots numbered? | Spot ids (already in Core, 1-based as the Lot builds them) are drawn as large labels on each spot; the overlay shows "Parked in spot N" per ticket; each car shows its car number on its body | lead, from play-test note 4 |
| 2026-11-06 | human | Play-test round 2 notes | Cars get letters A, B, C and spots keep numbers. Cars must be heavier (valet could push them). CURB text vertical. Cars pile up to 3, each with a park timer (dark fill rising on the car); a pickup timer gives $0 tip if missed but the car must still be delivered | Human, chat |
| 2026-11-06 | lead | What does the cap of 3 count? | Cars awaiting parking only; parked cars do not count | Human, chat |
| 2026-11-06 | lead | What happens when the park timer runs out? | Patron takes the car: it vanishes, tip $0, no strike, no penalty (pillar 1) | Human, chat |
| 2026-11-06 | lead | Timer lengths and look | Park 45 game minutes, pickup 60, both in PatronConfig. Dark fill rises bottom to top over the car; pickup fill is red-tinted | Human, chat |
| 2026-11-06 | lead | Does the park timer run while the valet drives the car? | No: it pauses while the valet is inside, so no car has to be despawned under a driver. It resumes if he leaves the car outside a spot | lead, avoids cross-module ejection |
| 2026-11-06 | lead | How do waiting cars queue? | New Core member `ILotLayout.ArrivalSlots` (slot 0 = arrival point); patrons uses the first free slot among `MaxWaitingCars` | lead |
| 2026-11-06 | lead | Engineering standards | Added "Engineering standards" to CLAUDE.md so every new agent follows them; qa reviews against it | Human, chat |
| 2026-11-06 | human | Play-test round 3 notes | Prefers the first build's look: queued cars next to each other and all visible at once. Overlay shows percent of time left (keep). Add low-volume funky background groove and basic sound effects. Shorter nights. End-of-night summary card (tips, cars parked, cars not parked) with a Next button | Human, chat |
| 2026-11-06 | lead | Queue layout? | Side by side in a row, close together, all inside the camera at the old size 8.5 | Human, chat |
| 2026-11-06 | lead | Night length? | 3 real minutes (180 game minutes, 22:00 to 01:00), in RunConfig | Human, chat |
| 2026-11-06 | lead | Audio source? | Generated in code (synthesised loop and effects) behind an AudioConfig; new module Audio references only Core | Human, chat |
| 2026-11-06 | lead | After the final night? | The card shows paid off or debt left; the button starts a new run | Human, chat |
| 2026-11-06 | lead | What happens to open tickets at night end? | All close with $0 and their cars leave; the summary counts never-parked cars as not parked. Core gains NightSummary, NightSummaryReady, NextNightRequested | lead |
| 2026-11-06 | human | Play-test round 4 notes | Sound, parking, tips and car mass are good. Add side mirrors at the front. Faster clock with proportionally shorter timers. Seven nights adding one challenge at a time (simple, messy, EV charging, dirty cars, then all combined); unlocks listed on the previous night's end screen with the tip calculation. Sabotage stays off | Human, chat |
| 2026-11-06 | lead | Night length? | 2 real minutes: clock at 1.5 game minutes per second; park and pickup timers stay in game minutes so they shrink by the same ratio | Human, chat |
| 2026-11-06 | lead | How does EV charging work? | A charging station, like the car wash, not EV spots. An EV is an ordinary blue car with a small blue charge bar; delivered fully charged it earns a bonus; no penalty if not. Timed charge (about 20 game minutes) while stopped in a bay (human: no preference on timing, lead chose timed) | Human, chat |
| 2026-11-06 | lead | When does the wash slot free? | When the washed car drives out | Human, chat |
| 2026-11-06 | lead | What if a dirty car is handed back unwashed? | $0 tip, from design.md section 5 (required, a miss can zero the tip). The human did not answer this question directly: flagged in chat so they can overrule it | lead |
| 2026-11-06 | lead | Station design? | Lot owns geometry and occupancy (events CarEnteredStation / CarLeftStation); Patrons owns the rules and timers (charge, clean, tip flags); Cars draws blue EV body, charge bar, mud and mirrors; Night owns the NightPlan. 2 charging bays, 1 wash slot; stations appear only on the nights that unlock them | lead |
| 2026-11-06 | lead | Apply the simulation's pacing and debt? | Yes, Schedule A (gap scales, open spots 4,4,5,5,5,5,5) and starting debt $250 instead of $1500; tip values unchanged. Debt figure is the lead's pick from the simulation, open to change | Human, chat |
| 2026-11-06 | lead | Round 5 choices | Charging spots accept any car (only EVs charge); EVs stay on from night 3 with unlocks layered; the panel covers all pacing numbers. Lead picked the faster pacing table (design.md section 10) and EV surge 0.6 to 0.75 | Human, chat (first three); lead (rest) |

| 2026-11-06 | Round 6: starting debt $500; pacing reverted to round-5 table | Human asked for $500 "only extremely good players can reach", then reverted the pacing increase. Debt kept at $500 with round-5 pacing, so $500 may be very hard; tune live with F2. |
| 2026-11-09 | lead | Adopt the chaos redesign? | Yes, on branch pivot/chaos. Placeholder diner building: any MetroCity building. Old debt/orders code is deleted on the branch (main keeps it). The pivot build replaces the live GitHub Pages site (workflow deploys pivot/chaos) | Human, chat |
| 2026-11-09 | lead | Keep the design doc alive? | Yes: design.md Part A is the living current design; the lead updates it and its Design log after every merge that changes the game. Part B is the old design, kept for history | Human, chat |
