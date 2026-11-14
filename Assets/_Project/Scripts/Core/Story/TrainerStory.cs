using System;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Core.Story
{
    /// <summary>The generic trainer line of a world's training lot on the results screen: failed or cleared, no god and no name.</summary>
    [Serializable]
    public struct TrainerStory
    {
        [SerializeField] private World _world;
        [SerializeField, Tooltip("Said when the training lot was failed. Uppercase, up to 90 characters, only the pixel font's characters.")] private string _failedLine;
        [SerializeField, Tooltip("Said when the threshold was reached.")] private string _clearedLine;

        public TrainerStory(World world, string failedLine, string clearedLine)
        {
            _world = world; _failedLine = failedLine; _clearedLine = clearedLine;
        }

        public World World => _world;

        /// <summary>The line for a tier (a training lot has no cleared-well line, so that tier reads the cleared one).</summary>
        public string Line(VerdictTier tier) => tier == VerdictTier.Failed ? _failedLine : _clearedLine;
    }
}
