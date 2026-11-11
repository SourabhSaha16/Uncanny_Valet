using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Mood
{
    /// <summary>The emotion rule plus the colour and flash period of each emotion, built once from a <see cref="HudEmotionConfig"/>.</summary>
    public sealed class EmotionStyle
    {
        private readonly HudEmotionConfig _config;

        public EmotionStyle(HudEmotionConfig config)
        {
            _config = config;
            Rule = new EmotionRule(config.ImpatientFrom, config.AngryFrom, config.WillNotTipFrom);
        }

        /// <summary>Maps patience to an emotion.</summary>
        public EmotionRule Rule { get; }

        /// <summary>The colour of the emotion.</summary>
        public Color ColorOf(Emotion emotion)
        {
            switch (emotion)
            {
                case Emotion.WillNotTip: return _config.WillNotTipColor;
                case Emotion.Angry: return _config.AngryColor;
                case Emotion.Impatient: return _config.ImpatientColor;
                default: return _config.HappyColor;
            }
        }

        /// <summary>Seconds per flash of the emotion.</summary>
        public float PeriodOf(Emotion emotion) => 1f / HzOf(emotion);

        /// <summary>True during the lit half of the current flash of this emotion.</summary>
        public bool IsLit(Emotion emotion, float time) => Blink.IsOn(time, PeriodOf(emotion));

        private float HzOf(Emotion emotion)
        {
            switch (emotion)
            {
                case Emotion.WillNotTip: return _config.WillNotTipHz;
                case Emotion.Angry: return _config.AngryHz;
                case Emotion.Impatient: return _config.ImpatientHz;
                default: return _config.HappyHz;
            }
        }
    }
}
