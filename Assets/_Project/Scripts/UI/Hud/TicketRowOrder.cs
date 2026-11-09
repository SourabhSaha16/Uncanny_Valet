using System.Collections.Generic;

namespace UncannyValet.UI.Hud
{
    /// <summary>Pure ordering of the key board: pickups first, then the least patience left, then the rest by car.</summary>
    public static class TicketRowOrder
    {
        private const float PickupBase = 2f;
        private const float NoTimer = -1f;

        /// <summary>Higher means more urgent.</summary>
        public static float Urgency(TicketRow row)
        {
            var elapsed = row.Expired ? 1f : row.HasTimer ? row.Elapsed01 : 0f;
            if (row.State == TicketRowState.PickupRequested) return PickupBase + elapsed;
            return row.HasTimer || row.Expired ? elapsed : NoTimer;
        }

        /// <summary>Negative when <paramref name="a"/> should be listed first.</summary>
        public static int Compare(TicketRow a, TicketRow b)
        {
            var ua = Urgency(a);
            var ub = Urgency(b);
            if (ua != ub) return ua > ub ? -1 : 1;
            return a.Car.CompareTo(b.Car);
        }

        /// <summary>Insertion sort in place: the lists are tiny and this allocates nothing.</summary>
        public static void Sort(List<TicketRow> rows)
        {
            for (var i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                var j = i - 1;
                while (j >= 0 && Compare(rows[j], row) > 0)
                {
                    rows[j + 1] = rows[j];
                    j--;
                }
                rows[j + 1] = row;
            }
        }
    }
}
