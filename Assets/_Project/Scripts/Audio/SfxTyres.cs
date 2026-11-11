namespace UncannyValet.Audio
{
    /// <summary>One-shot tyre sounds (docs/redesign/car_handling_spec.md section 6): the slide chirp and the hard-braking screech.</summary>
    public static class SfxTyres
    {
        private const float ChirpStartHz = 1900f;
        private const float ChirpEndHz = 2700f;
        private const float ChirpSeconds = 0.09f;
        private const float ChirpGain = 0.7f;
        private const float ChirpOvertoneMix = 0.3f;
        private const float ChirpHissSeconds = 0.03f;
        private const float ChirpHissGain = 0.2f;
        private const float HissHighPassHz = 4000f;
        private const float ScreechStartHz = 1250f;
        private const float ScreechEndHz = 900f;
        private const float ScreechSeconds = 0.3f;
        private const float ScreechGain = 0.5f;
        private const float ScreechFilterStartHz = 3000f;
        private const float ScreechFilterEndHz = 1800f;
        private const float ScreechFilterDecay = 0.2f;
        private const float ScreechHissSeconds = 0.28f;
        private const float ScreechHissGain = 0.25f;
        private const uint ChirpSeed = 19u;
        private const uint ScreechSeed = 23u;

        /// <summary>A short, high arcade chirp for the moment a car starts to slide.</summary>
        public static float[] Chirp(int rate)
        {
            var b = new MixBuffer(rate * 15 / 100, rate, false);
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Sine, ChirpStartHz, ChirpSeconds, ChirpGain).Gliding(ChirpEndHz)
                .Blend(Waveform.Triangle, ChirpOvertoneMix).WithEnvelope(new Adsr(0.003f, ChirpSeconds, 0f, 0.03f)));
            DrumSynth.AddNoiseBurst(b, 0, ChirpHissSeconds, ChirpHissGain, HissHighPassHz, ChirpSeed);
            return SfxActions.Finish(b);
        }

        /// <summary>A light rubbery screech for hard braking: a falling, filtered saw over a little hiss.</summary>
        public static float[] Screech(int rate)
        {
            var b = new MixBuffer(rate * 45 / 100, rate, false);
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Saw, ScreechStartHz, ScreechSeconds, ScreechGain).Gliding(ScreechEndHz)
                .Filtered(ScreechFilterStartHz, ScreechFilterEndHz, ScreechFilterDecay)
                .WithEnvelope(new Adsr(0.01f, 0.1f, 0.5f, 0.15f)));
            DrumSynth.AddNoiseBurst(b, 0, ScreechHissSeconds, ScreechHissGain, HissHighPassHz, ScreechSeed);
            return SfxActions.Finish(b);
        }
    }
}
