using System.Text;
using UncannyValet.Core.Contracts;

namespace UncannyValet.UI
{
    /// <summary>The overlay's score block: score, combo, complaints, stars and the three star thresholds. Pure text.</summary>
    public static class ScoreLines
    {
        public static void Append(StringBuilder text, IShiftScoreView score)
        {
            text.Append("Score: $").Append(score.Score)
                .Append("   Combo: x").Append(score.ComboMultiplier)
                .Append("   Complaints: ").Append(score.Complaints).AppendLine();
            text.Append("Stars: ").Append(score.Stars).Append("   (1: $").Append(score.OneStarScore)
                .Append("  2: $").Append(score.TwoStarScore)
                .Append("  3: $").Append(score.ThreeStarScore).AppendLine(")");
        }
    }
}
