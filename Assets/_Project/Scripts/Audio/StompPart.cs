using UncannyValet.Core.Music;

namespace UncannyValet.Audio
{
    /// <summary>Boot stomps: a heavy thud on every half bar and a lighter one on the offbeat before it.</summary>
    public sealed class StompPart : IThemePart
    {
        private const int StompEvery = 8;
        private const int HalfOffset = 4;
        private const float HeavyGain = 0.8f;
        private const float LightGain = 0.4f;
        private const float ScuffSeconds = 0.05f;
        private const float ScuffHighPassHz = 300f;
        private const float ScuffGain = 0.25f;
        private const uint Seed = 433u;

        public void Render(MixBuffer buffer, in ThemeContext context, float volume)
        {
            for (int step = 0; step < MusicThemeDefinition.Steps; step += HalfOffset)
            {
                int start = context.StepStart(step);
                float gain = step % StompEvery == 0 ? HeavyGain : LightGain;
                DrumSynth.AddKick(buffer, start, gain * volume);
                DrumSynth.AddNoiseBurst(buffer, start, ScuffSeconds, ScuffGain * gain * volume, ScuffHighPassHz, Seed + (uint)step);
            }
        }
    }
}
