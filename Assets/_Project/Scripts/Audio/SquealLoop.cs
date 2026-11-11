using System;

namespace UncannyValet.Audio
{
    /// <summary>
    /// The sustained tyre squeal, built to loop with no click: every tone has a whole number of cycles in the loop, the
    /// wobble repeats each loop, the grit is smoothed around the wrap point and the final low-pass runs circularly.
    /// The sharpness knob (0..1) scales the upper partial, the grit and the low-pass cutoff. Volume and pitch come from <see cref="SquealMix"/>.
    /// </summary>
    public static class SquealLoop
    {
        /// <summary>Length of the loop in seconds. Every frequency below is a whole multiple of 1 / LoopSeconds.</summary>
        public const float LoopSeconds = 0.4f;

        private const int MainCycles = 200;      // 500 Hz
        private const int BeatCycles = 301;      // 752.5 Hz, a little off 3/2 of the main tone so the squeal shimmers
        private const int UpperCycles = 400;     // 1000 Hz, only as sharpness rises
        private const float BeatGain = 0.4f;
        private const float UpperGain = 0.3f;
        private const int WobbleCycles = 2;      // 5 Hz vibrato
        private const double WobbleDepth = 0.25; // in cycles of the main tone
        private const float GritGain = 0.1f;     // noise, only as sharpness rises
        private const int GritSmoothing = 3;
        private const float NormalizedPeak = 0.9f;
        private const uint Seed = 29u;

        /// <summary>Renders the loop to mono samples in [-1, 1].</summary>
        public static float[] Build(int rate, float sharpness = SfxTyres.DefaultSharpness)
        {
            var s = Math.Min(Math.Max(sharpness, 0f), 1f);
            int n = (int)(rate * LoopSeconds);
            var grit = Grit(n);
            var buffer = new MixBuffer(n, rate, true);
            for (int i = 0; i < n; i++) buffer.Add(i, Tone((double)i / n, s) + grit[i] * GritGain * s);
            SoftenFilter.LowPass(buffer.Samples, rate, SoftenFilter.CutoffFor(s), true);
            buffer.NormalizeTo(NormalizedPeak);
            return buffer.Samples;
        }

        // x is the position in the loop, 0..1.
        private static float Tone(double x, float sharpness)
        {
            double wobble = WobbleDepth * Math.Sin(2.0 * Math.PI * WobbleCycles * x);
            double main = Math.Sin(2.0 * Math.PI * (MainCycles * x + wobble));
            double beat = Math.Sin(2.0 * Math.PI * (BeatCycles * x + wobble));
            double upper = Math.Sin(2.0 * Math.PI * (UpperCycles * x + wobble));
            return (float)(main + beat * BeatGain + upper * UpperGain * sharpness);
        }

        // High-passed white noise: the raw noise minus a moving average that wraps around the loop, so the end joins the start.
        private static float[] Grit(int n)
        {
            var noise = new NoiseSource(Seed);
            var raw = new float[n];
            for (int i = 0; i < n; i++) raw[i] = noise.Next();
            var grit = new float[n];
            for (int i = 0; i < n; i++)
            {
                float sum = 0f;
                for (int j = -GritSmoothing; j <= GritSmoothing; j++) sum += raw[((i + j) % n + n) % n];
                grit[i] = raw[i] - sum / (2 * GritSmoothing + 1);
            }
            return grit;
        }
    }
}
