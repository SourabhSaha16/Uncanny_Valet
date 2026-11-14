using UncannyValet.Core.Music;

namespace UncannyValet.Core.Contracts
{
    /// <summary>The music and ambience of the lot being played. Registered at lot start; Audio reads it. Either part may be null: no theme keeps the funk loop.</summary>
    public interface ILotAudio
    {
        /// <summary>The lot's music theme, or null for the funk loop.</summary>
        MusicThemeDefinition Theme { get; }

        /// <summary>The lot's ambience set, or null for none.</summary>
        AmbienceSetDefinition Ambience { get; }
    }
}
