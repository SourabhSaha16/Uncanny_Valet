namespace UncannyValet.Audio
{
    /// <summary>Sound effects for things that happen to the valet: pickups, lost cars, arrivals, end of night.</summary>
    public static class SfxAlerts
    {
        private const float BellMidi = 88f;
        private const float BellPartialRatio = 2.76f;
        private const float BellSeconds = 0.4f;
        private const float BellRepeat = 0.35f;
        private const float SadStartHz = 400f;
        private const float SadEndHz = 120f;
        private const float SadSeconds = 0.4f;
        private const float HonkSeconds = 0.14f;
        private const float HonkRepeat = 0.2f;
        private const float HonkHz = 233f;
        private const float HonkFilterHz = 1500f;
        private const float JingleStep = 0.12f;
        private const float JingleNoteSeconds = 0.1f;
        private const float JingleLastSeconds = 0.3f;
        private const float ClickSeconds = 0.03f;
        private const float ClickHighPassHz = 3000f;
        private const float ClickToneHz = 880f;
        private const float BumpStartHz = 170f;
        private const float BumpEndHz = 340f;
        private const float BumpSeconds = 0.14f;
        private const float BumpGain = 0.5f;
        private const float ThudStartHz = 150f;
        private const float ThudEndHz = 48f;
        private const float ThudSeconds = 0.16f;
        private const float ThudGain = 0.8f;
        private const float KnockHz = 95f;
        private const float KnockSeconds = 0.05f;
        private const float KnockGain = 0.3f;
        private const float KnockFilterHz = 700f;
        private static readonly int[] JingleMidi = { 72, 76, 79, 84 };

        public static float[] PickupRequested(int rate)
        {
            var b = new MixBuffer(rate * 9 / 10, rate, false);
            var env = new Adsr(0.002f, 0.5f, 0f, 0.3f);
            float hz = Pitch.MidiToHz(BellMidi);
            for (int i = 0; i < 2; i++)
            {
                b.AddVoice(i * BellRepeat, VoiceSpec.Tone(Waveform.Sine, hz, BellSeconds, 0.6f).WithEnvelope(env));
                b.AddVoice(i * BellRepeat, VoiceSpec.Tone(Waveform.Sine, hz * BellPartialRatio, BellSeconds, 0.2f).WithEnvelope(env));
            }
            return SfxActions.Finish(b);
        }

        public static float[] CarVanished(int rate)
        {
            var b = new MixBuffer(rate * 6 / 10, rate, false);
            var fall = VoiceSpec.Tone(Waveform.Triangle, SadStartHz, SadSeconds, 0.7f).Gliding(SadEndHz);
            b.AddVoice(0f, fall.Blend(Waveform.Square, 0.2f));
            return SfxActions.Finish(b);
        }

        public static float[] CarArrives(int rate)
        {
            var b = new MixBuffer(rate / 2, rate, false);
            for (int i = 0; i < 2; i++)
            {
                var honk = VoiceSpec.Tone(Waveform.Saw, HonkHz, HonkSeconds, 0.5f)
                    .Blend(Waveform.Square, 0.5f).Filtered(HonkFilterHz, HonkFilterHz, 1f);
                b.AddVoice(i * HonkRepeat, honk);
            }
            return SfxActions.Finish(b);
        }

        public static float[] NightEnded(int rate)
        {
            var b = new MixBuffer(rate, rate, false);
            for (int i = 0; i < JingleMidi.Length; i++)
            {
                float length = i == JingleMidi.Length - 1 ? JingleLastSeconds : JingleNoteSeconds;
                var note = VoiceSpec.Tone(Waveform.Triangle, Pitch.MidiToHz(JingleMidi[i]), length, 0.6f)
                    .Blend(Waveform.Square, 0.3f);
                b.AddVoice(i * JingleStep, note);
            }
            return SfxActions.Finish(b);
        }

        /// <summary>A short, quiet rubber-ball "boing" for a car or the valet bumping a pedestrian.</summary>
        public static float[] PedestrianBump(int rate)
        {
            var b = new MixBuffer(rate * 25 / 100, rate, false);
            var boing = VoiceSpec.Tone(Waveform.Sine, BumpStartHz, BumpSeconds, BumpGain).Gliding(BumpEndHz)
                .WithEnvelope(new Adsr(0.003f, BumpSeconds, 0f, 0.05f));
            b.AddVoice(0f, boing);
            return SfxActions.Finish(b);
        }

        /// <summary>A short, soft rubbery "thud" for two cars bumping: a falling low sine plus a muffled knock.</summary>
        public static float[] CarBump(int rate)
        {
            var b = new MixBuffer(rate * 25 / 100, rate, false);
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Sine, ThudStartHz, ThudSeconds, ThudGain).Gliding(ThudEndHz)
                .WithEnvelope(new Adsr(0.002f, ThudSeconds, 0f, 0.06f)));
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Square, KnockHz, KnockSeconds, KnockGain).Filtered(KnockFilterHz, KnockFilterHz, 1f)
                .WithEnvelope(new Adsr(0.001f, KnockSeconds, 0f, 0.02f)));
            return SfxActions.Finish(b);
        }

        public static float[] ButtonClick(int rate)
        {
            var b = new MixBuffer(rate * 15 / 100, rate, false);
            DrumSynth.AddNoiseBurst(b, 0, ClickSeconds, 0.4f, ClickHighPassHz, 17u);
            b.AddVoice(0f, VoiceSpec.Tone(Waveform.Sine, ClickToneHz, ClickSeconds, 0.5f));
            return SfxActions.Finish(b);
        }
    }
}
