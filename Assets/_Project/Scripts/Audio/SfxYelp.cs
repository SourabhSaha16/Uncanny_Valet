namespace UncannyValet.Audio
{
    /// <summary>The comic yelp of a pedestrian a car knocked away: two quick squeaky "yip" glides, soft and never painful.</summary>
    public static class SfxYelp
    {
        private const float Seconds = 0.4f;
        private const float RiseStartHz = 520f;
        private const float RiseEndHz = 940f;
        private const float RiseSeconds = 0.13f;
        private const float FallDelay = 0.12f;
        private const float FallStartHz = 880f;
        private const float FallEndHz = 360f;
        private const float FallSeconds = 0.24f;
        private const float Gain = 0.45f;
        private const float OvertoneGain = 0.15f;
        private const float SoftenHz = 3200f;

        /// <summary>The yelp: a squeak that rises then a wobbling fall.</summary>
        public static float[] Yelp(int rate)
        {
            var b = new MixBuffer((int)(rate * Seconds), rate, false);
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Triangle, RiseStartHz, RiseSeconds, Gain).Gliding(RiseEndHz)
                .WithEnvelope(new Adsr(0.008f, RiseSeconds, 0f, 0.03f)));
            b.AddVoice(FallDelay, VoiceSpec.Tone(Waveform.Triangle, FallStartHz, FallSeconds, Gain).Gliding(FallEndHz)
                .WithEnvelope(new Adsr(0.008f, FallSeconds, 0f, 0.06f)));
            b.AddVoice(FallDelay, VoiceSpec.Tone(Waveform.Sine, FallStartHz * 2f, FallSeconds, OvertoneGain).Gliding(FallEndHz * 2f)
                .WithEnvelope(new Adsr(0.008f, FallSeconds, 0f, 0.05f)));
            SoftenFilter.LowPass(b.Samples, rate, SoftenHz);
            return SfxActions.Finish(b);
        }
    }
}
