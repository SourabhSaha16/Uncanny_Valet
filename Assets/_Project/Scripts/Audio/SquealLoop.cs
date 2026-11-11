using System;

namespace UncannyValet.Audio
{
    /// <summary>
    /// The sustained tyre squeal, built to loop with no click: every tone has a whole number of cycles in the loop, the
    /// wobble repeats each loop, and the hiss is smoothed around the wrap point. Volume and pitch come from <see cref="SquealMix"/>.
    /// </summary>
    public static class SquealLoop
    {
        /// <summary>Length of the loop in seconds. Every frequency below is a whole multiple of 1 / LoopSeconds.</summary>
        public const float LoopSeconds = 0.4f;

        private const int MainCycles = 400;      // 1000 Hz
        private const int BeatCycles = 601;      // 1502.5 Hz, a little off 3/2 of the main tone so the squeal shimmers
        private const int UpperCycles = 802;     // 2005 Hz
        private const float BeatGain = 0.5f;
        private const float UpperGain = 0.25f;
        private const int WobbleCycles = 2;      // 5 Hz vibrato
        private const double WobbleDepth = 0.35; // in cycles of the main tone
        private const float HissGain = 0.35f;
        private const int HissSmoothing = 3;
        private const float NormalizedPeak = 0.9f;
        private const uint Seed = 29u;

        /// <summary>Renders the loop to mono samples in [-1, 1].</summary>
        public static float[] Build(int rate)
        {
            int n = (int)(rate * LoopSeconds);
            var tone = new float[n];
            for (int i = 0; i < n; i++) tone[i] = Tone((double)i / n);
            var hiss = Hiss(n);
            var buffer = new MixBuffer(n, rate, true);
            for (int i = 0; i < n; i++) buffer.Add(i, tone[i] + hiss[i] * HissGain);
            buffer.NormalizeTo(NormalizedPeak);
            return buffer.Samples;
        }

        // x is the position in the loop, 0..1.
        private static float Tone(double x)
        {
            double wobble = WobbleDepth * Math.Sin(2.0 * Math.PI * WobbleCycles * x);
            double main = Math.Sin(2.0 * Math.PI * (MainCycles * x + wobble));
            double beat = Math.Sin(2.0 * Math.PI * (BeatCycles * x + wobble));
            double upper = Math.Sin(2.0 * Math.PI * (UpperCycles * x + wobble));
            return (float)(main + beat * BeatGain + upper * UpperGain);
        }

        // High-passed white noise: the raw noise minus a moving average that wraps around the loop, so the end joins the start.
        private static float[] Hiss(int n)
        {
            var noise = new NoiseSource(Seed);
            var raw = new float[n];
            for (int i = 0; i < n; i++) raw[i] = noise.Next();
            var hiss = new float[n];
            for (int i = 0; i < n; i++)
            {
                float sum = 0f;
                for (int j = -HissSmoothing; j <= HissSmoothing; j++) sum += raw[((i + j) % n + n) % n];
                hiss[i] = raw[i] - sum / (2 * HissSmoothing + 1);
            }
            return hiss;
        }
    }
}
