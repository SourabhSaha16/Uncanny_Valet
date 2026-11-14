using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Music;
using UncannyValet.Run;

namespace UncannyValet.Bootstrap
{
    /// <summary>Hands the lot's music theme and ambience set to the Audio module through <see cref="ILotAudio"/>; call before the audio director starts.</summary>
    public static class AudioBootstrap
    {
        /// <summary>Registers <see cref="ILotAudio"/> for the lot.</summary>
        public static void Register(LotDefinition lot) => Services.Register<ILotAudio>(new LotAudioSelection(lot.MusicTheme, lot.Ambience));

        private sealed class LotAudioSelection : ILotAudio
        {
            public LotAudioSelection(MusicThemeDefinition theme, AmbienceSetDefinition ambience)
            {
                Theme = theme;
                Ambience = ambience;
            }

            public MusicThemeDefinition Theme { get; }
            public AmbienceSetDefinition Ambience { get; }
        }
    }
}
