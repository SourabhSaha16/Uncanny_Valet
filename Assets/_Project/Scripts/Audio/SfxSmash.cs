namespace UncannyValet.Audio
{
    /// <summary>Sound effects of the Park Smash power: a heavy impact boom and a bright chime in four rising steps (one step per car done).</summary>
    public static class SfxSmash
    {
        /// <summary>Number of chime steps; the chime of a car beyond the last one repeats the top step.</summary>
        public const int ChimeSteps = 4;

        private const float BoomSeconds = 0.85f;
        private const float SubStartHz = 95f;
        private const float SubEndHz = 30f;
        private const float SubGain = 1f;
        private const float ThudStartHz = 190f;
        private const float ThudEndHz = 55f;
        private const float ThudSeconds = 0.3f;
        private const float ThudGain = 0.7f;
        private const float CrackSeconds = 0.22f;
        private const float CrackGain = 0.8f;
        private const float CrackHighPassHz = 400f;
        private const uint CrackSeed = 31u;
        private const float RumbleSeconds = 0.5f;
        private const float RumbleGain = 0.35f;
        private const float RumbleHighPassHz = 90f;
        private const uint RumbleSeed = 37u;
        private const float ChimeSeconds = 0.5f;
        private const float ChimeGain = 0.6f;
        private const float ChimePartialRatio = 2f;
        private const float ChimePartialGain = 0.3f;
        private const float ChimeFifthRatio = 1.5f;
        private const float ChimeFifthGain = 0.25f;
        private static readonly int[] ChimeMidi = { 76, 79, 83, 88 };

        /// <summary>The impact: a deep sub drop and a mid thud under a short noise crack and a low rumble.</summary>
        public static float[] Boom(int rate)
        {
            var b = new MixBuffer((int)(rate * BoomSeconds), rate, false);
            var subEnv = new Adsr(0.003f, BoomSeconds * 0.6f, 0.2f, 0.25f);
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Sine, SubStartHz, BoomSeconds * 0.9f, SubGain)
                .Gliding(SubEndHz).WithEnvelope(subEnv));
            var thudEnv = new Adsr(0.002f, ThudSeconds * 0.6f, 0f, 0.08f);
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Square, ThudStartHz, ThudSeconds, ThudGain)
                .Gliding(ThudEndHz).Blend(Waveform.Triangle, 0.5f).WithEnvelope(thudEnv));
            DrumSynth.AddNoiseBurst(b, 0, CrackSeconds, CrackGain, CrackHighPassHz, CrackSeed);
            DrumSynth.AddNoiseBurst(b, 0, RumbleSeconds, RumbleGain, RumbleHighPassHz, RumbleSeed);
            return SfxActions.Finish(b);
        }

        /// <summary>A bell-like chime; <paramref name="step"/> 0 to <see cref="ChimeSteps"/> - 1 climbs the scale (clamped).</summary>
        public static float[] Chime(int rate, int step)
        {
            var index = step < 0 ? 0 : step >= ChimeSteps ? ChimeSteps - 1 : step;
            var hz = Pitch.MidiToHz(ChimeMidi[index]);
            var b = new MixBuffer((int)(rate * ChimeSeconds), rate, false);
            var env = new Adsr(0.002f, ChimeSeconds * 0.5f, 0f, 0.2f);
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Sine, hz, ChimeSeconds, ChimeGain).WithEnvelope(env));
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Sine, hz * ChimePartialRatio, ChimeSeconds, ChimePartialGain).WithEnvelope(env));
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Triangle, hz * ChimeFifthRatio, ChimeSeconds, ChimeFifthGain).WithEnvelope(env));
            return SfxActions.Finish(b);
        }
    }
}
