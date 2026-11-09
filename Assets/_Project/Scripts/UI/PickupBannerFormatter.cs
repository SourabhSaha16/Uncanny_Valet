using System.Collections.Generic;
using System.Text;

namespace UncannyValet.UI
{
    /// <summary>Pure text rules for the pickup banner, e.g. "PICKUP: ABC 123 spot 4". The car is told apart by its plate and tag colour. No Unity types.</summary>
    public static class PickupBannerFormatter
    {
        /// <summary>No lot number known yet.</summary>
        public const int NoSpot = 0;

        /// <summary>
        /// Appends one pickup line (no newline): "PICKUP: ABC 123 spot 4". The plate is left out when empty and the
        /// spot when it is <see cref="NoSpot"/>.
        /// </summary>
        public static void AppendPickup(StringBuilder text, string plate, int spot)
        {
            text.Append("PICKUP");
            var hasPlate = !string.IsNullOrEmpty(plate);
            if (hasPlate) text.Append(": ").Append(plate);
            if (spot == NoSpot) return;
            text.Append(hasPlate ? " spot " : ": spot ").Append(spot);
        }

        /// <summary>Appends the overflow line (no newline), e.g. "+2 more".</summary>
        public static void AppendOverflow(StringBuilder text, int hidden)
        {
            text.Append('+').Append(hidden).Append(" more");
        }

        /// <summary>
        /// Writes one line per pending pickup (at most maxLines, then "+N more"), separated by newlines with none at
        /// the end. Returns the number of lines written. When <paramref name="tags"/> is given it is cleared and
        /// receives the tag colour index of every pickup line (not the overflow line).
        /// </summary>
        public static int AppendAll(StringBuilder text, SortedDictionary<int, TicketRow>.ValueCollection rows,
            int maxLines, List<int> tags)
        {
            tags?.Clear();
            var lines = 0;
            var hidden = 0;
            foreach (var row in rows)
            {
                if (row.State != TicketRowState.PickupRequested) continue;
                if (lines >= maxLines) { hidden++; continue; }
                if (lines++ > 0) text.Append('\n');
                AppendPickup(text, row.Plate, row.Spot);
                tags?.Add(row.Tag);
            }
            if (hidden == 0) return lines;
            text.Append('\n');
            AppendOverflow(text, hidden);
            return lines + 1;
        }
    }
}
