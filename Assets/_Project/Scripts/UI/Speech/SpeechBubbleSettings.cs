using System;
using UnityEngine;

namespace UncannyValet.UI.Speech
{
    /// <summary>Tunables of the watcher speech bubble.</summary>
    [Serializable]
    public sealed class SpeechBubbleSettings
    {
        /// <summary>The scale used when no config is given (owner playtest 2026-11-14: about 28 percent smaller).</summary>
        public const float DefaultScale = 0.72f;

        [SerializeField, Range(0.4f, 1f), Tooltip("Size of the whole bubble (panel, name and text) as a share of the normal pixel size. 0.72 is about 28% smaller; 1 is the old size.")]
        private float _scale = DefaultScale;

        /// <summary>The view margin used when no config is given.</summary>
        public const float DefaultViewMargin = 0.02f;

        [SerializeField, Min(0f), Tooltip("How far past the screen edge (as a share of the screen size) the speaker may stand and still get a bubble. Beyond it the bubble is hidden; the tip money is paid either way.")]
        private float _viewMargin = DefaultViewMargin;

        public float Scale => _scale;

        public float ViewMargin => _viewMargin;
    }
}
