using System;

namespace UncannyValet.Audio
{
    /// <summary>Band-limited noise that loops without a click: the tail is faded into the head (equal power).</summary>
    public static class SeamlessNoise
    {
        private const float FadeSeconds = 0.3f;

        /// <summary>Noise between <paramref name="lowHz"/> and <paramref name="highHz"/>, exactly <paramref name="seconds"/> long.</summary>
        public static float[] Band(int sampleRate, float seconds, float lowHz, float highHz, uint seed)
        {
            int length = (int)(seconds * sampleRate);
            int fade = (int)(FadeSeconds * sampleRate);
            var raw = new float[length + fade];
            var noise = new NoiseSource(seed);
            var top = new OnePoleLowPass();
            var bottom = new OnePoleLowPass();
            float topCoefficient = OnePoleLowPass.Coefficient(highHz, sampleRate);
            float bottomCoefficient = OnePoleLowPass.Coefficient(lowHz, sampleRate);
            for (int i = 0; i < raw.Length; i++)
            {
                float smooth = top.Process(noise.Next(), topCoefficient);
                raw[i] = smooth - bottom.Process(smooth, bottomCoefficient);
            }
            var loop = new float[length];
            Array.Copy(raw, loop, length);
            for (int i = 0; i < fade; i++)
            {
                float t = (float)i / fade;
                loop[i] = raw[i] * (float)Math.Sin(t * Math.PI * 0.5) + raw[length + i] * (float)Math.Cos(t * Math.PI * 0.5);
            }
            return loop;
        }

        /// <summary>Multiplies the loop by a slow swell of <paramref name="cycles"/> whole cycles (so the loop still joins up): level swings between 1 - depth and 1.</summary>
        public static void Swell(float[] loop, float cycles, float depth, float phase)
        {
            for (int i = 0; i < loop.Length; i++)
            {
                float wave = 0.5f + 0.5f * (float)Math.Sin(2.0 * Math.PI * (cycles * i / loop.Length + phase));
                loop[i] *= 1f - depth + depth * wave;
            }
        }
    }
}
