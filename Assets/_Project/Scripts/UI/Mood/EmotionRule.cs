namespace UncannyValet.UI.Mood
{
    /// <summary>Pure rule: the share of pickup patience that has run out decides the emotion.</summary>
    public sealed class EmotionRule
    {
        private readonly float _impatientFrom;
        private readonly float _angryFrom;

        /// <summary>Thresholds are fractions of the patience used up (0 = just started, 1 = out of time); angry never starts before impatient.</summary>
        public EmotionRule(float impatientFrom, float angryFrom)
        {
            _impatientFrom = impatientFrom;
            _angryFrom = angryFrom < impatientFrom ? impatientFrom : angryFrom;
        }

        /// <summary>The emotion at this point of the timer; an expired timer is always angry.</summary>
        public Emotion Resolve(float elapsed01, bool expired)
        {
            if (expired || elapsed01 >= _angryFrom) return Emotion.Angry;
            return elapsed01 >= _impatientFrom ? Emotion.Impatient : Emotion.Happy;
        }

        /// <summary>The emotion of a ticket row.</summary>
        public Emotion Resolve(TicketRow row) => Resolve(row.Elapsed01, row.Expired);
    }
}
