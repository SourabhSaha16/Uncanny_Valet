namespace UncannyValet.Orders
{
    /// <summary>
    /// Tells charging spots from regular ones (ILotLayout cannot). Bootstrap registers an adapter in Services;
    /// orders only ever target regular spots.
    /// </summary>
    public interface IChargingSpotQuery
    {
        bool IsCharging(int spotId);
    }
}
