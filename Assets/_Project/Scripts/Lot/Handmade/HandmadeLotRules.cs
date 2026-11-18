using System.Collections.Generic;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>
    /// The validator for an authored lot: pure rules over <see cref="HandmadeLotData"/> (see <see cref="LotRule"/> for the list). Errors mean the lot
    /// cannot be built; warnings only point out something the author probably did not mean.
    /// </summary>
    public static class HandmadeLotRules
    {
        /// <summary>Checks every rule and appends the findings to <paramref name="issues"/>.</summary>
        public static void Validate(HandmadeLotData data, List<LotIssue> issues)
        {
            LotPieceRules.Check(data, issues);
            LotRoadRules.Check(data, issues);
            LotWalkRules.Check(data, issues);
        }

        /// <summary>True when the list holds at least one error.</summary>
        public static bool HasErrors(IReadOnlyList<LotIssue> issues)
        {
            for (var i = 0; i < issues.Count; i++)
                if (issues[i].IsError) return true;
            return false;
        }
    }
}
