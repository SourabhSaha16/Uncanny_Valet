# Patrons changelog

Entries are added by the patrons agent, one per commit. Format in docs/changelogs/README.md.

## patrons: close ticket when car already at curb at pickup
Date: 2026-11-05
Files: Assets/_Project/Scripts/Patrons/{Ticket,TicketRegistry}.cs, Assets/_Project/Tests/EditMode/Patrons/TicketRegistryTests.cs
Why: QA found a ticket stuck if the car waited at the curb before pickup (CarAtCurb fires once). Registry now remembers AtCurb per ticket (cleared on CarEntered), closes on pickup request, iterates a copy of tickets and removes closed ones.
Tests: 3 EditMode tests added (early curb, CarEntered clears flag, open inside handler); not run, Unity not run per instruction.

## patrons: add tickets, registry, spawner and departure
Date: 2026-11-05
Files: Assets/_Project/Scripts/Patrons/{PatronConfig,PatronRequest,Ticket,TicketState,TicketRegistry,PatronSpawner,PatronDeparture}.cs, Assets/_Project/Tests/EditMode/Patrons/TicketRegistryTests.cs
Why: P1 patron loop (design.md sections 5 and 10): arrive, park, stay, pickup request, close with tip, depart.
Tests: 4 EditMode tests for TicketRegistry (whole ticket, early curb ignored, no pickup before park, unknown car); all 7 EditMode tests pass.
