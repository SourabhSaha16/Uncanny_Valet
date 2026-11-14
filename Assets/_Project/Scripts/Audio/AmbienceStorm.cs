using System;

namespace UncannyValet.Audio
{
    /// <summary>The wet-feast ambience loops: dripping eaves and thunder a long way off.</summary>
    public static class AmbienceStorm
    {
        private const float DripSeconds = 8f;
        private const float ThunderSeconds = 14f;
        private const float RainGain = 0.06f;
        private const float DripGain = 0.4f;
        private const float DripNoteSeconds = 0.05f;
        // Drip times as shares of the loop and pitches (Hz): uneven on purpose.
        private static readonly float[] DripAt = { 0.02f, 0.13f, 0.21f, 0.4f, 0.47f, 0.6f, 0.71f, 0.78f, 0.93f };
        private static readonly float[] DripHz = { 2100f, 2600f, 1900f, 2400f, 2200f, 1800f, 2700f, 2000f, 2300f };
        private static readonly float[] RumbleAt = { 0.1f, 0.58f };
        private static readonly float[] RumbleLength = { 0.28f, 0.34f };

        /// <summary>A faint rain hiss with single drips falling off the eaves.</summary>
        public static float[] DrippingEaves(int rate)
        {
            var buffer = new MixBuffer((int)(DripSeconds * rate), rate, true);
            var rain = SeamlessNoise.Band(rate, DripSeconds, 2500f, 8000f, 521u);
            for (int i = 0; i < rain.Length; i++) buffer.Add(i, rain[i] * RainGain);
            for (int i = 0; i < DripAt.Length; i++)
            {
                var drip = VoiceSpec.Tone(Waveform.Sine, DripHz[i], DripNoteSeconds, DripGain).Gliding(DripHz[i] * 0.55f)
                    .WithEnvelope(new Adsr(0.001f, DripNoteSeconds, 0f, 0.03f));
                buffer.AddVoice(DripAt[i] * DripSeconds, drip);
            }
            return AmbienceLoops.Finish(buffer);
        }

        /// <summary>Two low rumbles that swell and fade.</summary>
        public static float[] DistantThunder(int rate)
        {
            var rumble = SeamlessNoise.Band(rate, ThunderSeconds, 35f, 220f, 523u);
            for (int i = 0; i < rumble.Length; i++) rumble[i] *= Swells(i / (float)rumble.Length, RumbleAt, RumbleLength);
            var buffer = new MixBuffer(rumble.Length, rate, true);
            for (int i = 0; i < rumble.Length; i++) buffer.Add(i, rumble[i]);
            return AmbienceLoops.Finish(buffer);
        }

        /// <summary>Sum of sine-shaped swells; each starts at a share of the loop and lasts a share of it.</summary>
        internal static float Swells(float position, float[] starts, float[] lengths)
        {
            float level = 0f;
            for (int i = 0; i < starts.Length; i++)
            {
                float t = (position - starts[i]) / lengths[i];
                if (t > 0f && t < 1f) level += (float)Math.Sin(t * Math.PI);
            }
            return level;
        }
    }
}
