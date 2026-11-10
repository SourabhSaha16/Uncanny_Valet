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
            Rule = new EmotionRule(config.ImpatientFrom, config.AngryFrom);
        }

        /// <summary>Maps patience to an emotion.</summary>
        public EmotionRule Rule { get; }

        /// <summary>The colour of the emotion.</summary>
        public Color ColorOf(Emotion emotion)
        {
            if (emotion == Emotion.Angry) return _config.AngryColor;
            return emotion == Emotion.Impatient ? _config.ImpatientColor : _config.HappyColor;
        }

        /// <summary>Seconds per flash of the emotion.</summary>
        public float PeriodOf(Emotion emotion)
        {
            if (emotion == Emotion.Angry) return 1f / _config.AngryHz;
            return 1f / (emotion == Emotion.Impatient ? _config.ImpatientHz : _config.HappyHz);
        }

        /// <summary>True during the lit half of the current flash of this emotion.</summary>
        public bool IsLit(Emotion emotion, float time) => Blink.IsOn(time, PeriodOf(emotion));
    }
}
