# ui-status changelog

## ui-status: CAR SLOTS rows show a status word
Date: 2026-11-10
Files: Scripts/UI/Status/{CarStatus,ICarStatusRule,CarStatusResolver,CarStatusTable,CustAngryStatusRule,PickUpStatusRule,WashingStatusRule,ChargingStatusRule,WashedStatusRule,ChargedStatusRule}.cs, Scripts/UI/{TicketRow,TicketBoard}.cs, Scripts/UI/Hud/KeyBoardPainter.cs, Tests/EditMode/UI/CarStatusTests.cs
Why: human request (design.md A4 / style guide rev 6): each CAR SLOTS row shows Parked, Washing, Washed, Charging, Charged, Pick Up or Cust. Angry instead of the dash and the patience bar; the spot number and the pickup bell stay. The resolver takes the first applying rule of an ordered list (Cust. Angry, Pick Up, Washing, Charging, Washed, Charged, else Parked); a new status is one enum id, one rule class, one table entry. TicketRow now remembers the station the car is in (CarEnteredStation / CarLeftStation); no new gameplay state. "Cust. Angry" is 65 px wide and the row has 56 px left, so the table has a short label (Angry) used only when the full one does not fit.
Tests: CarStatusTests (priorities, station transitions, table completeness, abbreviation, an extra rule joining the list, board tracks stations).

## ui-status: licence plate shows only once the car is parked
Date: 2026-11-10
Files: Scripts/UI/TicketRow.cs, Scripts/UI/Hud/WorldTicketPainter.cs, Tests/EditMode/UI/TicketBoardTests.cs
Why: human saw a plate at the screen edge before the car was in view (design.md A4). The plate is drawn by the UI world painter (not a Cars component) and was gated only on HasPlate, with its position clamped to the canvas, so a plate for an off-screen car pinned itself to the edge. New TicketRow.ShowsPlate = HasPlate && Placed, the same Placed flag the CAR SLOTS list uses (CarParked or CarEnteredStation); the plate-assigned event still only fills the row text.
Tests: TicketBoardTests.PlateShowsOnlyOnceTheCarIsParked (regression: hidden while queued and after CarArrived, shown after CarParked), PlateShowsOnceTheCarEntersAStation.

## ui-status: pickup emotions on a patron bubble, the plate and the row
Date: 2026-11-10
Files: Scripts/UI/Mood/{Emotion,EmotionRule,EmotionStyle,HudEmotionConfig,MoodBubbleLayout}.cs, Scripts/UI/Hud/{PatronMoodPainter,PickupMark,PlateLayout,Alarm,WorldTicketPainter,KeyBoardPainter}.cs, Scripts/UI/{GameUi}.cs, Scripts/UI/Status/{CustAngryStatusRule,CarStatusResolver}.cs, Editor/BootstrapSceneBuilder.cs, Data/UI/HudEmotionConfig.asset, Scenes/Bootstrap.unity (regenerated: GameUi gets the config reference; the file shows id churn), Tests/EditMode/UI/{EmotionTests,PickupMarkTests,HudRulesTests,PlateUiTests,CarStatusTests}.cs
Why: human request (design.md A4 pickup urgency). One pure EmotionRule (patience fraction to Happy / Impatient / Angry; angry also when expired) feeds (a) a small pixel chat bubble with a face over the waiting patron, placed through IPatronLocator so it follows them and vanishes when they stop waiting or the ticket closes, (b) the plate flash colour and speed, (c) the CAR SLOTS highlight, bell and the Cust. Angry status. Colours green / yellow / red, flash 1.5 / 3 / 7 Hz, thresholds 0.6 / 0.85 (the old amber and danger levels) all live in the new HudEmotionConfig asset with [Min]/[Range]/[Tooltip]. The old cyan/amber/orange-red plate flash, FlashPeriod, the patience constants and the patience bar maths in Alarm are removed (dead code). The Cars-module body tint pulse (CarFlashConfig) is untouched and still uses cyan/amber/orange-red.
Tests: EmotionTests (thresholds, expiry, rates and colours, bubble faces, Cust. Angry follows Angry), PickupMarkTests rewritten for emotion tint and speed. Full EditMode run at this commit.
