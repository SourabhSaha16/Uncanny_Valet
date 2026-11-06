using System;

namespace UncannyValet.Audio
{
    /// <summary>One-pole low-pass filter. Subtract its output from the input to get a high-pass.</summary>
    public struct OnePoleLowPass
    {
        private float _state;

        /// <summary>Smoothing coefficient (0..1) for a cutoff frequency; 0 Hz blocks everything.</summary>
        public static float Coefficient(float cutoffHz, int sampleRate)
        {
            if (cutoffHz <= 0f) return 0f;
            return (float)(1.0 - Math.Exp(-2.0 * Math.PI * cutoffHz / sampleRate));
        }

        public float Process(float input, float coefficient)
        {
            _state += coefficient * (input - _state);
            return _state;
        }
    }
}
