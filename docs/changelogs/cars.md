# Cars changelog

Entries are added by the cars agent, one per commit. Format in docs/changelogs/README.md.

## cars: add driver, interaction, factory and physics maths
Date: 2026-11-05
Files: Assets/_Project/Scripts/Cars/{CarDefinition,CarPhysicsMath,CarDriver,CarInteraction,CarFactory}.cs (+ .meta), Assets/_Project/Data/Cars/Sedan.asset (+ .meta), Assets/_Project/Tests/EditMode/Cars/CarPhysicsMathTests.cs
Why: P1 Cars module (design.md sections 8, 9, 10): top-down Rigidbody2D driving, enter-car interaction via CarEntered, generated-rectangle car factory.
Tests: 8 EditMode tests for CarPhysicsMath added; Unity batch run was blocked by the permission system, so not yet run.
