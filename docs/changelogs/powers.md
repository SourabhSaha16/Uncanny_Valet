# powers changelog

## powers: add Powers module, meter, stacking and F2 keys
Date: 2026-11-11
Files: Assets/_Project/Scripts/Powers/* (UncannyValet.Powers asmdef, PowerMeter, PowerMeterConfig, PowerSettings, DriftFillTracker, PowerTimer, IPowerEffect, SpeedBoostEffect, FreezeTimeEffect, ParkSmashEffect, PowerStack, IPowerInput, KeyboardPowerInput, PowerMeterController, PowerActivationController); Assets/_Project/Scripts/Core/Events/TuningKey.cs (11 Power* keys), Core/Contracts/PowerRows.cs (new), Core/Contracts/TuningCatalog.cs; Bootstrap/TuningSeeder.cs + Bootstrap asmdef (seed power keys, needed by the existing "every catalog key is seeded" test); Tests/EditMode/Powers/*, Tests asmdef reference.
Why: design.md A11, docs/redesign/valet_powers.md; issues #37 (meter), #40 (activation), #44 (stacking), #38 (non-art parts).
Tests: 20 new EditMode tests; full EditMode suite 1015 passed, 0 failed.

Wiring needed (integration/lead), in the Bootstrap scene or GameBootstrap:
- Add one GameObject with `PowerMeterController` (assign a `PowerMeterConfig` asset, create via Assets > Create > Uncanny Valet > Power Meter Config; defaults match the spec) and `PowerActivationController` (assign the meter controller). Both resolve `IEventBus` in Start, so they must start after the bus is registered. `PowerActivationController` registers `IWorldTime` in Start (FreezeTimeEffect); consumers should resolve it with `Services.TryGet` in their own Start or lazily.
- Optional: pass the config to `TuningSeeder.Seed(..., powers: config)`; without it the seeder uses config defaults.
- Optional: register an `IPowerInput` to replace the built-in Q / left shoulder `KeyboardPowerInput`.
- Valet adoption (later task): read `PowerActivationController.SpeedBoost.WalkMultiplier`.

Open seams and deviations:
- `PerfectPark` event (lot agent, issue #16) is not in Core yet: `PowerMeterController.AddPerfectPark()` is the named seam; subscribe to the event there when it lands.
- Activate input lives in the Powers module (`IPowerInput`) so the Valet module and `IInputSource` are untouched.
- Manager (`PowerStack`) publishes PowerActivated for every power, including Park Smash; the effects themselves stay free of events.
- Edited Bootstrap `TuningSeeder` and its asmdef (one optional parameter) because `TuningSeederTests` requires every catalog key to be seeded.

## powers: fill the meter from PerfectPark
Date: 2026-11-11
Files: Assets/_Project/Scripts/Powers/PowerMeterController.cs, Assets/_Project/Tests/EditMode/Powers/PowerMeterControllerTests.cs
Why: PerfectPark now exists in Core (ParkEvents.cs); it replaces the AddPerfectPark seam (design.md A11, issue #38).
Tests: 4 new controller tests (park and drop-off fill, shift reset, unsubscribe on disable); full EditMode suite run.

## powers: freeze time stops the world's own clocks, with frost look and sound [Claude]
Date: 2026-11-11
Files: Core/WorldTimeReader.cs, Core/Contracts/IPlayerFocus.cs, Bootstrap/TransformPlayerFocus.cs, Bootstrap/GameBootstrap.cs, Night/ShiftClock.cs, Director/DirectorTicker.cs, Director/PatronDirector.cs, Traffic/TrafficModule.cs, Pedestrians/PedestrianCrowd.cs, Pedestrians/PedestrianWalker.cs, Cars/CarAutoDrive.cs, UI/Hud/FreezeFade.cs, FreezeTracker.cs, FreezeLook.cs, FreezeVeilPainter.cs, PlayingHud.cs, Audio/SfxFreeze.cs, SfxId.cs (TimeStop, TimeResume appended), SfxRecipeBook.cs, SfxEventRouter.cs, tests (EditMode Core/Director/Night/UI/Powers, PlayMode FreezeTimePlayModeTests, PlayMode asmdef gets Night)
Why: issue #42, Power 2 (docs/redesign/valet_powers.md section 3 and 4, design.md A11). Each module scales its own delta by IWorldTime through the new WorldTimeReader (lazy lookup, one refresh per real second): shift clock, Director (paused while slowed, so it sends no car), ambient traffic, auto-driven cars (trip and straight arrive/depart), ambient and patron walkers' stride. Patrons need no edit: all their timers count clock game minutes. The valet and the driven car are untouched. HUD countdown already uses unscaled time. Look: cold blue wash with a clear bubble round the valet or driven car (IPlayerFocus) plus frost bands and ice spikes, fading in real time; low time-stop whoosh and a short tick on resume.
Tests: 25 new EditMode tests and 2 PlayMode tests; full EditMode and PlayMode suites run.
