using UncannyValet.Core.Music;

namespace UncannyValet.Audio
{
    /// <summary>Soft frame drum: a low thump on every half bar and a lighter tap before the next.</summary>
    public sealed class FrameDrumPart : IThemePart
    {
        private const int ThumpEvery = 8;
        private const int TapOffset = 4;
        private const float ThumpGain = 0.7f;
        private const float TapGain = 0.3f;
        private const float StartHz = 120f;
        private const float EndHz = 70f;
        private const float Seconds = 0.2f;

        public void Render(MixBuffer buffer, in ThemeContext context, float volume)
        {
            for (int step = 0; step < MusicThemeDefinition.Steps; step += TapOffset)
            {
                float gain = step % ThumpEvery == 0 ? ThumpGain : TapGain;
                var voice = VoiceSpec.Tone(Waveform.Sine, StartHz, Seconds, gain * volume).Gliding(EndHz)
                    .WithEnvelope(new Adsr(0.003f, Seconds, 0f, 0.03f));
                VoiceRenderer.Render(buffer, context.StepStart(step), voice);
            }
        }
    }
}
