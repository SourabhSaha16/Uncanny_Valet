namespace UncannyValet.Core.Events
{
    /// <summary>Why a tip was forced to $0, if it was.</summary>
    public enum TipZeroReason
    {
        None,
        PickupExpired,
        NotCleaned,
        Wrecked
    }
}
