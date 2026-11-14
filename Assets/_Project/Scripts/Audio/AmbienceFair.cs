namespace UncannyValet.Audio
{
    /// <summary>The fair night ambience loops: a crowd murmur and distant rides.</summary>
    public static class AmbienceFair
    {
        private const float MurmurSeconds = 8f;
        private const float RidesSeconds = 10f;
        private const float RideWhooshGain = 0.25f;
        private const float HumGain = 0.25f;
        private static readonly int[] CalliopeMidi = { 72, 76, 79, 76, 74, 77, 81, 77 };

        /// <summary>Two babble bands that swell at different speeds, so it never sounds like one hiss.</summary>
        public static float[] CrowdMurmur(int rate)
        {
            var low = SeamlessNoise.Band(rate, MurmurSeconds, 150f, 700f, 501u);
            var high = SeamlessNoise.Band(rate, MurmurSeconds, 500f, 1500f, 503u);
            SeamlessNoise.Swell(low, 3f, 0.5f, 0f);
            SeamlessNoise.Swell(high, 5f, 0.7f, 0.3f);
            var buffer = new MixBuffer(low.Length, rate, true);
            for (int i = 0; i < low.Length; i++) buffer.Add(i, low[i] + high[i] * 0.5f);
            return AmbienceLoops.Finish(buffer);
        }

        /// <summary>A low ride hum, a slow whoosh and a faint calliope tune far away.</summary>
        public static float[] DistantRides(int rate)
        {
            var buffer = new MixBuffer((int)(RidesSeconds * rate), rate, true);
            var whoosh = SeamlessNoise.Band(rate, RidesSeconds, 300f, 1100f, 509u);
            SeamlessNoise.Swell(whoosh, 1f, 0.95f, 0f);
            for (int i = 0; i < whoosh.Length; i++) buffer.Add(i, whoosh[i] * RideWhooshGain);
            buffer.AddVoice(0f, VoiceSpec.Tone(Waveform.Sine, 55f, RidesSeconds, HumGain).WithEnvelope(new Adsr(0.01f, 0.01f, 1f, 0.01f)));
            float step = RidesSeconds / CalliopeMidi.Length;
            for (int i = 0; i < CalliopeMidi.Length; i++)
            {
                var note = VoiceSpec.Tone(Waveform.Triangle, Pitch.MidiToHz(CalliopeMidi[i]), step * 0.8f, 0.2f)
                    .WithEnvelope(new Adsr(0.02f, 0.1f, 0.6f, 0.15f)).Filtered(900f, 900f, 1f);
                buffer.AddVoice(i * step, note);
            }
            return AmbienceLoops.Finish(buffer);
        }
    }
}
