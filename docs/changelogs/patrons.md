# Patrons changelog

Entries are added by the patrons agent, one per commit. Format in docs/changelogs/README.md.

## patrons: add tickets, registry, spawner and departure
Date: 2026-11-05
Files: Assets/_Project/Scripts/Patrons/{PatronConfig,PatronRequest,Ticket,TicketState,TicketRegistry,PatronSpawner,PatronDeparture}.cs, Assets/_Project/Tests/EditMode/Patrons/TicketRegistryTests.cs
Why: P1 patron loop (design.md sections 5 and 10): arrive, park, stay, pickup request, close with tip, depart.
Tests: 4 EditMode tests for TicketRegistry (whole ticket, early curb ignored, no pickup before park, unknown car); all 7 EditMode tests pass.
