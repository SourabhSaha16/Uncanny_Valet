namespace UncannyValet.Audio
{
    /// <summary>The waterwheel ambience loop: soft lapping water and a slow creak twice a turn.</summary>
    public static class AmbienceWheel
    {
        private const float Seconds = 7f;
        private const float CreakSeconds = 1.2f;
        private const float CreakGain = 0.3f;
        private const float LapGain = 0.18f;
        private static readonly float[] CreakAt = { 0.5f, 4f };
        private static readonly float[] CreakStartHz = { 150f, 175f };
        private static readonly float[] CreakEndHz = { 105f, 120f };

        public static float[] WaterwheelCreak(int rate)
        {
            var buffer = new MixBuffer((int)(Seconds * rate), rate, true);
            var lap = SeamlessNoise.Band(rate, Seconds, 200f, 900f, 541u);
            SeamlessNoise.Swell(lap, 7f, 0.6f, 0f);
            for (int i = 0; i < lap.Length; i++) buffer.Add(i, lap[i] * LapGain);
            for (int i = 0; i < CreakAt.Length; i++)
            {
                var creak = VoiceSpec.Tone(Waveform.Saw, CreakStartHz[i], CreakSeconds, CreakGain).Gliding(CreakEndHz[i])
                    .Blend(Waveform.Square, 0.3f).WithEnvelope(new Adsr(0.15f, 0.3f, 0.5f, 0.4f))
                    .Filtered(700f, 700f, 1f).WithVibrato(0.03f, 9f);
                buffer.AddVoice(CreakAt[i], creak);
            }
            return AmbienceLoops.Finish(buffer);
        }
    }
}
