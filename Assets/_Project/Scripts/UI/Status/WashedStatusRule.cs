namespace UncannyValet.UI.Status
{
    /// <summary>The wash is done and the car waits for its patron.</summary>
    public sealed class WashedStatusRule : ICarStatusRule
    {
        public CarStatus Status => CarStatus.Washed;

        public bool Applies(TicketRow row) => row.IsDirty && row.Washed;
    }
}
