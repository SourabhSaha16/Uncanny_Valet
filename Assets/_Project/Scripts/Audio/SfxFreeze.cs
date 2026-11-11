namespace UncannyValet.Audio
{
    /// <summary>Sound effects of the freeze time power: a low "time stop" whoosh and a lower, short tick when time resumes.</summary>
    public static class SfxFreeze
    {
        private const float StopSeconds = 0.9f;
        private const float SweepStartHz = 260f;
        private const float SweepEndHz = 45f;
        private const float SweepGain = 0.5f;
        private const float SweepCutStartHz = 1800f;
        private const float SweepCutEndHz = 120f;
        private const float SubStartHz = 110f;
        private const float SubEndHz = 36f;
        private const float SubGain = 0.7f;
        private const float WhooshSeconds = 0.35f;
        private const float WhooshGain = 0.4f;
        private const float WhooshHighPassHz = 700f;
        private const uint WhooshSeed = 29u;
        private const float TickSeconds = 0.12f;
        private const float TickGap = 0.07f;
        private const float TickGain = 0.6f;
        private const float TickLowHz = 70f;
        private const float TickHighHz = 120f;
        private const float TickBlend = 0.5f;

        /// <summary>The time stop: a falling saw sweep that closes into a deep sub drop, with a short air whoosh at the start.</summary>
        public static float[] TimeStop(int rate)
        {
            var b = new MixBuffer((int)(rate * StopSeconds), rate, false);
            var sweepEnv = new Adsr(0.01f, StopSeconds * 0.6f, 0.2f, 0.25f);
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Saw, SweepStartHz, StopSeconds * 0.8f, SweepGain)
                .Gliding(SweepEndHz).Filtered(SweepCutStartHz, SweepCutEndHz, StopSeconds * 0.7f).WithEnvelope(sweepEnv));
            var subEnv = new Adsr(0.02f, StopSeconds * 0.5f, 0.3f, 0.3f);
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Sine, SubStartHz, StopSeconds * 0.8f, SubGain)
                .Gliding(SubEndHz).WithEnvelope(subEnv));
            DrumSynth.AddNoiseBurst(b, 0, WhooshSeconds, WhooshGain, WhooshHighPassHz, WhooshSeed);
            return SfxActions.Finish(b);
        }

        /// <summary>Time resumes: two low ticks, the second one higher, like a clock catching up.</summary>
        public static float[] TimeResume(int rate)
        {
            var b = new MixBuffer((int)(rate * (TickSeconds + TickGap + TickSeconds)), rate, false);
            var env = new Adsr(0.002f, 0.04f, 0f, 0.05f);
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Square, TickLowHz, TickSeconds, TickGain)
                .Blend(Waveform.Triangle, TickBlend).WithEnvelope(env));
            b.AddVoice(TickGap, VoiceSpec.Tone(Waveform.Square, TickHighHz, TickSeconds, TickGain)
                .Blend(Waveform.Triangle, TickBlend).WithEnvelope(env));
            return SfxActions.Finish(b);
        }
    }
}
