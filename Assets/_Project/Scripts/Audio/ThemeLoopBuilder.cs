using UncannyValet.Core.Music;

namespace UncannyValet.Audio
{
    /// <summary>Renders one seamless music loop (mono) from a <see cref="MusicThemeDefinition"/>: every instrument of the theme, mixed and normalised.</summary>
    public static class ThemeLoopBuilder
    {
        private const float NormalizedPeak = 0.9f;

        /// <summary>Samples in the loop of this theme at this rate (a whole number of beats).</summary>
        public static int LengthSamples(MusicThemeDefinition theme, int sampleRate)
            => new ThemeContext(theme, sampleRate).Settings.LengthSamples;

        /// <summary>Builds the loop. Note tails past the end wrap onto the start.</summary>
        public static float[] Build(MusicThemeDefinition theme, int sampleRate)
        {
            var context = new ThemeContext(theme, sampleRate);
            var buffer = new MixBuffer(context.Settings.LengthSamples, sampleRate, true);
            for (int i = 0; i < theme.Voices.Count; i++)
                ThemeParts.For(theme.Voices[i].Voice).Render(buffer, context, theme.Voices[i].Volume);
            buffer.NormalizeTo(NormalizedPeak);
            return buffer.Samples;
        }
    }
}
