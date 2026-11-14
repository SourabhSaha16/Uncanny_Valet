using UncannyValet.Core.Music;

namespace UncannyValet.Audio
{
    /// <summary>Fairground organ: the tune in reedy square tones an octave up over oom-pah chords.</summary>
    public sealed class OrganPart : IThemePart
    {
        private const float TuneGain = 0.28f;
        private const float OomGain = 0.3f;
        private const float PahGain = 0.14f;
        private const float HoldShare = 0.92f;
        private const float VibratoDepth = 0.004f;
        private const float VibratoRate = 6f;
        private const float CutoffHz = 2800f;
        private const float PahSeconds = 0.12f;
        private const int OomOctave = -1;

        public void Render(MixBuffer buffer, in ThemeContext context, float volume)
        {
            for (int step = 0; step < MusicThemeDefinition.Steps; step++)
            {
                int start = context.StepStart(step);
                if (context.HasNote(step))
                {
                    float seconds = context.NoteSteps(step) * context.StepSeconds * HoldShare;
                    VoiceRenderer.Render(buffer, start, Reed(context.NoteHz(step, 1), seconds, TuneGain * volume)
                        .WithVibrato(VibratoDepth, VibratoRate));
                }
                if (step % ThemeContext.StepsPerChord == 0)
                    VoiceRenderer.Render(buffer, start, Reed(context.ChordHz(step, 0, OomOctave), context.StepSeconds * 3f, OomGain * volume));
                else if (step % ThemeContext.StepsPerChord == 4) Pah(buffer, context, step, start, volume);
            }
        }

        private static void Pah(MixBuffer buffer, in ThemeContext context, int step, int start, float volume)
        {
            for (int tone = 0; tone < 3; tone++)
                VoiceRenderer.Render(buffer, start, Reed(context.ChordHz(step, tone, 0), PahSeconds, PahGain * volume));
        }

        private static VoiceSpec Reed(float hz, float seconds, float gain)
            => VoiceSpec.Tone(Waveform.Square, hz, seconds, gain).Blend(Waveform.Saw, 0.3f)
                .WithEnvelope(new Adsr(0.012f, 0.05f, 0.85f, 0.05f)).Filtered(CutoffHz, CutoffHz, 1f);
    }
}
