using System.Collections.Generic;
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

        public static ResultsContent Build(in ShiftResult result, int carsParked, int perfectParks = 0, VerdictSpeech speech = default)
        {
            return new ResultsContent
            {
                Title = Title,
                Verdict = result.Cleared ? ClearedLabel : FailedLabel,
                Cleared = result.Cleared,
                Blessings = result.Blessings + " / " + result.Threshold,
                Goal = Goal(result),
                Stats = Stats(result, carsParked, perfectParks),
                Speaker = speech.Speaker ?? string.Empty,
                SpeechLines = Wrap(speech.Line),
            };
        }

        /// <summary>The verdict line broken to the width of the speech panel.</summary>
        public static string[] Wrap(string line)
        {
            if (string.IsNullOrEmpty(line)) return System.Array.Empty<string>();
            var spans = new List<WordWrap.Span>();
            WordWrap.Wrap(line, ResultsPainter.SpeechChars, spans);
            var lines = new string[spans.Count];
            for (var i = 0; i < lines.Length; i++) lines[i] = line.Substring(spans[i].Start, spans[i].Length);
            return lines;
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
