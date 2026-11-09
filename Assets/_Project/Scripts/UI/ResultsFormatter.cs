using System.Text;
using UncannyValet.Core.Events;

namespace UncannyValet.UI
{
    /// <summary>Turns a <see cref="ShiftResult"/> into the text on the results screen. Pure.</summary>
    public static class ResultsFormatter
    {
        public const string Title = "Shift over";
        public const string RetryLabel = "Retry shift";
        public const int MaxStars = 3;
        private const char Filled = '*';
        private const char Empty = '-';

        public static ResultsContent Build(in ShiftResult result)
        {
            return new ResultsContent
            {
                Title = Title,
                Stars = StarText(result.Stars),
                Lines = new[]
                {
                    $"Score: ${result.Score}",
                    $"Complaints: {result.Complaints}",
                    NextGoal(result),
                },
                ButtonLabel = RetryLabel,
            };
        }

        /// <summary>One character per possible star: filled for earned, empty for the rest.</summary>
        public static string StarText(int stars)
        {
            var text = new StringBuilder(MaxStars);
            for (var i = 0; i < MaxStars; i++) text.Append(i < stars ? Filled : Empty);
            return text.ToString();
        }

        private static string NextGoal(in ShiftResult r)
        {
            if (r.Stars >= MaxStars) return "Three stars. Top marks!";
            var target = r.Stars == 0 ? r.OneStarScore : r.Stars == 1 ? r.TwoStarScore : r.ThreeStarScore;
            return $"Next star at ${target} ({target - r.Score} to go)";
        }
    }
}
