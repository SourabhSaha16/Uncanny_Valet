using UncannyValet.Core.Music;

namespace UncannyValet.Audio
{
    /// <summary>Shaker on every eighth note, a little stronger on the beat.</summary>
    public sealed class ShakerPart : IThemePart
    {
        private const float BeatGain = 0.28f;
        private const float OffGain = 0.16f;
        private const float Seconds = 0.045f;
        private const float HighPassHz = 5000f;
        private const uint SeedBase = 401u;

        public void Render(MixBuffer buffer, in ThemeContext context, float volume)
        {
            for (int step = 0; step < MusicThemeDefinition.Steps; step += 2)
            {
                float gain = ThemeContext.OnBeat(step) ? BeatGain : OffGain;
                DrumSynth.AddNoiseBurst(buffer, context.StepStart(step), Seconds, gain * volume, HighPassHz, SeedBase + (uint)step);
            }
        }
    }
}
