using System.Collections.Generic;
using System.Text;

namespace UncannyValet.UI
{
    /// <summary>Pure text rules for the pickup banner, e.g. "PICKUP: spot 4". The car is told apart by its tag colour. No Unity types.</summary>
    public static class PickupBannerFormatter
    {
        /// <summary>No lot number known yet.</summary>
        public const int NoSpot = 0;

        /// <summary>Appends one pickup line (no newline). The spot is left out when it is <see cref="NoSpot"/>.</summary>
        public static void AppendPickup(StringBuilder text, int spot)
        {
            text.Append("PICKUP");
            if (spot != NoSpot) text.Append(": spot ").Append(spot);
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
                AppendPickup(text, row.Spot);
                tags?.Add(row.Tag);
            }
            if (hidden == 0) return lines;
            text.Append('\n');
            AppendOverflow(text, hidden);
            return lines + 1;
        }
    }
}
