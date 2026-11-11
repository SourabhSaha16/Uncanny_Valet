using System;

namespace UncannyValet.Audio
{
    /// <summary>Rounds off a rendered sound with a gentle low-pass, and maps the 0..1 sharpness knob to its cutoff.</summary>
    public static class SoftenFilter
    {
        /// <summary>Cutoff at sharpness 0 (dull), Hz.</summary>
        public const float MinCutoffHz = 500f;
        /// <summary>Cutoff at sharpness 1 (bright), Hz.</summary>
        public const float MaxCutoffHz = 4000f;

        /// <summary>Cutoff for a sharpness in 0..1.</summary>
        public static float CutoffFor(float sharpness)
        {
            var t = Math.Min(Math.Max(sharpness, 0f), 1f);
            return MinCutoffHz + (MaxCutoffHz - MinCutoffHz) * t;
        }

        /// <summary>
        /// Low-passes the samples in place. A <paramref name="circular"/> buffer (a loop) is run once first to settle the
        /// filter, so the end joins the start without a step.
        /// </summary>
        public static void LowPass(float[] samples, int rate, float cutoffHz, bool circular = false)
        {
            var coefficient = OnePoleLowPass.Coefficient(cutoffHz, rate);
            var filter = new OnePoleLowPass();
            if (circular)
                for (var i = 0; i < samples.Length; i++) filter.Process(samples[i], coefficient);
            for (var i = 0; i < samples.Length; i++) samples[i] = filter.Process(samples[i], coefficient);
        }
    }
}
