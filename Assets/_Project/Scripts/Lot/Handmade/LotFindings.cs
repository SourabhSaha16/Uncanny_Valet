using System.Collections.Generic;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>Small helpers the rule classes share to record findings.</summary>
    internal static class LotFindings
    {
        public static void Error(List<LotIssue> issues, LotRule rule, string message, string subject = "")
            => issues.Add(new LotIssue(rule, LotSeverity.Error, message, subject));

        public static void Warn(List<LotIssue> issues, LotRule rule, string message, string subject = "")
            => issues.Add(new LotIssue(rule, LotSeverity.Warning, message, subject));

        /// <summary>True when the numbers are exactly first, first + 1, ... with no repeat and no gap.</summary>
        public static bool IsRun(List<int> numbers, int first)
        {
            var sorted = new List<int>(numbers);
            sorted.Sort();
            for (var i = 0; i < sorted.Count; i++)
                if (sorted[i] != first + i) return false;
            return true;
        }
    }
}
