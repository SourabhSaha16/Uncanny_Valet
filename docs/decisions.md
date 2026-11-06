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
