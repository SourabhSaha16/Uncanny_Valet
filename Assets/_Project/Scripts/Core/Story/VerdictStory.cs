using System;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Core.Story
{
    /// <summary>What the local god of a crazy lot (or Odin at the finale) says on the results screen: a name and one line per tier.</summary>
    [Serializable]
    public struct VerdictStory
    {
        [SerializeField] private LotId _lot;
        [SerializeField, Tooltip("The name shown on the panel, uppercase (FREYJA, MRS. OKAFOR).")] private string _speaker;
        [SerializeField, Tooltip("Said when the lot was failed. Uppercase, up to 90 characters, only the pixel font's characters.")] private string _failedLine;
        [SerializeField, Tooltip("Said when the threshold was reached.")] private string _clearedLine;
        [SerializeField, Tooltip("Said when the blessings reach the cleared-well multiple of the threshold.")] private string _clearedWellLine;

        public VerdictStory(LotId lot, string speaker, string failedLine, string clearedLine, string clearedWellLine)
        {
            _lot = lot; _speaker = speaker; _failedLine = failedLine; _clearedLine = clearedLine; _clearedWellLine = clearedWellLine;
        }

        public LotId Lot => _lot;
        public string Speaker => _speaker;

        /// <summary>The line for a tier.</summary>
        public string Line(VerdictTier tier)
        {
            switch (tier)
            {
                case VerdictTier.Failed: return _failedLine;
                case VerdictTier.ClearedWell: return _clearedWellLine;
                default: return _clearedLine;
            }
        }
    }
}
