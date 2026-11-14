using UncannyValet.Core.Music;

namespace UncannyValet.Audio
{
    /// <summary>Brass: a chord stab at every chord change.</summary>
    public sealed class BrassPart : IThemePart
    {
        private const float Gain = 0.18f;
        private const float Seconds = 0.22f;
        private const float OpenHz = 2800f;
        private const float ClosedHz = 900f;
        private const float SweepSeconds = 0.12f;

        public void Render(MixBuffer buffer, in ThemeContext context, float volume)
        {
            for (int step = 0; step < MusicThemeDefinition.Steps; step += ThemeContext.StepsPerChord)
            {
                for (int tone = 0; tone < 3; tone++)
                {
                    var voice = VoiceSpec.Tone(Waveform.Saw, context.ChordHz(step, tone, 0), Seconds, Gain * volume)
                        .Blend(Waveform.Square, 0.3f).WithEnvelope(new Adsr(0.015f, 0.08f, 0.5f, 0.08f))
                        .Filtered(OpenHz, ClosedHz, SweepSeconds);
                    VoiceRenderer.Render(buffer, context.StepStart(step), voice);
                }
            }
        }
    }
}
