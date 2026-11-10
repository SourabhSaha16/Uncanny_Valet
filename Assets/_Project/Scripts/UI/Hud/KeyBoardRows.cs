using System.Collections.Generic;

namespace UncannyValet.UI.Hud
{
    /// <summary>Pure rule for what the key board lists: a car appears once it is parked in a spot or station, and stays until delivered.</summary>
    public static class KeyBoardRows
    {
        /// <summary>True when the row belongs on the key board.</summary>
        public static bool IsListed(TicketRow row) => row.Placed;

        /// <summary>Removes the rows that are not listed, keeping the order. Allocates nothing.</summary>
        public static void Filter(List<TicketRow> rows)
        {
            for (var i = rows.Count - 1; i >= 0; i--)
                if (!IsListed(rows[i])) rows.RemoveAt(i);
        }
    }
}
