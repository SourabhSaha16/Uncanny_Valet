namespace UncannyValet.UI.Status
{
    /// <summary>The wash just finished; holds until the car is parked again.</summary>
    public sealed class WashedStatusRule : ICarStatusRule
    {
        public CarStatus Status => CarStatus.Washed;

        public bool Applies(TicketRow row) => row.ShowWashed;
    }
}
