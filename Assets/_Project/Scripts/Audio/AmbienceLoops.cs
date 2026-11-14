using System;
using UncannyValet.Core.Music;

namespace UncannyValet.Audio
{
    /// <summary>Renders the looping ambience layers of an <see cref="AmbienceKind"/>.</summary>
    public static class AmbienceLoops
    {
        private const float Peak = 0.5f;

        /// <summary>True when the kind is a looping layer (the wash splash is a one-shot sound effect instead).</summary>
        public static bool IsLoop(AmbienceKind kind) => kind != AmbienceKind.WashSplash;

        /// <summary>The loop of an ambience layer, mono in [-1, 1].</summary>
        public static float[] Build(AmbienceKind kind, int sampleRate)
        {
            switch (kind)
            {
                case AmbienceKind.CrowdMurmur: return AmbienceFair.CrowdMurmur(sampleRate);
                case AmbienceKind.DistantRides: return AmbienceFair.DistantRides(sampleRate);
                case AmbienceKind.DrippingEaves: return AmbienceStorm.DrippingEaves(sampleRate);
                case AmbienceKind.DistantThunder: return AmbienceStorm.DistantThunder(sampleRate);
                case AmbienceKind.WaterwheelCreak: return AmbienceWheel.WaterwheelCreak(sampleRate);
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, "This ambience kind is not a loop.");
            }
        }

        internal static float[] Finish(MixBuffer buffer)
        {
            buffer.NormalizeTo(Peak);
            return buffer.Samples;
        }
    }
}
