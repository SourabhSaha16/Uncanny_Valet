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
