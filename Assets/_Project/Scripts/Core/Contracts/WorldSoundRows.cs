using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows of the themed lot sounds (Audio owns the values).</summary>
    public static class WorldSoundRows
    {
        public const string Group = "Lot music and sounds";

        public static readonly TuningInfo[] Rows =
        {
            new TuningInfo(TuningKey.AmbienceVolume, Group, "Ambience volume", 0f, 1f, 0.05f,
                "Loudness of the background sounds of the lot (crowd, rides, drips, distant thunder, waterwheel)."),
            new TuningInfo(TuningKey.EscalationVolume, Group, "Last 30 seconds layer volume", 0f, 1f, 0.05f,
                "Loudness of the fireworks or rolling thunder that join in for the last 30 seconds."),
            new TuningInfo(TuningKey.WorldSoundVolume, Group, "Chime, splash and squelch volume", 0f, 1f, 0.05f,
                "Loudness of the blessing chime, the wash splash and the mud squelch."),
        };
    }
}
