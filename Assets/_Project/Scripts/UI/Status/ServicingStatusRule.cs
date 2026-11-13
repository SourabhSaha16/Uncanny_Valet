using UncannyValet.Core.Events;

namespace UncannyValet.UI.Status
{
    /// <summary>The car is in a station that clears one of its open modifiers; the word comes from that modifier (Washing, Charging, ...).</summary>
    public sealed class ServicingStatusRule : ICarStatusRule
    {
        public CarStatus Status => CarStatus.Servicing;

        public bool Applies(TicketRow row) => row.HasStation && row.OpenAt(row.Station) != null;
    }
}
