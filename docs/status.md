# Status board

The lead keeps this current. Agents read it when they are (re)spawned.

| Phase | Item | Owner | State |
| --- | --- | --- | --- |
| P0 | Core compiles, EventBusTests pass in batch mode | lead | done (3 of 3) |
| P1 | Cars module | cars | merged |
| P1 | Valet module | valet | merged; small CarExited fix pending |
| P1 | Lot module | lot | merged |
| P1 | Patrons module | patrons | merged |
| P1 | Economy + Night modules | economy-night | merged |
| P2 | Bootstrap scene, overlay, local Web build | integration | todo |
| P2 | Cross-module tests | qa | todo |
| P3 | Reviews, merge, first Pages deploy | qa + lead | todo |
| Human | Pages source set to GitHub Actions | you | done |
| Done | Unity project, git repo, first Web build live on GitHub Pages (empty scene) | lead | done |
| Human | Play the P1 build in a browser | you | done (notes in decisions.md) |

## Notes

- 2026-11-05: all five P1 modules merged into main; full EditMode suite 52 of 52 (3 Core, 8 cars, 5 valet, 8 lot, 4 patrons, 24 economy-night).
- Integration must create the missing assets: `LotConfig` (Data/Lot) and `PatronConfig` (Data/Patrons) via an Editor script; `Sedan`, `TipConfig` and `RunConfig` already exist.
- Integration must wire: `NightClock.StartNight`, `new DebtLedger(bus, runConfig.StartingDebt)` as `IDebtView`, `CarFactory.Bus`, register `ILotLayout`, `IEventBus`, `IInputSource` before zones/valet start.
- Not yet verified in play: trigger behaviour in the lot (CarParked / CarLeftSpot / CarAtCurb), car driving feel, enter/exit flow.

- 2026-11-05 play-test round 1: handling and camera fine. Tasks: (patrons) ArrivalGate to stop car stacking; (lot) draw spot numbers on spots; (cars) show car number on each car; (integration) overlay shows which spot each parked car is in.

- 2026-11-06 play-test round 2 (Core contracts added by lead: `ILotLayout.ArrivalSlots`, `TimerKind`, `TicketTimerChanged/Expired/Stopped`, `CarLetterAssigned`, `TipInput.PickupExpired`). Tasks: (lot) ArrivalSlots + vertical CURB text; (patrons) queue of 3, letters, park and pickup timers; (cars) heavy mass, letter label, timer fill; (economy-night) tip $0 when PickupExpired; (qa) review against the new Engineering standards. Main does not compile until lot implements ArrivalSlots.

- 2026-11-06 round 3 (Core adds NightSummary, NightSummaryReady, NextNightRequested). Phase 1 (parallel): lot (side-by-side queue slots), economy-night (NightTally publishes the summary, night 180 min), audio (new module, synthesised music and sfx), valet (survive the car despawning under the driver), patrons (close all tickets at NightEnded). Phase 2: integration (summary card, next night and new run, camera back to 8.5, wire Audio).
