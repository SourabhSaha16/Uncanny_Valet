using UncannyValet.Core.Events;

namespace UncannyValet.UI.Status
{
    /// <summary>The car is in the wash slot and not clean yet.</summary>
    public sealed class WashingStatusRule : ICarStatusRule
    {
        public CarStatus Status => CarStatus.Washing;

        public bool Applies(TicketRow row) => row.InStation(StationKind.Wash) && row.NeedsWash;
    }
}
