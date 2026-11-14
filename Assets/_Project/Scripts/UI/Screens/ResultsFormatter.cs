using UncannyValet.Core.Events;
using UncannyValet.UI.Kit;

namespace UncannyValet.UI.Screens
{
    /// <summary>Turns a <see cref="ShiftResult"/> and the shift's stats into the text on the results screen. Pure.</summary>
    public static class ResultsFormatter
    {
        public const string Title = "SHIFT OVER";
        public const string ClearedLabel = "CLEARED";
        public const string FailedLabel = "FAILED";
        public const string BlessingsHeading = "BLESSINGS / THRESHOLD";
        public const string RetryLabel = "RETRY";
        public const string NextLotLabel = "NEXT LOT";
        public const string ReachedLine = "THRESHOLD REACHED!";

        public static ResultsContent Build(in ShiftResult result, int carsParked, int perfectParks = 0)
        {
            return new ResultsContent
            {
                Title = Title,
                Verdict = result.Cleared ? ClearedLabel : FailedLabel,
                Cleared = result.Cleared,
                Blessings = result.Blessings + " / " + result.Threshold,
                Goal = Goal(result),
                Stats = Stats(result, carsParked, perfectParks),
            };
        }

        private static StatLine[] Stats(in ShiftResult result, int carsParked, int perfectParks)
        {
            return new[]
            {
                new StatLine(PixelIcon.Tyre, "CARS PARKED", carsParked.ToString()),
                new StatLine(PixelIcon.Bolt, "PEAK FAVOUR", "X" + result.PeakFavour),
                new StatLine(PixelIcon.Bell, "CURSES", result.Curses.ToString()),
                new StatLine(PixelIcon.Car, "PERFECT PARKS", perfectParks.ToString()),
            };
        }

        private static string Goal(in ShiftResult r)
            => r.Cleared ? ReachedLine : (r.Threshold - r.Blessings) + " MORE BLESSINGS NEEDED";
    }
}
