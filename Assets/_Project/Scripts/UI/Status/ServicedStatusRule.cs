namespace UncannyValet.UI.Status
{
    /// <summary>A modifier just cleared; the word comes from it (Washed, Charged, ...) and holds until the car is parked again.</summary>
    public sealed class ServicedStatusRule : ICarStatusRule
    {
        public CarStatus Status => CarStatus.Serviced;

        public bool Applies(TicketRow row) => row.JustCleared() != null;
    }
}
