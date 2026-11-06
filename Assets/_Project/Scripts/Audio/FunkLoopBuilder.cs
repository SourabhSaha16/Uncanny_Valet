using System;

namespace UncannyValet.Audio
{
    /// <summary>
    /// Renders one seamless funk loop (mono): slap-style bass, swung kick/snare/hat and a quiet chord stab.
    /// Note tails past the end wrap onto the start, so the loop point has no click.
    /// </summary>
    public static class FunkLoopBuilder
    {
        private const float BassCutoffOpenHz = 2600f;
        private const float BassCutoffClosedHz = 300f;
        private const float BassCutoffDecay = 0.05f;
        private const float BassSquareMix = 0.4f;
        private const float StabCutoffHz = 1800f;
        private const float StabTriangleMix = 0.5f;
        private const uint HatSeedBase = 101u;

        /// <summary>Builds the loop. Length is exactly <see cref="LoopSettings.LengthSamples"/>.</summary>
        public static float[] Build(in LoopSettings settings)
        {
            var buffer = new MixBuffer(settings.LengthSamples, settings.SampleRate, true);
            double stepSamples = settings.SamplesPerBeat / (double)LoopSettings.StepsPerBeat;
            for (int step = 0; step < settings.TotalSteps; step++)
            {
                double swung = step + (step % 2 == 1 ? settings.Swing : 0f);
                int start = (int)Math.Round(swung * stepSamples);
                int index = step % FunkPattern.Steps;
                AddDrums(buffer, start, index, step);
                AddBass(buffer, start, index, settings);
                AddStab(buffer, start, index, settings);
            }
            buffer.NormalizeTo(FunkPattern.NormalizedPeak);
            return buffer.Samples;
        }

        private static void AddDrums(MixBuffer buffer, int start, int index, int step)
        {
            if (FunkPattern.Kick[index]) DrumSynth.AddKick(buffer, start, FunkPattern.KickGain);
            if (FunkPattern.Snare[index]) DrumSynth.AddSnare(buffer, start, FunkPattern.SnareGain);
            else if (index == FunkPattern.SnareGhostStep)
                DrumSynth.AddSnare(buffer, start, FunkPattern.SnareGhostGain);
            bool accent = step % LoopSettings.StepsPerBeat == 2;
            float hat = accent ? FunkPattern.HatAccentGain : FunkPattern.HatGain;
            DrumSynth.AddHat(buffer, start, hat, HatSeedBase + (uint)step);
        }

        private static void AddBass(MixBuffer buffer, int start, int index, in LoopSettings settings)
        {
            int note = FunkPattern.Bass[index];
            if (note == FunkPattern.Rest) return;
            float hz = Pitch.MidiToHz(settings.RootMidi + note);
            var env = new Adsr(0.003f, 0.06f, 0.5f, 0.04f);
            var voice = VoiceSpec.Tone(Waveform.Saw, hz, FunkPattern.BassSeconds, FunkPattern.BassGain)
                .Blend(Waveform.Square, BassSquareMix).WithEnvelope(env)
                .Filtered(BassCutoffOpenHz, BassCutoffClosedHz, BassCutoffDecay);
            VoiceRenderer.Render(buffer, start, voice);
        }

        private static void AddStab(MixBuffer buffer, int start, int index, in LoopSettings settings)
        {
            if (!FunkPattern.StabSteps[index]) return;
            var env = new Adsr(0.004f, 0.05f, 0.4f, 0.05f);
            foreach (int semitone in FunkPattern.Chord)
            {
                float hz = Pitch.MidiToHz(settings.RootMidi + FunkPattern.ChordOctaveShift + semitone);
                var voice = VoiceSpec.Tone(Waveform.Square, hz, FunkPattern.StabSeconds, FunkPattern.StabGain)
                    .Blend(Waveform.Triangle, StabTriangleMix).WithEnvelope(env)
                    .Filtered(StabCutoffHz, StabCutoffHz, 1f);
                VoiceRenderer.Render(buffer, start, voice);
            }
        }
    }
}
