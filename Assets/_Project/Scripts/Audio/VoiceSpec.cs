namespace UncannyValet.Audio
{
    /// <summary>
    /// Description of one synthesised note: one or two blended oscillators, an optional pitch glide, an optional vibrato
    /// and an optional decaying low-pass sweep. Build with <see cref="Tone"/> and the With/Blend helpers.
    /// </summary>
    public readonly struct VoiceSpec
    {
        private const float DefaultAttack = 0.004f;
        private const float DefaultDecay = 0.05f;
        private const float DefaultSustain = 0.6f;
        private const float DefaultRelease = 0.04f;

        public readonly float Frequency;
        public readonly float EndFrequency;
        public readonly float Duration;
        public readonly float Gain;
        public readonly Waveform WaveA;
        public readonly Waveform WaveB;
        public readonly float MixB;
        public readonly Adsr Envelope;
        /// <summary>Filter cutoff at the note start; 0 disables the filter.</summary>
        public readonly float CutoffStart;
        public readonly float CutoffEnd;
        public readonly float CutoffDecay;
        /// <summary>Vibrato depth as a fraction of the pitch (0.01 = one percent); 0 is none.</summary>
        public readonly float VibratoDepth;
        /// <summary>Vibrato speed in Hz.</summary>
        public readonly float VibratoRate;

        private VoiceSpec(float freq, float endFreq, float duration, float gain, Waveform a, Waveform b,
            float mixB, Adsr env, float cutStart, float cutEnd, float cutDecay, float vibDepth, float vibRate)
        {
            Frequency = freq; EndFrequency = endFreq; Duration = duration; Gain = gain;
            WaveA = a; WaveB = b; MixB = mixB; Envelope = env;
            CutoffStart = cutStart; CutoffEnd = cutEnd; CutoffDecay = cutDecay;
            VibratoDepth = vibDepth; VibratoRate = vibRate;
        }

        /// <summary>A plain single-oscillator note with a short plucky envelope and no filter.</summary>
        public static VoiceSpec Tone(Waveform wave, float frequency, float duration, float gain)
        {
            var env = new Adsr(DefaultAttack, DefaultDecay, DefaultSustain, DefaultRelease);
            return new VoiceSpec(frequency, frequency, duration, gain, wave, wave, 0f, env, 0f, 0f, 1f, 0f, 0f);
        }

        public VoiceSpec WithEnvelope(Adsr env) =>
            new VoiceSpec(Frequency, EndFrequency, Duration, Gain, WaveA, WaveB, MixB, env, CutoffStart, CutoffEnd, CutoffDecay, VibratoDepth, VibratoRate);

        /// <summary>Glide the pitch exponentially to <paramref name="endFrequency"/> over the held duration.</summary>
        public VoiceSpec Gliding(float endFrequency) =>
            new VoiceSpec(Frequency, endFrequency, Duration, Gain, WaveA, WaveB, MixB, Envelope, CutoffStart, CutoffEnd, CutoffDecay, VibratoDepth, VibratoRate);

        /// <summary>Mix a second oscillator in (0 = only the first, 1 = only the second).</summary>
        public VoiceSpec Blend(Waveform other, float mix) =>
            new VoiceSpec(Frequency, EndFrequency, Duration, Gain, WaveA, other, mix, Envelope, CutoffStart, CutoffEnd, CutoffDecay, VibratoDepth, VibratoRate);

        /// <summary>Low-pass the note, closing from <paramref name="start"/> Hz to <paramref name="end"/> Hz.</summary>
        public VoiceSpec Filtered(float start, float end, float decaySeconds) =>
            new VoiceSpec(Frequency, EndFrequency, Duration, Gain, WaveA, WaveB, MixB, Envelope, start, end, decaySeconds, VibratoDepth, VibratoRate);

        /// <summary>Wobble the pitch: <paramref name="depth"/> as a fraction of the pitch, <paramref name="rateHz"/> times a second.</summary>
        public VoiceSpec WithVibrato(float depth, float rateHz) =>
            new VoiceSpec(Frequency, EndFrequency, Duration, Gain, WaveA, WaveB, MixB, Envelope, CutoffStart, CutoffEnd, CutoffDecay, depth, rateHz);
    }
}
