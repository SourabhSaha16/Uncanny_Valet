using System;

namespace UncannyValet.Audio
{
    /// <summary>Renders a <see cref="VoiceSpec"/> into a <see cref="MixBuffer"/>.</summary>
    public static class VoiceRenderer
    {
        /// <summary>Adds the note to the buffer starting at <paramref name="startSample"/>.</summary>
        public static void Render(MixBuffer buffer, int startSample, in VoiceSpec spec)
        {
            int rate = buffer.SampleRate;
            int count = (int)(spec.Envelope.TotalTime(spec.Duration) * rate);
            double glide = Math.Log(spec.EndFrequency / spec.Frequency);
            bool gliding = glide != 0.0;
            bool sweeping = spec.CutoffStart != spec.CutoffEnd;
            float fixedCoefficient = OnePoleLowPass.Coefficient(spec.CutoffStart, rate);
            var filter = new OnePoleLowPass();
            double phase = 0.0;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / rate;
                double hz = spec.Frequency;
                if (gliding) hz *= Math.Exp(glide * Math.Min(t / spec.Duration, 1.0));
                phase += hz / rate;
                float v = Oscillator.Sample(spec.WaveA, phase) * (1f - spec.MixB)
                        + Oscillator.Sample(spec.WaveB, phase) * spec.MixB;
                if (spec.CutoffStart > 0f)
                    v = filter.Process(v, sweeping ? SweptCoefficient(spec, t, rate) : fixedCoefficient);
                buffer.Add(startSample + i, v * spec.Envelope.Evaluate(t, spec.Duration) * spec.Gain);
            }
        }

        private static float SweptCoefficient(in VoiceSpec spec, float t, int rate)
        {
            float sweep = (float)Math.Exp(-t / spec.CutoffDecay);
            float cutoff = spec.CutoffEnd + (spec.CutoffStart - spec.CutoffEnd) * sweep;
            return OnePoleLowPass.Coefficient(cutoff, rate);
        }
    }
}
