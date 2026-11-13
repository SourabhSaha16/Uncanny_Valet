namespace UncannyValet.Audio
{
    /// <summary>Sound effects for what the valet does: enter, exit, park, earn a blessing.</summary>
    public static class SfxActions
    {
        private const float ClickSeconds = 0.02f;
        private const float ClickHighPassHz = 2000f;
        private const float BlipSeconds = 0.12f;
        private const float LowBlipHz = 110f;
        private const float HighBlipHz = 220f;
        private const float ExitDelay = 0.1f;
        private const float DingStep = 0.12f;
        private const float DingSeconds = 0.25f;
        private const float DingOvertoneMix = 0.3f;
        private const float ArpStep = 0.07f;
        private const float ArpNoteSeconds = 0.08f;
        private static readonly int[] DingMidi = { 79, 84 };
        private static readonly int[] ArpMidi = { 84, 88, 91, 96 };

        public static float[] CarEnter(int rate)
        {
            var b = new MixBuffer(rate * 3 / 10, rate, false);
            DrumSynth.AddNoiseBurst(b, 0, ClickSeconds, 0.5f, ClickHighPassHz, 11u);
            b.AddVoice(ClickSeconds, VoiceSpec.Tone(Waveform.Triangle, HighBlipHz, BlipSeconds, 0.7f).Gliding(LowBlipHz));
            return Finish(b);
        }

        public static float[] CarExit(int rate)
        {
            var b = new MixBuffer(rate * 3 / 10, rate, false);
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Triangle, LowBlipHz, BlipSeconds, 0.7f).Gliding(HighBlipHz));
            DrumSynth.AddNoiseBurst(b, (int)(ExitDelay * rate), ClickSeconds, 0.5f, ClickHighPassHz, 13u);
            return Finish(b);
        }

        public static float[] CarParked(int rate)
        {
            var b = new MixBuffer(rate * 8 / 10, rate, false);
            var env = new Adsr(0.002f, 0.2f, 0f, 0.3f);
            for (int i = 0; i < DingMidi.Length; i++)
            {
                var ding = VoiceSpec.Tone(Waveform.Sine, Pitch.MidiToHz(DingMidi[i]), DingSeconds, 0.6f)
                    .Blend(Waveform.Triangle, DingOvertoneMix).WithEnvelope(env);
                b.AddVoice(i * DingStep, ding);
            }
            return Finish(b);
        }

        public static float[] BlessingEarned(int rate)
        {
            var b = new MixBuffer(rate * 6 / 10, rate, false);
            var env = new Adsr(0.002f, 0.06f, 0.3f, 0.12f);
            for (int i = 0; i < ArpMidi.Length; i++)
            {
                var coin = VoiceSpec.Tone(Waveform.Square, Pitch.MidiToHz(ArpMidi[i]), ArpNoteSeconds, 0.4f)
                    .Blend(Waveform.Triangle, 0.5f).WithEnvelope(env);
                b.AddVoice(i * ArpStep, coin);
            }
            return Finish(b);
        }

        internal static float[] Finish(MixBuffer buffer)
        {
            buffer.NormalizeTo(FunkPattern.NormalizedPeak);
            return buffer.Samples;
        }
    }
}
