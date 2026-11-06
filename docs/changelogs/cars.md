# Cars changelog

Entries are added by the cars agent, one per commit. Format in docs/changelogs/README.md.

## cars: add driver, interaction, factory and physics maths
Date: 2026-11-05
Files: Assets/_Project/Scripts/Cars/{CarDefinition,CarPhysicsMath,CarDriver,CarInteraction,CarFactory}.cs (+ .meta), Assets/_Project/Data/Cars/Sedan.asset (+ .meta), Assets/_Project/Tests/EditMode/Cars/CarPhysicsMathTests.cs
Why: P1 Cars module (design.md sections 8, 9, 10): top-down Rigidbody2D driving, enter-car interaction via CarEntered, generated-rectangle car factory.
Tests: 8 EditMode tests for CarPhysicsMath added; Unity batch run was blocked by the permission system, so not yet run.

## cars: show car number on each car
Date: 2026-11-05
Files: Assets/_Project/Scripts/Cars/CarLabel.cs (+ .meta), Assets/_Project/Scripts/Cars/CarFactory.cs
Why: Play-test: the player must match cars to ticket list and numbered spots (design.md section 10). TextMesh child with built-in LegacyRuntime font, sorted above the sprite.
Tests: None run (Unity not run, per request).

## cars: fix car number label size
Date: 2026-11-05
Files: Assets/_Project/Scripts/Cars/{CarLabelMath,CarLabel,CarFactory}.cs, Assets/_Project/Tests/EditMode/Cars/CarLabelMathTests.cs
Why: Label digit was about 4 units tall; characterSize now derived from a calibrated digit-height constant so the digit is 0.55 x car width (about 1 unit).
Tests: 3 EditMode tests for CarLabelMath added; Unity not run.

## cars: heavy cars, letter labels, timer fill overlay
Date: 2026-11-06
Files: Assets/_Project/Scripts/Cars/{CarDefinition,CarDriver,CarFactory,CarLabel,CarLabelMath}.cs, new Assets/_Project/Scripts/Cars/{CarSorting,CarTimerFill,CarTimerFillMath}.cs (+ .meta), Assets/_Project/Data/Cars/Sedan.asset, Assets/_Project/Tests/EditMode/Cars/{CarLabelMathTests,CarTimerFillMathTests}.cs (+ .meta)
Why: design.md section 10 pressure rules. (1) Heavy cars: CarDefinition gains Mass (20), ParkedLinearDrag and ParkedAngularDrag (8). The body stays Dynamic so spot/curb triggers are untouched (switching body type was rejected as riskier). Mass is set on the Rigidbody2D and damping is applied only while no driver is seated; seated damping is 0 so handling is unchanged. CarDriver sets velocity directly from CarPhysicsMath, so driving does not depend on mass and no force scaling was needed. (2) CarLabel is now a component subscribed to CarLetterAssigned (OnEnable/OnDisable), filtered by CarId, blank until assigned. (3) CarTimerFill listens to TicketTimerChanged/Expired/Stopped for its car and eases a bottom-up overlay (shared cached 1x1 sprite, child scaled in car-local space, park dark, pickup red) using CarTimerFillMath; sorting is body 10, fill 11, label 12 (CarSorting).
Tests: 8 new EditMode tests (CarTimerFillMath x6, CarLabelMath.LetterText x2); Unity not run, compiled by inspection.

## cars: remove duplicated members
Date: 2026-11-06
Files: Assets/_Project/Scripts/Cars/{CarDefinition,CarDriver}.cs, Assets/_Project/Data/Cars/Sedan.asset
Why: Fix compile break from af46781, where an edit script had applied twice: duplicate fields/properties in CarDefinition, duplicate ApplyDrag in CarDriver, duplicate asset keys. Each member now exists once; other Cars files checked by grep.
Tests: None run (Unity not run); checked by inspection.

## cars: mirrors, EV look with charge bar, mud overlay
Date: 2026-11-06
Files: Assets/_Project/Scripts/Cars/{CarDefinition,CarFactory,CarSorting}.cs, new Assets/_Project/Scripts/Cars/{CarMirrorStyle,CarChargeBarStyle,CarMirrorMath,CarMirrors,CarChargeBarMath,CarMudMath,CarChargeBar,CarMudOverlay,CarTraitLook}.cs (+ .meta), Assets/_Project/Tests/EditMode/Cars/CarLooksMathTests.cs (+ .meta)
Why: design.md section 10 challenge schedule. Two front mirrors on every car; TicketTraitsAssigned (via CarTraitLook) turns EVs blue and activates a charge bar driven by CarChargeChanged/CarFullyCharged (lighter tint when full), and activates a brown mud overlay for dirty cars that fades with CarCleanChanged and hides on CarCleaned. Bar and mud children start inactive so they only subscribe once needed. Tunables live in CarDefinition (nested CarMirrorStyle, CarChargeBarStyle, EvBodyColor, MudColor); Sedan.asset keeps the code defaults for the new fields. Smoothing reuses CarTimerFillMath. Sorting: mirrors 9, body 10, mud 11, timer fill 12, charge track 13, charge fill 14, label 15.
Tests: 7 new EditMode tests (mirror placement, charge fill geometry, mud alpha); Unity not run.
