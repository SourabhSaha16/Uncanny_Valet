# Audio changelog

Entries are added by the audio agent, one per commit. Format in docs/changelogs/README.md.

## audio: add synthesised funk loop and sound effects
Date: 2026-11-06
Files: Assets/_Project/Scripts/Audio/*.cs and UncannyValet.Audio.asmdef (synth maths, FunkLoopBuilder, SfxBank, SfxEventRouter, AudioDirector, AudioConfig), Assets/_Project/Tests/EditMode/Audio/*.cs, Assets/_Project/Tests/EditMode/UncannyValet.Tests.EditMode.asmdef (added UncannyValet.Audio reference)
Why: Audio module with no audio files; everything is generated in code at Start (design.md section 10 listed audio as out of scope for P1, this is the first audio milestone).
Tests: EditMode tests for oscillators, ADSR, noise, low-pass, pitch, mute state, loop length and peak, loop point, every SFX length and peak, and event routing. Not run (Unity not launched, per request).

## audio: play button click on next night
Date: 2026-11-06
Files: Assets/_Project/Scripts/Audio/SfxEventRouter.cs, Assets/_Project/Tests/EditMode/Audio/SfxTests.cs
Why: Core now has NextNightRequested; the summary-card Next button plays SfxId.ButtonClick (design.md section 10 audio).
Tests: Router test extended to cover NextNightRequested and unsubscribe. Not run (Unity not launched). Checked the Audio files for duplicate members by inspection and grep: none.

## audio: add charge, wash and station sounds
Date: 2026-11-06
Files: Assets/_Project/Scripts/Audio/{SfxStations,SfxId,SfxRecipeBook,SfxEventRouter}.cs, Assets/_Project/Tests/EditMode/Audio/SfxTests.cs
Why: Round-4 stations: CarFullyCharged plays a zap and chime, CarCleaned a splash with sparkle and ding, CarEnteredStation a click and low hum.
Tests: Router mapping test extended with the three events; the existing every-effect length and peak test covers the new clips automatically. Not run (Unity not launched). Duplicate-member grep on edited files: none.
