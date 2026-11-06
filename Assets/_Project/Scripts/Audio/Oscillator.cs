using System;

namespace UncannyValet.Audio
{
    /// <summary>Stateless oscillator shapes. Phase is in cycles and is wrapped to [0, 1) internally.</summary>
    public static class Oscillator
    {
        private const double TwoPi = Math.PI * 2.0;

        /// <summary>One sample of the wave at the given phase, in [-1, 1].</summary>
        public static float Sample(Waveform wave, double phase)
        {
            double p = phase - Math.Floor(phase);
            switch (wave)
            {
                case Waveform.Square: return p < 0.5 ? 1f : -1f;
                case Waveform.Saw: return (float)(2.0 * p - 1.0);
                case Waveform.Triangle: return (float)(1.0 - 4.0 * Math.Abs(p - 0.5));
                default: return (float)Math.Sin(TwoPi * p);
            }
        }
    }
}
