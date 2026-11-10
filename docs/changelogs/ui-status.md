# ui-status changelog

## ui-status: CAR SLOTS rows show a status word
Date: 2026-11-10
Files: Scripts/UI/Status/{CarStatus,ICarStatusRule,CarStatusResolver,CarStatusTable,CustAngryStatusRule,PickUpStatusRule,WashingStatusRule,ChargingStatusRule,WashedStatusRule,ChargedStatusRule}.cs, Scripts/UI/{TicketRow,TicketBoard}.cs, Scripts/UI/Hud/KeyBoardPainter.cs, Tests/EditMode/UI/CarStatusTests.cs
Why: human request (design.md A4 / style guide rev 6): each CAR SLOTS row shows Parked, Washing, Washed, Charging, Charged, Pick Up or Cust. Angry instead of the dash and the patience bar; the spot number and the pickup bell stay. The resolver takes the first applying rule of an ordered list (Cust. Angry, Pick Up, Washing, Charging, Washed, Charged, else Parked); a new status is one enum id, one rule class, one table entry. TicketRow now remembers the station the car is in (CarEnteredStation / CarLeftStation); no new gameplay state. "Cust. Angry" is 65 px wide and the row has 56 px left, so the table has a short label (Angry) used only when the full one does not fit.
Tests: CarStatusTests (priorities, station transitions, table completeness, abbreviation, an extra rule joining the list, board tracks stations).
