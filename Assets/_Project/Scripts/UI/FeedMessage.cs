using UncannyValet.Core.Ids;

namespace UncannyValet.UI
{
    /// <summary>One boss message card. Owned and mutated only by <see cref="MessageFeed"/>.</summary>
    public sealed class FeedMessage
    {
        public readonly OrderId Order;
        public readonly string Text;
        public readonly float IssuedAt;
        public MessageStatus Status { get; internal set; }
        public float ClosedAt { get; internal set; }

        public FeedMessage(OrderId order, string text, float issuedAt)
        {
            Order = order;
            Text = text;
            IssuedAt = issuedAt;
        }
    }
}
