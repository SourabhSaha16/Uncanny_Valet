namespace UncannyValet.Core.Contracts
{
    /// <summary>The rule for which way a car must face to be handed back at the curb. Implemented by the Lot module.</summary>
    public interface ICurbFacing
    {
        /// <summary>True when a car rotated by <paramref name="rotationDegrees"/> (0 = nose up) points down the screen within the tolerance.</summary>
        bool IsAcceptable(float rotationDegrees);
    }
}
