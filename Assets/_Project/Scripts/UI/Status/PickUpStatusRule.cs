namespace UncannyValet.UI.Status
{
    /// <summary>The patron wants the car back.</summary>
    public sealed class PickUpStatusRule : ICarStatusRule
    {
        public CarStatus Status => CarStatus.PickUp;

        public bool Applies(TicketRow row) => row.State == TicketRowState.PickupRequested;
    }
}
