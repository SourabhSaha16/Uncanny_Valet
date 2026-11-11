namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// How hard the player's car is sliding right now. Polled (no events, no allocation) by the tyre sound loop.
    /// Cars owns the value; Audio only reads it.
    /// </summary>
    public interface ISlipSource
    {
        /// <summary>True while the player's car is sliding.</summary>
        bool IsSliding { get; }

        /// <summary>Slide strength 0..1: 0 is the gentlest slide, 1 a full drift. 0 when not sliding.</summary>
        float Intensity01 { get; }
    }
}
