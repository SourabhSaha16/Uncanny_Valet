namespace UncannyValet.Audio
{
    /// <summary>The Thunder stride sound: a sharp noise crack on top of a falling saw zap, with a low rumble that dies away.</summary>
    public static class SfxThunder
    {
        private const float Seconds = 0.7f;
        private const float CrackSeconds = 0.12f;
        private const float CrackGain = 0.9f;
        private const float CrackHighPassHz = 1500f;
        private const uint CrackSeed = 31u;
        private const float ZapStartHz = 1400f;
        private const float ZapEndHz = 180f;
        private const float ZapSeconds = 0.18f;
        private const float ZapGain = 0.4f;
        private const float RumbleSeconds = 0.6f;
        private const float RumbleGain = 0.45f;
        private const float RumbleHighPassHz = 120f;
        private const uint RumbleSeed = 37u;
        private const float RumbleDelay = 0.04f;
        private const float SubHz = 55f;
        private const float SubEndHz = 32f;
        private const float SubGain = 0.7f;

        /// <summary>The crack of thunder when the valet dashes or boosts.</summary>
        public static float[] Crack(int rate)
        {
            var b = new MixBuffer((int)(rate * Seconds), rate, false);
            DrumSynth.AddNoiseBurst(b, 0, CrackSeconds, CrackGain, CrackHighPassHz, CrackSeed);
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Saw, ZapStartHz, ZapSeconds, ZapGain).Gliding(ZapEndHz)
                .WithEnvelope(new Adsr(0.001f, ZapSeconds * 0.6f, 0f, 0.05f)));
            DrumSynth.AddNoiseBurst(b, (int)(RumbleDelay * rate), RumbleSeconds, RumbleGain, RumbleHighPassHz, RumbleSeed);
            b.AddVoice(RumbleDelay, VoiceSpec.Tone(Waveform.Sine, SubHz, RumbleSeconds, SubGain).Gliding(SubEndHz)
                .WithEnvelope(new Adsr(0.005f, RumbleSeconds * 0.6f, 0.1f, 0.2f)));
            return SfxActions.Finish(b);
        }
    }
}
