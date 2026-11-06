# Audio changelog

Entries are added by the audio agent, one per commit. Format in docs/changelogs/README.md.

## audio: add synthesised funk loop and sound effects
Date: 2026-11-06
Files: Assets/_Project/Scripts/Audio/*.cs and UncannyValet.Audio.asmdef (synth maths, FunkLoopBuilder, SfxBank, SfxEventRouter, AudioDirector, AudioConfig), Assets/_Project/Tests/EditMode/Audio/*.cs, Assets/_Project/Tests/EditMode/UncannyValet.Tests.EditMode.asmdef (added UncannyValet.Audio reference)
Why: Audio module with no audio files; everything is generated in code at Start (design.md section 10 listed audio as out of scope for P1, this is the first audio milestone).
Tests: EditMode tests for oscillators, ADSR, noise, low-pass, pitch, mute state, loop length and peak, loop point, every SFX length and peak, and event routing. Not run (Unity not launched, per request).
