using UnityEngine;

namespace UncannyValet.Core.Watchers
{
    /// <summary>One thing somebody wants to say in a speech bubble: who, what, where and how important. Plain data.</summary>
    public readonly struct SpeechRequest
    {
        /// <summary>The name shown above the line (for example MRS. OKAFOR).</summary>
        public readonly string Speaker;

        /// <summary>The line; uppercase, up to about 40 characters, only characters the pixel font has.</summary>
        public readonly string Text;

        /// <summary>Where the speaker stands in world space; used when <see cref="Anchor"/> is gone.</summary>
        public readonly Vector2 Position;

        /// <summary>The speaker's transform, so a walking speaker's bubble follows; null for a fixed spot.</summary>
        public readonly Transform Anchor;

        public readonly SpeechPriority Priority;

        public SpeechRequest(string speaker, string text, Vector2 position, Transform anchor, SpeechPriority priority)
        {
            Speaker = speaker; Text = text; Position = position; Anchor = anchor; Priority = priority;
        }

        /// <summary>Where the speaker is right now: the anchor when it still exists, otherwise <see cref="Position"/>.</summary>
        public Vector2 CurrentPosition => Anchor != null ? (Vector2)Anchor.position : Position;
    }
}
