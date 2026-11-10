namespace UncannyValet.UI.Status
{
    /// <summary>The patron wants the car and ran out of patience.</summary>
    public sealed class CustAngryStatusRule : ICarStatusRule
    {
        public CarStatus Status => CarStatus.CustAngry;

        public bool Applies(TicketRow row) => row.State == TicketRowState.PickupRequested && row.Expired;
    }
}
