using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UncannyValet.Core.Contracts;

namespace UncannyValet.UI
{
    /// <summary>
    /// Builds the flat JSON object copied by the tuning panel: every catalog key name mapped to its current value,
    /// ordered by enum value, numbers written with the invariant culture. Plain C#, no Unity types.
    /// </summary>
    public static class TuningSnapshotJson
    {
        /// <summary>Returns {"KeyName": value, ...} for every row; a non-finite value is written as null.</summary>
        public static string Build(IReadOnlyList<TuningInfo> rows, Func<TuningInfo, float> getValue)
        {
            var sorted = new List<TuningInfo>(rows);
            sorted.Sort((a, b) => ((int)a.Key).CompareTo((int)b.Key));
            var sb = new StringBuilder(sorted.Count * 32);
            sb.Append('{');
            for (var i = 0; i < sorted.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append("\n  \"").Append(sorted[i].Key.ToString()).Append("\": ");
                AppendNumber(sb, getValue(sorted[i]));
            }
            return sb.Append("\n}").ToString();
        }

        private static void AppendNumber(StringBuilder sb, float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) sb.Append("null");
            else sb.Append(value.ToString("R", CultureInfo.InvariantCulture));
        }
    }
}
