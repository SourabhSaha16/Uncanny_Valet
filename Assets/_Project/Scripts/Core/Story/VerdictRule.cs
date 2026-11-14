using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Story
{
    /// <summary>Picks the verdict tier of a shift (issue 85). Pure.</summary>
    public static class VerdictRule
    {
        /// <summary>
        /// Failed below the threshold, Cleared at it, ClearedWell at or above <paramref name="clearedWellMultiple"/> times the threshold.
        /// Training lots never reach ClearedWell (their trainer has two lines). A multiple under 1 counts as 1.
        /// </summary>
        public static VerdictTier Tier(LotKind kind, int blessings, int threshold, float clearedWellMultiple)
        {
            if (blessings < threshold) return VerdictTier.Failed;
            if (kind == LotKind.Training) return VerdictTier.Cleared;
            var multiple = clearedWellMultiple < 1f ? 1f : clearedWellMultiple;
            return blessings >= threshold * (double)multiple ? VerdictTier.ClearedWell : VerdictTier.Cleared;
        }
    }
}
