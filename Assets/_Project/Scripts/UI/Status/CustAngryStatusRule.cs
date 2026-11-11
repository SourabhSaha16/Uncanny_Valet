using UncannyValet.UI.Mood;

namespace UncannyValet.UI.Status
{
    /// <summary>The patron wants the car and is angry (the red emotion, before the patron gives up).</summary>
    public sealed class CustAngryStatusRule : ICarStatusRule
    {
        private readonly EmotionRule _emotions;

        public CustAngryStatusRule(EmotionRule emotions) => _emotions = emotions;

        public CarStatus Status => CarStatus.CustAngry;

        public bool Applies(TicketRow row)
            => row.State == TicketRowState.PickupRequested && _emotions.Resolve(row) == Emotion.Angry;
    }
}
