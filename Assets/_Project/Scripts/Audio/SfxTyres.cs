namespace UncannyValet.Audio
{
    /// <summary>
    /// One-shot tyre sounds (docs/redesign/car_handling_spec.md section 6): the slide chirp and the hard-braking screech.
    /// Both are soft: sine or triangle tones in the low hundreds of Hz, a rounded attack, and a low-pass set by the sharpness knob.
    /// </summary>
    public static class SfxTyres
    {
        /// <summary>Sharpness used when none is given (0 = dull, 1 = bright).</summary>
        public const float DefaultSharpness = 0.35f;

        private const float ChirpStartHz = 600f;
        private const float ChirpEndHz = 850f;
        private const float ChirpSeconds = 0.06f;
        private const float ChirpGain = 0.6f;
        private const float ChirpAttack = 0.01f;
        private const float ScreechStartHz = 480f;
        private const float ScreechEndHz = 360f;
        private const float ScreechSeconds = 0.25f;
        private const float ScreechGain = 0.5f;
        private const float ScreechOvertoneMix = 0.35f;
        private const float ScreechAttack = 0.03f;
        private const float ScreechFilterStartHz = 1200f;
        private const float ScreechFilterEndHz = 700f;
        private const float ScreechFilterDecay = 0.2f;

        /// <summary>A short, soft, low chirp for the moment a car starts to slide.</summary>
        public static float[] Chirp(int rate, float sharpness = DefaultSharpness)
        {
            var b = new MixBuffer(rate * 12 / 100, rate, false);
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Sine, ChirpStartHz, ChirpSeconds, ChirpGain).Gliding(ChirpEndHz)
                .WithEnvelope(new Adsr(ChirpAttack, ChirpSeconds, 0f, 0.03f)));
            SoftenFilter.LowPass(b.Samples, rate, SoftenFilter.CutoffFor(sharpness));
            return SfxActions.Finish(b);
        }

        /// <summary>A light, rubbery screech for hard braking: a falling triangle with a hint of saw, filtered and rounded.</summary>
        public static float[] Screech(int rate, float sharpness = DefaultSharpness)
        {
            var b = new MixBuffer(rate * 40 / 100, rate, false);
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Triangle, ScreechStartHz, ScreechSeconds, ScreechGain).Gliding(ScreechEndHz)
                .Blend(Waveform.Saw, ScreechOvertoneMix)
                .Filtered(ScreechFilterStartHz, ScreechFilterEndHz, ScreechFilterDecay)
                .WithEnvelope(new Adsr(ScreechAttack, 0.1f, 0.5f, 0.12f)));
            SoftenFilter.LowPass(b.Samples, rate, SoftenFilter.CutoffFor(sharpness));
            return SfxActions.Finish(b);
        }
    }
}
