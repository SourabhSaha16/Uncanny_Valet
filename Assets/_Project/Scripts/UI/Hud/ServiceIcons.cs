using UncannyValet.UI.Kit;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Which small icons sit by a car: a droplet while it still needs a wash, a plug while an EV still needs charge.
    /// They show from the moment the car has arrived (its patron steps out) until the service finishes or the ticket closes.
    /// </summary>
    public static class ServiceIcons
    {
        /// <summary>How many icons the car shows (0 to 2).</summary>
        public static int Count(TicketRow row) => (WantsWash(row) ? 1 : 0) + (WantsPlug(row) ? 1 : 0);

        /// <summary>The icon at <paramref name="index"/> (droplet first, then plug); false past <see cref="Count"/>.</summary>
        public static bool TryGet(TicketRow row, int index, out PixelIcon icon)
        {
            icon = PixelIcon.Wash;
            if (index < 0 || index >= Count(row)) return false;
            if (index == 0 && WantsWash(row)) return true;
            icon = PixelIcon.Plug;
            return true;
        }

        private static bool WantsWash(TicketRow row) => row.Arrived && row.NeedsWash;

        private static bool WantsPlug(TicketRow row) => row.Arrived && row.NeedsCharge;
    }
}
