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
