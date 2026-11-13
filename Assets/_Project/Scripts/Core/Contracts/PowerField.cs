namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// The numbers every power level holds. The order is the order of the per-level TuningKeys: append only, never
    /// reorder (see <see cref="PowerTuningKeys"/>).
    /// </summary>
    public enum PowerField
    {
        /// <summary>Seconds before the power can be used again, counted after its use ends.</summary>
        Cooldown = 0,
        /// <summary>Seconds the power lasts; 0 for an instant power.</summary>
        Duration = 1,
        /// <summary>Distance or range in world units (dash distance, gust range, recall range).</summary>
        Distance = 2,
        /// <summary>How hard the power pushes things.</summary>
        PushStrength = 3,
        /// <summary>Width of the power's reach in world units.</summary>
        Width = 4,
    }
}
