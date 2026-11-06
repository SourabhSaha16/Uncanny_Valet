using System;

namespace UncannyValet.Audio
{
    /// <summary>Drum and noise-burst voices built from the synth maths.</summary>
    public static class DrumSynth
    {
        private const float KickStartHz = 130f;
        private const float KickEndHz = 45f;
        private const float KickSeconds = 0.16f;
        private const float SnareToneHz = 190f;
        private const float SnareToneSeconds = 0.1f;
        private const float SnareToneShare = 0.5f;
        private const float SnareNoiseSeconds = 0.14f;
        private const float SnareNoiseHighPassHz = 900f;
        private const float HatSeconds = 0.045f;
        private const float HatHighPassHz = 6000f;
        private const float BurstDecayRate = 5f;
        private const float BurstAttackSeconds = 0.001f;
        private const uint SnareSeed = 7u;

        public static void AddKick(MixBuffer buffer, int start, float gain)
        {
            var env = new Adsr(0.002f, KickSeconds, 0f, 0.02f);
            var kick = VoiceSpec.Tone(Waveform.Sine, KickStartHz, KickSeconds, gain)
                .Gliding(KickEndHz).WithEnvelope(env);
            VoiceRenderer.Render(buffer, start, kick);
        }

        public static void AddSnare(MixBuffer buffer, int start, float gain)
        {
            var env = new Adsr(0.002f, SnareToneSeconds, 0f, 0.02f);
            var tone = VoiceSpec.Tone(Waveform.Triangle, SnareToneHz, SnareToneSeconds, gain * SnareToneShare)
                .WithEnvelope(env);
            VoiceRenderer.Render(buffer, start, tone);
            AddNoiseBurst(buffer, start, SnareNoiseSeconds, gain, SnareNoiseHighPassHz, SnareSeed);
        }

        public static void AddHat(MixBuffer buffer, int start, float gain, uint seed)
        {
            AddNoiseBurst(buffer, start, HatSeconds, gain, HatHighPassHz, seed);
        }

        /// <summary>High-passed white noise with a quick attack and an exponential decay.</summary>
        public static void AddNoiseBurst(MixBuffer buffer, int start, float seconds, float gain,
            float highPassHz, uint seed)
        {
            int rate = buffer.SampleRate;
            int count = (int)(seconds * rate);
            int attack = Math.Max(1, (int)(BurstAttackSeconds * rate));
            float coefficient = OnePoleLowPass.Coefficient(highPassHz, rate);
            var noise = new NoiseSource(seed);
            var lowPass = new OnePoleLowPass();
            for (int i = 0; i < count; i++)
            {
                float raw = noise.Next();
                float high = raw - lowPass.Process(raw, coefficient);
                float env = (float)Math.Exp(-BurstDecayRate * i / count) * Math.Min(1f, (float)i / attack);
                buffer.Add(start + i, high * env * gain);
            }
        }
    }
}
