namespace UncannyValet.Core.Music
{
    /// <summary>The extra layer a theme adds for the last 30 seconds. Stored as integers in assets: append-only.</summary>
    public enum EscalationLayer
    {
        /// <summary>No extra layer (only the tempo rises).</summary>
        None = 0,
        /// <summary>Whistling rockets and crackling bursts (Midgard).</summary>
        Fireworks = 1,
        /// <summary>Thunder that rolls closer and grows (Vanaheim).</summary>
        ThunderCloser = 2,
    }
}
