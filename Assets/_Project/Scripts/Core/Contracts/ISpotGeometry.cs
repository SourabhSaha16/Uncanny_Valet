namespace UncannyValet.Core.Contracts
{
    /// <summary>Where each bay is. Implemented by the Lot module, registered in Services by Bootstrap; read by the HUD.</summary>
    public interface ISpotGeometry
    {
        /// <summary>True and the bay's rectangle when <paramref name="spotId"/> is a bay the lot has built (regular or charging).</summary>
        bool TryGetSpot(int spotId, out SpotBounds bay);
    }
}
