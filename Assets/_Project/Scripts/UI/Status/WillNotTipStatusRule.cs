using UncannyValet.UI.Mood;

namespace UncannyValet.UI.Status
{
    /// <summary>The patron wants the car but has run out of patience and will not tip (includes an expired timer).</summary>
    public sealed class WillNotTipStatusRule : ICarStatusRule
    {
        private readonly EmotionRule _emotions;

        public WillNotTipStatusRule(EmotionRule emotions) => _emotions = emotions;

        public CarStatus Status => CarStatus.WillNotTip;

        public bool Applies(TicketRow row)
            => row.State == TicketRowState.PickupRequested && _emotions.Resolve(row) == Emotion.WillNotTip;
    }
}
