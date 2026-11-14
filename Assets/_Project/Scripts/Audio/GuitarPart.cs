using UncannyValet.Core.Music;

namespace UncannyValet.Audio
{
    /// <summary>Plucked folk guitar: the tune in single plucks and a soft strum on each chord.</summary>
    public sealed class GuitarPart : IThemePart
    {
        private const float NoteGain = 0.5f;
        private const float StrumGain = 0.2f;
        private const float MaxNoteSeconds = 0.6f;
        private const float StrumGap = 0.014f;
        private const float StrumSeconds = 0.35f;
        private const int StrumOctave = -1;

        public void Render(MixBuffer buffer, in ThemeContext context, float volume)
        {
            for (int step = 0; step < MusicThemeDefinition.Steps; step++)
            {
                int start = context.StepStart(step);
                if (context.HasNote(step))
                {
                    float seconds = System.Math.Min(context.NoteSteps(step) * context.StepSeconds, MaxNoteSeconds);
                    VoiceRenderer.Render(buffer, start, Pluck(context.NoteHz(step, 0), seconds, NoteGain * volume));
                }
                if (step % ThemeContext.StepsPerChord == 0) Strum(buffer, context, step, start, volume);
            }
        }

        private static void Strum(MixBuffer buffer, in ThemeContext context, int step, int start, float volume)
        {
            for (int tone = 0; tone < 3; tone++)
            {
                int at = start + (int)(tone * StrumGap * buffer.SampleRate);
                VoiceRenderer.Render(buffer, at, Pluck(context.ChordHz(step, tone, StrumOctave), StrumSeconds, StrumGain * volume));
            }
        }

        private static VoiceSpec Pluck(float hz, float seconds, float gain)
            => VoiceSpec.Tone(Waveform.Triangle, hz, seconds, gain).Blend(Waveform.Saw, 0.35f)
                .WithEnvelope(new Adsr(0.002f, 0.2f, 0.1f, 0.12f)).Filtered(3200f, 700f, 0.12f);
    }
}
