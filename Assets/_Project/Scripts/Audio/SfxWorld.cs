namespace UncannyValet.Audio
{
    /// <summary>Sound effects of the story worlds: the warm blessing chime, the wash splash and the mud squelch.</summary>
    public static class SfxWorld
    {
        private const float ChimeStep = 0.09f;
        private const float ChimeSeconds = 0.6f;
        private static readonly int[] ChimeMidi = { 76, 83, 88 };
        private const float SplashSeconds = 0.45f;
        private const float BubbleStep = 0.07f;
        private const float BubbleSeconds = 0.06f;
        private static readonly float[] BubbleHz = { 1200f, 1700f, 1400f, 2200f, 1900f };

        /// <summary>A soft warm chime: three bell-like sines rising, each ringing out.</summary>
        public static float[] BlessingChime(int rate)
        {
            var b = new MixBuffer(rate, rate, false);
            var env = new Adsr(0.004f, ChimeSeconds, 0f, 0.15f);
            for (int i = 0; i < ChimeMidi.Length; i++)
            {
                float hz = Pitch.MidiToHz(ChimeMidi[i]);
                var bell = VoiceSpec.Tone(Waveform.Sine, hz, ChimeSeconds, 0.45f).Blend(Waveform.Triangle, 0.2f).WithEnvelope(env);
                b.AddVoice(i * ChimeStep, bell);
                b.AddVoice(i * ChimeStep, VoiceSpec.Tone(Waveform.Sine, hz * 2f, ChimeSeconds * 0.5f, 0.12f).WithEnvelope(env));
            }
            return SfxActions.Finish(b);
        }

        /// <summary>A bright splash: a wash of high noise with rising bubble pings.</summary>
        public static float[] WashSplash(int rate)
        {
            var b = new MixBuffer(rate * 8 / 10, rate, false);
            DrumSynth.AddNoiseBurst(b, 0, SplashSeconds, 0.45f, 3000f, 701u);
            DrumSynth.AddNoiseBurst(b, (int)(0.12f * rate), SplashSeconds * 0.7f, 0.3f, 4500f, 709u);
            var env = new Adsr(0.002f, BubbleSeconds, 0f, 0.03f);
            for (int i = 0; i < BubbleHz.Length; i++)
                b.AddVoice(0.05f + i * BubbleStep, VoiceSpec.Tone(Waveform.Sine, BubbleHz[i], BubbleSeconds, 0.3f).Gliding(BubbleHz[i] * 1.6f).WithEnvelope(env));
            return SfxActions.Finish(b);
        }

        /// <summary>A wet squelch: a wobbling saw falling in pitch under a dull slap of noise.</summary>
        public static float[] MudSquelch(int rate)
        {
            var b = new MixBuffer(rate * 5 / 10, rate, false);
            var glug = VoiceSpec.Tone(Waveform.Saw, 260f, 0.28f, 0.6f).Gliding(70f).Blend(Waveform.Square, 0.3f)
                .WithEnvelope(new Adsr(0.004f, 0.2f, 0.2f, 0.08f)).Filtered(900f, 250f, 0.15f).WithVibrato(0.08f, 22f);
            b.AddVoice(0f, glug);
            DrumSynth.AddNoiseBurst(b, 0, 0.12f, 0.25f, 150f, 719u);
            b.AddVoice(0.1f, VoiceSpec.Tone(Waveform.Sine, 400f, 0.05f, 0.4f).Gliding(150f).WithEnvelope(new Adsr(0.002f, 0.05f, 0f, 0.02f)));
            return SfxActions.Finish(b);
        }
    }
}
