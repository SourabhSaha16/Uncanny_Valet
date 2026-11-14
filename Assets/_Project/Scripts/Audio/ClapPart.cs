using UncannyValet.Core.Music;

namespace UncannyValet.Audio
{
    /// <summary>Hand claps on the backbeat: two quick slaps of noise.</summary>
    public sealed class ClapPart : IThemePart
    {
        private const int FirstStep = 4;
        private const int Every = 8;
        private const float Gain = 0.3f;
        private const float SlapSeconds = 0.05f;
        private const float SlapGapSeconds = 0.011f;
        private const float HighPassHz = 1500f;
        private const uint Seed = 457u;

        public void Render(MixBuffer buffer, in ThemeContext context, float volume)
        {
            int gap = (int)(SlapGapSeconds * buffer.SampleRate);
            for (int step = FirstStep; step < MusicThemeDefinition.Steps; step += Every)
            {
                int start = context.StepStart(step);
                DrumSynth.AddNoiseBurst(buffer, start, SlapSeconds, Gain * volume, HighPassHz, Seed + (uint)step);
                DrumSynth.AddNoiseBurst(buffer, start + gap, SlapSeconds, Gain * volume, HighPassHz, Seed + 7u + (uint)step);
            }
        }
    }
}
