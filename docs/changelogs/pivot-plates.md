# Pivot plates changelog

Source: docs/redesign/ui_style_guide.md, "Revision 2" (binding).

## pivot-plates: licence plates, tinted car bodies, remove legacy meters
Date: 2026-11-09
Files:
- Cars: removed CarChargeBar, CarChargeBarMath, CarChargeBarStyle, CarTimerFill (and their CarDefinition fields ChargeBar, ParkFillColor, PickupFillColor; CarSorting layers Outline/TimerFill/ChargeTrack/ChargeFill/TagBacking/Tag). CarTimerFillMath keeps only Smooth (used by the mud wipe). CarKeyTag rewritten: no roof stripe or rectangular ring; it tints the body sprite and pulses the tint toward the alarm colour while a pickup is wanted. New CarTintMath (pure). CarBodyArt gains SetTint and left-right mirroring; CarArtMath gains IsFlipped, MirrorCenter and a murmur-style hash for PickIndex; CarFlashConfig now holds tint softness, EV tint strength, pulse mix and the flash tunables (shape fields removed); CarFactory/CarTraitLook updated.
- Core: CarEvents adds CarPlateAssigned (additive).
- Patrons: new PlateFormat (3 letters from BCDFGHJKLMNPRSTVWXZ + 3 digits), PlatePool (unique among held plates, IPatronRandom injected), SeededPatronRandom (xorshift32); Ticket.Plate; TicketBook owns the pool (default seed 20261109, injectable) and releases the plate in Remove/ResetTags; TicketRegistry takes an optional PlatePool and publishes CarPlateAssigned after CarTagAssigned.
- UI: TicketRow.Plate/HasPlate, TicketBoard (CarPlateAssigned), PickupBannerFormatter ("PICKUP: ABC 123 spot 4"), new Hud/PlateLayout and Hud/PlatePainter, WorldTicketPainter draws a plate under every ticketed car (border and glow flash from the pickup request, faster as patience runs out, alarm colours), KeyBoardPainter shows tag swatch, plate, spot and a shorter patience bar (car icon removed, panel 124 wide), GameUi passes the time.
- Editor: CarArtBuilder adds the two police cells (14 regular sprites, 2 EV).
- Tests: added Patrons/PlateTests, Patrons/TicketPlateTests, Cars/CarTintMathTests, UI/PlateUiTests; extended CarArtMathTests and PickupBannerTests; removed the charge-bar and timer-overlay maths tests.
Why: ui_style_guide.md Revision 2. The info box above the car is the only place for dirt and charge meters; the plate is the car's identity on the lot, key board and pickup banner; the ticket colour is the car body tint (softened toward white, EVs only lightly); more car looks from Cars-Sheet. CarChargeChanged and the timer events stay: TicketBoard, the info box and audio still use them.
Tests: new and extended EditMode fixtures listed above. NOT run: Unity could not be launched in this sandbox, so compilation and the suite are unverified. New .meta files are not committed; Unity creates them on first import.

### Decisions to know
- Plate drawn with the existing pixel kit (UI canvas projected from the car, like the info box), not a world sprite: the kit has the font, and the text is a cached string per ticket row so nothing allocates per frame; no per-plate texture is needed. It keeps a fixed pixel size (does not scale with camera zoom), under the car in screen space.
- Pickup flash: plate border plus glow, and a body tint pulse. A sprite-shaped silhouette outline was not done: it needs a flat silhouette texture (Read/Write sprites or a shader), which cannot be checked here.
- Car variety: Cars-Sheet has only 12 civilian and 2 EV top-down cells (all were already used; the rest are side views, ambulances or the fire truck). Variety now comes from adding the 2 police cells (same footprint), mirroring each sprite for half of the car ids, and the per-ticket tint. Ambulances (longer) and the fire truck are excluded.
