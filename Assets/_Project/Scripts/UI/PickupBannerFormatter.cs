using System.Text;

namespace UncannyValet.UI
{
    /// <summary>Pure text rules for the pickup banner, e.g. "PICKUP: car B from spot 4". No Unity types.</summary>
    public static class PickupBannerFormatter
    {
        /// <summary>No lot number known yet.</summary>
        public const int NoSpot = 0;

        /// <summary>Appends one pickup line (no newline). The spot is left out when it is <see cref="NoSpot"/>.</summary>
        public static void AppendPickup(StringBuilder text, char letter, int spot)
        {
            text.Append("PICKUP: car ").Append(letter);
            if (spot != NoSpot) text.Append(" from spot ").Append(spot);
        }

        /// <summary>Appends the overflow line (no newline), e.g. "+2 more".</summary>
        public static void AppendOverflow(StringBuilder text, int hidden)
        {
            text.Append('+').Append(hidden).Append(" more");
        }
    }
}
