using UncannyValet.UI.Kit;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Which small icons sit by a car: one per open service modifier (a mud splat while Muddy, a plug while it needs a
    /// charge), each drawn from its own definition. They show from the moment the car has arrived (its patron steps out)
    /// until the modifier clears or the ticket closes.
    /// </summary>
    public static class ServiceIcons
    {
        /// <summary>How many icons the car shows: its open modifiers once it has arrived.</summary>
        public static int Count(TicketRow row)
        {
            if (!row.Arrived) return 0;
            var n = 0;
            for (var i = 0; i < row.Modifiers.Count; i++) if (row.Modifiers[i].IsOpen) n++;
            return n;
        }

        /// <summary>The icon at <paramref name="index"/> (open modifiers in list order); false past <see cref="Count"/>.</summary>
        public static bool TryGet(TicketRow row, int index, out PixelIcon icon)
        {
            icon = default;
            if (index < 0 || !row.Arrived) return false;
            for (var i = 0; i < row.Modifiers.Count; i++)
            {
                if (!row.Modifiers[i].IsOpen) continue;
                if (index-- > 0) continue;
                icon = row.Modifiers[i].Icon;
                return true;
            }
            return false;
        }
    }
}
