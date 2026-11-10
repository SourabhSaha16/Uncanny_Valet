using UncannyValet.Core.Events;

namespace UncannyValet.UI.Status
{
    /// <summary>The car is on a charger and not full yet.</summary>
    public sealed class ChargingStatusRule : ICarStatusRule
    {
        public CarStatus Status => CarStatus.Charging;

        public bool Applies(TicketRow row) => row.InStation(StationKind.Charging) && row.NeedsCharge;
    }
}
