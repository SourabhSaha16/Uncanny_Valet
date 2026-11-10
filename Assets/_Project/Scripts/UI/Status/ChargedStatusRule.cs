namespace UncannyValet.UI.Status
{
    /// <summary>The EV is fully charged and waits for its patron.</summary>
    public sealed class ChargedStatusRule : ICarStatusRule
    {
        public CarStatus Status => CarStatus.Charged;

        public bool Applies(TicketRow row) => row.IsEv && !row.NeedsCharge;
    }
}
