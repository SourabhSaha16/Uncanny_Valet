using System;
using UncannyValet.Core.Music;

namespace UncannyValet.Audio
{
    /// <summary>Renders the loops of the last-30-seconds layers: fireworks and thunder rolling closer.</summary>
    public static class EscalationLoops
    {
        private const float Peak = 0.8f;
        private const float FireworksSeconds = 8f;
        private const float ThunderSeconds = 12f;
        private const float WhistleSeconds = 0.5f;
        private const float BurstDelay = 0.55f;
        private const int CracklesPerBurst = 8;
        private const float CrackleStep = 0.07f;
        private static readonly float[] LaunchAt = { 0.4f, 2.3f, 4.5f, 6.2f };
        private static readonly float[] WhistleStartHz = { 700f, 900f, 650f, 800f };
        private static readonly float[] CrackStarts = { 0.08f, 0.38f, 0.7f };
        private static readonly float[] CrackLengths = { 0.2f, 0.25f, 0.28f };

        /// <summary>The loop of a layer, or null for <see cref="EscalationLayer.None"/>.</summary>
        public static float[] Build(EscalationLayer layer, int sampleRate)
        {
            switch (layer)
            {
                case EscalationLayer.Fireworks: return Fireworks(sampleRate);
                case EscalationLayer.ThunderCloser: return ThunderCloser(sampleRate);
                default: return null;
            }
        }

        private static float[] Fireworks(int rate)
        {
            var buffer = new MixBuffer((int)(FireworksSeconds * rate), rate, true);
            for (int i = 0; i < LaunchAt.Length; i++)
            {
                var whistle = VoiceSpec.Tone(Waveform.Sine, WhistleStartHz[i], WhistleSeconds, 0.2f).Gliding(WhistleStartHz[i] * 3f)
                    .WithEnvelope(new Adsr(0.01f, WhistleSeconds * 0.9f, 0f, 0.05f));
                buffer.AddVoice(LaunchAt[i], whistle);
                AddBurst(buffer, rate, LaunchAt[i] + BurstDelay, (uint)(601 + i * 17));
            }
            return Finish(buffer);
        }

        private static void AddBurst(MixBuffer buffer, int rate, float at, uint seed)
        {
            int start = (int)(at * rate);
            DrumSynth.AddNoiseBurst(buffer, start, 0.5f, 0.5f, 2000f, seed);
            buffer.AddVoice(at, VoiceSpec.Tone(Waveform.Sine, 90f, 0.15f, 0.6f).Gliding(50f).WithEnvelope(new Adsr(0.002f, 0.15f, 0f, 0.02f)));
            for (int k = 0; k < CracklesPerBurst; k++)
            {
                int offset = (int)((0.1f + k * CrackleStep + (k * 7 % 3) * 0.015f) * rate);
                DrumSynth.AddNoiseBurst(buffer, start + offset, 0.015f, 0.3f, 5000f, seed + 1u + (uint)k);
            }
        }

        private static float[] ThunderCloser(int rate)
        {
            var rumble = SeamlessNoise.Band(rate, ThunderSeconds, 35f, 260f, 631u);
            for (int i = 0; i < rumble.Length; i++)
                rumble[i] *= 0.2f + AmbienceStorm.Swells(i / (float)rumble.Length, CrackStarts, CrackLengths);
            var buffer = new MixBuffer(rumble.Length, rate, true);
            for (int i = 0; i < rumble.Length; i++) buffer.Add(i, rumble[i]);
            for (int i = 0; i < CrackStarts.Length; i++)
            {
                float peak = (CrackStarts[i] + CrackLengths[i] * 0.5f) * ThunderSeconds;
                DrumSynth.AddNoiseBurst(buffer, (int)(peak * rate), 0.3f, 0.5f, 700f, (uint)(641 + i));
            }
            return Finish(buffer);
        }

        private static float[] Finish(MixBuffer buffer)
        {
            buffer.NormalizeTo(Peak);
            return buffer.Samples;
        }
    }
}
