using System;

namespace UncannyValet.Audio
{
    /// <summary>Mono float buffer that voices are summed into. A looping buffer wraps writes past the end.</summary>
    public sealed class MixBuffer
    {
        public readonly float[] Samples;
        public readonly int SampleRate;
        private readonly bool _wrap;

        public MixBuffer(int length, int sampleRate, bool wrap)
        {
            Samples = new float[length];
            SampleRate = sampleRate;
            _wrap = wrap;
        }

        /// <summary>Adds a value at an index. Looping buffers wrap; one-shot buffers drop what runs past the end.</summary>
        public void Add(int index, float value)
        {
            if (_wrap) index %= Samples.Length;
            else if (index < 0 || index >= Samples.Length) return;
            Samples[index] += value;
        }

        /// <summary>Renders a note into the buffer, starting at a time in seconds.</summary>
        public void AddVoice(float atSeconds, in VoiceSpec spec) =>
            VoiceRenderer.Render(this, (int)(atSeconds * SampleRate), spec);

        /// <summary>Scales the whole buffer so its loudest sample has the given absolute level.</summary>
        public void NormalizeTo(float peak)
        {
            float max = 0f;
            for (int i = 0; i < Samples.Length; i++) max = Math.Max(max, Math.Abs(Samples[i]));
            if (max <= 0f) return;
            float gain = peak / max;
            for (int i = 0; i < Samples.Length; i++) Samples[i] *= gain;
        }
    }
}
