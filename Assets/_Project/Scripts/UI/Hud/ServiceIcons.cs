using UncannyValet.Core.Events;
using UncannyValet.UI.Kit;

namespace UncannyValet.UI.Hud
{
    /// <summary>Which small icon, if any, sits beside a car's plate: a droplet while it is being washed, a plug while it charges.</summary>
    public static class ServiceIcons
    {
        /// <summary>True when the car is in a station that is working on it right now; <paramref name="icon"/> says which.</summary>
        public static bool TryGet(TicketRow row, out PixelIcon icon)
        {
            icon = PixelIcon.Wash;
            if (row.InStation(StationKind.Wash) && row.NeedsWash) return true;
            if (!row.InStation(StationKind.Charging) || !row.NeedsCharge) return false;
            icon = PixelIcon.Plug;
            return true;
        }
    }
}
