namespace UncannyValet.Core.Modifiers
{
    /// <summary>How a modifier shows on the car sprite. Append-only (stored in definition assets).</summary>
    public enum ModifierLook
    {
        /// <summary>The car looks the same.</summary>
        None = 0,

        /// <summary>A mud overlay that wipes away as the modifier clears.</summary>
        MudOverlay = 1,

        /// <summary>The car body takes the electric-car tint.</summary>
        ElectricBody = 2
    }
}
