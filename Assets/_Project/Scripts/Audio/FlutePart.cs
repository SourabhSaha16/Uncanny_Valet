using UncannyValet.Core.Music;

namespace UncannyValet.Audio
{
    /// <summary>Wooden flute: doubles the tune two octaves above the root in the second bar, with a soft breathy sine.</summary>
    public sealed class FlutePart : IThemePart
    {
        private const float Gain = 0.25f;
        private const float HoldShare = 0.9f;
        private const int Octave = 2;
        private const int FirstStep = MusicThemeDefinition.Steps / 2;
        private const float VibratoDepth = 0.004f;
        private const float VibratoRate = 5f;

        public void Render(MixBuffer buffer, in ThemeContext context, float volume)
        {
            for (int step = FirstStep; step < MusicThemeDefinition.Steps; step++)
            {
                if (!context.HasNote(step)) continue;
                float seconds = context.NoteSteps(step) * context.StepSeconds * HoldShare;
                var voice = VoiceSpec.Tone(Waveform.Sine, context.NoteHz(step, Octave), seconds, Gain * volume)
                    .Blend(Waveform.Triangle, 0.2f).WithEnvelope(new Adsr(0.05f, 0.1f, 0.7f, 0.1f))
                    .WithVibrato(VibratoDepth, VibratoRate);
                VoiceRenderer.Render(buffer, context.StepStart(step), voice);
            }
        }
    }
}
