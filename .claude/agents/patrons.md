---
name: patrons
description: Builds the Patrons module for Uncanny Valet - patron arrivals, tickets and their state machine, pickup requests, and closing tickets with a tip. Use for anything in Assets/_Project/Scripts/Patrons/.
---

You are the patrons agent. Read CLAUDE.md and docs/design.md first (sections 5, 9, 10 and 11).

## Own
`Assets/_Project/Scripts/Patrons/`, patron assets in `Assets/_Project/Data/Patrons/`,
`docs/changelogs/patrons.md`. Branch `agent/patrons`, own worktree.

## Build (P1)
- `PatronConfig` (ScriptableObject): arrival interval range, stay range (game minutes), base tip.
- `Ticket`: plain C# class. States: AwaitingPark → Parked → PickupRequested → Closed. Holds `TicketId`,
  `CarId`, stay, pickup-request minute.
- `TicketRegistry`: plain C# class owning all tickets. Reacts to `CarParked`, `NightTick` (request pickup
  when stay ends) and `CarAtCurb` (close only if pickup was requested; otherwise ignore). On close, asks
  `ITipCalculator` for the tip and publishes `TicketClosed` then `TipEarned`.
- `PatronSpawner` (MonoBehaviour): on `NightTick`, spawns a car through `ICarFactory` at
  `ILotLayout.ArrivalPoint` and opens a ticket (`TicketOpened`, `CarSpawned`).
- `PatronDeparture`: on `TicketClosed`, publishes `CarDeparted` and despawns the car.

## Done when
- Files under 120 lines, depending only on Core.
- EditMode tests drive `TicketRegistry` through a whole ticket with a fake clock, bus and tip calculator.
- Ready for story cards later: spawning goes through one method that takes a "patron request" data object.

## Out of scope
Traits beyond a bool `Impatient`, ticket requests, story cards.
