using UncannyValet.Core.Events;
using UncannyValet.UI.Kit;

namespace UncannyValet.UI.Screens
{
    /// <summary>Turns a <see cref="ShiftResult"/> and the shift's stats into the text on the results screen. Pure.</summary>
    public static class ResultsFormatter
    {
        public const string Title = "SHIFT OVER";
        public const string RetryLabel = "RETRY";
        public const string NextLotLabel = "NEXT LOT";
        public const string SoonLabel = "SOON";
        public const string BossName = "MRS. OKAFOR";
        public const string TopMarks = "THREE STARS! TOP MARKS!";
        public const int MaxStars = 3;

        public static ResultsContent Build(in ShiftResult result, int carsParked, int carsReturned, int bestCombo)
        {
            return new ResultsContent
            {
                Title = Title,
                Score = "$" + result.Score,
                Stars = result.Stars,
                Goal = NextGoal(result),
                Stats = new[]
                {
                    new StatLine(PixelIcon.Tyre, "CARS PARKED", carsParked.ToString()),
                    new StatLine(PixelIcon.Car, "CARS RETURNED", carsReturned.ToString()),
                    new StatLine(PixelIcon.StarLit, "BEST COMBO", "X" + bestCombo),
                    new StatLine(PixelIcon.Bell, "COMPLAINTS", result.Complaints.ToString()),
                },
            };
        }

        private static string NextGoal(in ShiftResult r)
        {
            if (r.Stars >= MaxStars) return TopMarks;
            var target = r.Stars == 0 ? r.OneStarScore : r.Stars == 1 ? r.TwoStarScore : r.ThreeStarScore;
            return "NEXT STAR AT $" + target + " (" + (target - r.Score) + " TO GO)";
        }
    }
}
