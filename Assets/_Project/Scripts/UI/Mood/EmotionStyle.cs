using UnityEngine;

namespace UncannyValet.UI.Mood
{
    /// <summary>The emotion rule plus the colour of each emotion, built once from a <see cref="HudEmotionConfig"/>.</summary>
    public sealed class EmotionStyle
    {
        private readonly HudEmotionConfig _config;

        public EmotionStyle(HudEmotionConfig config)
        {
            _config = config;
            Rule = new EmotionRule(config.ImpatientFrom, config.AngryFrom, config.WillNotBlessFrom);
        }

        /// <summary>Maps patience to an emotion.</summary>
        public EmotionRule Rule { get; }

        /// <summary>The colour of the emotion.</summary>
        public Color ColorOf(Emotion emotion)
        {
            switch (emotion)
            {
                case Emotion.WillNotBless: return _config.WillNotBlessColor;
                case Emotion.Angry: return _config.AngryColor;
                case Emotion.Impatient: return _config.ImpatientColor;
                default: return _config.HappyColor;
            }
        }
    }
}
