namespace UncannyValet.Audio
{
    /// <summary>Sound effects of the valet powers. Original tunes, not a copy of any game's.</summary>
    public static class SfxPowers
    {
        private const float RunStep = 0.05f;
        private const float RunNoteSeconds = 0.07f;
        private const float ChordSeconds = 0.4f;
        private const float ChordNoteGain = 0.35f;
        private const float RunNoteGain = 0.45f;
        private const float BlendMix = 0.4f;
        private const float TotalSeconds = 0.8f;
        private static readonly int[] RunMidi = { 67, 71, 74, 79, 83, 86, 91 };
        private static readonly int[] ChordMidi = { 79, 83, 86, 91 };

        /// <summary>The speed boost fanfare: a quick rising run of bright notes that lands on a held, shimmering chord.</summary>
        public static float[] SpeedBoostFanfare(int rate)
        {
            var b = new MixBuffer((int)(rate * TotalSeconds), rate, false);
            var runEnv = new Adsr(0.002f, RunNoteSeconds, 0.2f, 0.03f);
            for (var i = 0; i < RunMidi.Length; i++)
            {
                var note = VoiceSpec.Tone(Waveform.Square, Pitch.MidiToHz(RunMidi[i]), RunNoteSeconds, RunNoteGain)
                    .Blend(Waveform.Triangle, BlendMix).WithEnvelope(runEnv);
                b.AddVoice(i * RunStep, note);
            }
            var chordEnv = new Adsr(0.005f, ChordSeconds, 0.4f, 0.25f);
            var start = RunMidi.Length * RunStep;
            for (var i = 0; i < ChordMidi.Length; i++)
            {
                var chord = VoiceSpec.Tone(Waveform.Triangle, Pitch.MidiToHz(ChordMidi[i]), ChordSeconds, ChordNoteGain)
                    .Blend(Waveform.Sine, BlendMix).WithEnvelope(chordEnv);
                b.AddVoice(start, chord);
            }
            return SfxActions.Finish(b);
        }
    }
}
