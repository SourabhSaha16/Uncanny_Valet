using UncannyValet.Core.Events;
using UncannyValet.UI.Kit;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Which small icons sit by a car, in modifier list order, once it has arrived: a mud splat while Muddy and a plug while it needs a
    /// charge, each drawn from its own definition. While the wash station works on the car its wash pie replaces the mud splat (no icon),
    /// and once washed a blue drop stays until the ticket closes. A charged car shows nothing.
    /// </summary>
    public static class ServiceIcons
    {
        /// <summary>How many icons the car shows once it has arrived.</summary>
        public static int Count(TicketRow row)
        {
            if (!row.Arrived) return 0;
            var n = 0;
            for (var i = 0; i < row.Modifiers.Count; i++) if (TryIcon(row, row.Modifiers[i], out _)) n++;
            return n;
        }

        /// <summary>The icon at <paramref name="index"/>; false past <see cref="Count"/>.</summary>
        public static bool TryGet(TicketRow row, int index, out PixelIcon icon)
        {
            icon = default;
            if (index < 0 || !row.Arrived) return false;
            for (var i = 0; i < row.Modifiers.Count; i++)
            {
                if (!TryIcon(row, row.Modifiers[i], out var found)) continue;
                if (index-- > 0) continue;
                icon = found;
                return true;
            }
            return false;
        }

        // Open: its own icon, except while the wash pie stands in for it. Cleared at the wash: the washed drop.
        private static bool TryIcon(TicketRow row, RowModifier modifier, out PixelIcon icon)
        {
            icon = modifier.Icon;
            if (modifier.IsOpen) return !(modifier.ClearedAt == StationKind.Wash && row.InStation(StationKind.Wash));
            icon = PixelIcon.Drop;
            return modifier.ClearedAt == StationKind.Wash;
        }
    }
}
