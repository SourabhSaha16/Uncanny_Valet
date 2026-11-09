using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Pure rules for the pickup mark on a key board row: a cyan row highlight and a bell that flash with the same
    /// urgency maths as the plate (slow while patience is fresh, fast as it runs out; amber, then orange-red).
    /// </summary>
    public static class PickupMark
    {
        /// <summary>True when the row's car is wanted back, so the row shows the mark.</summary>
        public static bool IsRequested(TicketRow row) => row.State == TicketRowState.PickupRequested;

        /// <summary>True during the lit half of the current flash.</summary>
        public static bool IsLit(TicketRow row, float time)
            => Blink.IsOn(time, PlateLayout.FlashPeriod(row.Elapsed01, row.Expired));

        /// <summary>Highlight colour: cyan, amber when patience is running low, orange-red when expired or nearly so.</summary>
        public static Color Tint(TicketRow row)
            => AlarmColors.For(Alarm.ForPatience(row.Elapsed01, row.Expired), PixelPalette.Cyan);
    }
}
