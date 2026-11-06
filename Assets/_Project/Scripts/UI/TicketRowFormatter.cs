using System.Text;

namespace UncannyValet.UI
{
    /// <summary>Pure text rules for one overlay line, e.g. "A  [Park 31%]  (spot 3)". No Unity types.</summary>
    public static class TicketRowFormatter
    {
        private const string Gap = "  ";

        public static string Format(TicketRow row)
        {
            var text = new StringBuilder();
            Append(text, row);
            return text.ToString();
        }

        public static void Append(StringBuilder text, TicketRow row)
        {
            text.Append(Gap).Append(row.Letter).Append(Gap);
            AppendStatus(text, row);
            AppendSpot(text, row);
            AppendTags(text, row);
        }

        /// <summary>Trait tags: " EV 45%", " DIRTY" until washed, then " clean".</summary>
        public static void AppendTags(StringBuilder text, TicketRow row)
        {
            if (row.IsEv)
            {
                text.Append(" EV");
                if (row.Charge01 >= 0f) text.Append(' ').Append(Percent(row.Charge01)).Append('%');
            }
            if (row.IsDirty) text.Append(row.Washed ? " clean" : " DIRTY");
        }

        public static int Percent(float value01)
        {
            var clamped = value01 < 0f ? 0f : value01 > 1f ? 1f : value01;
            return (int)(clamped * 100f + 0.5f);
        }

        private static void AppendSpot(StringBuilder text, TicketRow row)
        {
            if (row.Spot == 0) return;
            var inSpot = row.State != TicketRowState.AwaitingPark;
            text.Append(Gap).Append(inSpot ? "in spot " : "(spot ").Append(row.Spot);
            if (!inSpot) text.Append(')');
        }

        public static int PercentLeft(float elapsed01)
        {
            var clamped = elapsed01 < 0f ? 0f : elapsed01 > 1f ? 1f : elapsed01;
            return (int)((1f - clamped) * 100f + 0.5f);
        }

        private static void AppendStatus(StringBuilder text, TicketRow row)
        {
            var pickup = row.State == TicketRowState.PickupRequested;
            if (row.Expired)
            {
                text.Append(pickup ? "LATE: tip $0" : "Out of time");
                return;
            }
            if (row.HasTimer)
            {
                text.Append('[').Append(pickup ? "Pickup" : "Park").Append(' ');
                text.Append(PercentLeft(row.Elapsed01)).Append("%]");
                return;
            }
            text.Append(row.State == TicketRowState.Parked ? "Parked" : "Waiting");
        }
    }
}
