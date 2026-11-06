namespace UncannyValet.Audio
{
    /// <summary>Sound effects for the charging and wash stations.</summary>
    public static class SfxStations
    {
        private const float ZapStartHz = 400f;
        private const float ZapEndHz = 1600f;
        private const float ZapSeconds = 0.18f;
        private const float ChimeDelay = 0.16f;
        private const float ChimeMidi = 96f;
        private const float ChimeSeconds = 0.2f;
        private const float SplashSeconds = 0.35f;
        private const float SplashHighPassHz = 2500f;
        private const float SparkleStep = 0.09f;
        private const float SparkleSeconds = 0.1f;
        private const float DingDelay = 0.3f;
        private const float DingMidi = 93f;
        private const float DingSeconds = 0.2f;
        private const float HumHz = 70f;
        private const float HumSeconds = 0.25f;
        private const float HumFilterHz = 400f;
        private const float StartClickSeconds = 0.015f;
        private const float StartClickHighPassHz = 1500f;
        private static readonly int[] SparkleMidi = { 91, 95, 98 };

        public static float[] Charged(int rate)
        {
            var b = new MixBuffer(rate * 6 / 10, rate, false);
            var zap = VoiceSpec.Tone(Waveform.Saw, ZapStartHz, ZapSeconds, 0.5f)
                .Gliding(ZapEndHz).Blend(Waveform.Square, 0.4f);
            b.AddVoice(0f, zap);
            var env = new Adsr(0.002f, 0.2f, 0f, 0.2f);
            var chime = VoiceSpec.Tone(Waveform.Sine, Pitch.MidiToHz(ChimeMidi), ChimeSeconds, 0.6f).WithEnvelope(env);
            b.AddVoice(ChimeDelay, chime);
            return SfxActions.Finish(b);
        }

        public static float[] Washed(int rate)
        {
            var b = new MixBuffer(rate * 8 / 10, rate, false);
            DrumSynth.AddNoiseBurst(b, 0, SplashSeconds, 0.3f, SplashHighPassHz, 23u);
            var env = new Adsr(0.002f, 0.08f, 0f, 0.1f);
            for (int i = 0; i < SparkleMidi.Length; i++)
            {
                var spark = VoiceSpec.Tone(Waveform.Sine, Pitch.MidiToHz(SparkleMidi[i]), SparkleSeconds, 0.35f)
                    .WithEnvelope(env);
                b.AddVoice(i * SparkleStep, spark);
            }
            var ding = new Adsr(0.002f, 0.25f, 0f, 0.25f);
            b.AddVoice(DingDelay, VoiceSpec.Tone(Waveform.Sine, Pitch.MidiToHz(DingMidi), DingSeconds, 0.6f).WithEnvelope(ding));
            return SfxActions.Finish(b);
        }

        public static float[] StationStart(int rate)
        {
            var b = new MixBuffer(rate * 4 / 10, rate, false);
            DrumSynth.AddNoiseBurst(b, 0, StartClickSeconds, 0.4f, StartClickHighPassHz, 29u);
            var hum = VoiceSpec.Tone(Waveform.Saw, HumHz, HumSeconds, 0.5f)
                .Blend(Waveform.Square, 0.3f).Filtered(HumFilterHz, HumFilterHz, 1f);
            b.AddVoice(StartClickSeconds, hum);
            return SfxActions.Finish(b);
        }
    }
}
