namespace UncannyValet.UI.Status
{
    /// <summary>The EV just finished charging; holds until the car is parked again.</summary>
    public sealed class ChargedStatusRule : ICarStatusRule
    {
        public CarStatus Status => CarStatus.Charged;

        public bool Applies(TicketRow row) => row.ShowCharged;
    }
}
