namespace UncannyValet.UI.Mood
{
    /// <summary>Pure rule: the share of pickup patience that has run out decides the emotion.</summary>
    public sealed class EmotionRule
    {
        private float _impatientFrom;
        private float _angryFrom;
        private float _willNotBlessFrom;

        /// <summary>Thresholds are fractions of the patience used up (0 = just started, 1 = out of time); each level never starts before the one below it.</summary>
        public EmotionRule(float impatientFrom, float angryFrom, float willNotBlessFrom = 1f)
        {
            _impatientFrom = impatientFrom;
            SetLevels(angryFrom, willNotBlessFrom);
        }

        /// <summary>Changes the angry and will-not-blessing levels (a live tuning change).</summary>
        public void SetLevels(float angryFrom, float willNotBlessFrom)
        {
            _angryFrom = angryFrom < _impatientFrom ? _impatientFrom : angryFrom;
            _willNotBlessFrom = willNotBlessFrom < _angryFrom ? _angryFrom : willNotBlessFrom;
        }

        /// <summary>The emotion at this point of the timer; an expired timer always means the patron will not bless.</summary>
        public Emotion Resolve(float elapsed01, bool expired)
        {
            if (expired || elapsed01 >= _willNotBlessFrom) return Emotion.WillNotBless;
            if (elapsed01 >= _angryFrom) return Emotion.Angry;
            return elapsed01 >= _impatientFrom ? Emotion.Impatient : Emotion.Happy;
        }

        /// <summary>The emotion of a ticket row.</summary>
        public Emotion Resolve(TicketRow row) => Resolve(row.Elapsed01, row.Expired);
    }
}
