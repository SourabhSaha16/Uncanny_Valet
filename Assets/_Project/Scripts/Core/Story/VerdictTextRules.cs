using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Story
{
    /// <summary>Checks the results screen's verdict and trainer lines of a <see cref="StoryTextAsset"/> against the lots it must cover. Pure.</summary>
    public static class VerdictTextRules
    {
        /// <summary>Longest verdict or trainer line, in characters (it wraps on the panel).</summary>
        public const int MaxLength = StoryTextRules.WrappedLength;

        /// <summary>Null when every training lot has a trainer pair and every crazy lot and finale a speaker and three lines, otherwise the first fault.</summary>
        public static string Check(StoryTextAsset text, IReadOnlyList<LotId> lots)
        {
            if (text.ClearedWellMultiple < 1f) return "cleared-well multiple is under 1";
            for (var i = 0; i < lots.Count; i++)
            {
                var problem = lots[i].Kind == LotKind.Training ? CheckTrainer(text, lots[i].World) : CheckVerdict(text, lots[i]);
                if (problem != null) return problem;
            }
            return null;
        }

        private static string CheckTrainer(StoryTextAsset text, World world)
        {
            if (!text.TryGetTrainer(world, out var story)) return "no trainer lines for " + world;
            return StoryTextRules.CheckLine(story.Line(VerdictTier.Failed), "failed trainer line of " + world, MaxLength)
                ?? StoryTextRules.CheckLine(story.Line(VerdictTier.Cleared), "cleared trainer line of " + world, MaxLength);
        }

        private static string CheckVerdict(StoryTextAsset text, LotId lot)
        {
            if (!text.TryGetVerdict(lot, out var story)) return "no verdict lines for " + lot;
            return StoryTextRules.CheckLine(story.Speaker, "speaker of " + lot, StoryTextAsset.MaxLineLength)
                ?? StoryTextRules.CheckLine(story.Line(VerdictTier.Failed), "failed verdict of " + lot, MaxLength)
                ?? StoryTextRules.CheckLine(story.Line(VerdictTier.Cleared), "cleared verdict of " + lot, MaxLength)
                ?? StoryTextRules.CheckLine(story.Line(VerdictTier.ClearedWell), "cleared-well verdict of " + lot, MaxLength);
        }
    }
}
