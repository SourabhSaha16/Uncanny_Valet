namespace UncannyValet.UI.Mood
{
    /// <summary>Pure rule: the share of pickup patience that has run out decides the emotion.</summary>
    public sealed class EmotionRule
    {
        private float _impatientFrom;
        private float _angryFrom;
        private float _willNotTipFrom;

        /// <summary>Thresholds are fractions of the patience used up (0 = just started, 1 = out of time); each level never starts before the one below it.</summary>
        public EmotionRule(float impatientFrom, float angryFrom, float willNotTipFrom = 1f)
        {
            _impatientFrom = impatientFrom;
            SetLevels(angryFrom, willNotTipFrom);
        }

        /// <summary>Changes the angry and will-not-tip levels (a live tuning change).</summary>
        public void SetLevels(float angryFrom, float willNotTipFrom)
        {
            _angryFrom = angryFrom < _impatientFrom ? _impatientFrom : angryFrom;
            _willNotTipFrom = willNotTipFrom < _angryFrom ? _angryFrom : willNotTipFrom;
        }

        /// <summary>The emotion at this point of the timer; an expired timer always means the patron will not tip.</summary>
        public Emotion Resolve(float elapsed01, bool expired)
        {
            if (expired || elapsed01 >= _willNotTipFrom) return Emotion.WillNotTip;
            if (elapsed01 >= _angryFrom) return Emotion.Angry;
            return elapsed01 >= _impatientFrom ? Emotion.Impatient : Emotion.Happy;
        }

        /// <summary>The emotion of a ticket row.</summary>
        public Emotion Resolve(TicketRow row) => Resolve(row.Elapsed01, row.Expired);
    }
}
