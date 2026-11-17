using UncannyValet.UI.Mood;

namespace UncannyValet.UI.Status
{
    /// <summary>The patron wants the car but has run out of patience and will not bless (includes an expired timer).</summary>
    public sealed class WillNotBlessStatusRule : ICarStatusRule
    {
        private readonly EmotionRule _emotions;

        public WillNotBlessStatusRule(EmotionRule emotions) => _emotions = emotions;

        public CarStatus Status => CarStatus.WillNotBless;

        public bool Applies(TicketRow row)
            => row.State == TicketRowState.PickupRequested && _emotions.Resolve(row) == Emotion.WillNotBless;
    }
}
