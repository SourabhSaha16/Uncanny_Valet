using UncannyValet.Core.Music;

namespace UncannyValet.Audio
{
    /// <summary>Fiddle-like bowed saw with a gentle vibrato, carrying the tune an octave up.</summary>
    public sealed class FiddlePart : IThemePart
    {
        private const float Gain = 0.3f;
        private const float HoldShare = 0.95f;
        private const float VibratoDepth = 0.006f;
        private const float VibratoRate = 5.5f;
        private const float CutoffHz = 2200f;

        public void Render(MixBuffer buffer, in ThemeContext context, float volume)
        {
            for (int step = 0; step < MusicThemeDefinition.Steps; step++)
            {
                if (!context.HasNote(step)) continue;
                float seconds = context.NoteSteps(step) * context.StepSeconds * HoldShare;
                var voice = VoiceSpec.Tone(Waveform.Saw, context.NoteHz(step, 1), seconds, Gain * volume)
                    .Blend(Waveform.Triangle, 0.3f).WithEnvelope(new Adsr(0.06f, 0.1f, 0.8f, 0.12f))
                    .Filtered(CutoffHz, CutoffHz, 1f).WithVibrato(VibratoDepth, VibratoRate);
                VoiceRenderer.Render(buffer, context.StepStart(step), voice);
            }
        }
    }
}
