using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Screens
{
    /// <summary>
    /// The results screen: a dimmed lot, one big panel, the verdict (CLEARED or FAILED), the blessings against the
    /// threshold with the bolt, the stats in two columns, the local god's speech panel (a trainer line without a panel on a training lot), and the Retry and Next Lot buttons.
    /// </summary>
    public sealed class ResultsPainter
    {
        /// <summary>The Retry button, in canvas pixels. Clicks are tested against it.</summary>
        public static readonly PixelRect RetryButton = new PixelRect(96, 224, 128, 24);

        /// <summary>The Next Lot button, in canvas pixels. Clicks are tested against it.</summary>
        public static readonly PixelRect NextButton = new PixelRect(256, 224, 128, 24);
        private static readonly PixelRect Card = new PixelRect(48, 12, 384, 252);

        private const string KeyHint = "ENTER: RETRY   ANY OTHER KEY: NEXT LOT";
        private const string NextOnlyHint = "ANY KEY: NEXT LOT";
        private const int TitleScale = 2;
        private const int VerdictScale = 4;
        private const int VerdictTop = 44;
        private const int HeadingTop = 82;
        private const int ScoreTop = 92;
        private const int ScoreGap = 6;
        private const int StatsTop = 124;
        private const int StatStep = 16;
        private const int StatColumns = 2;
        private const int StatColumnWidth = 180;
        private const int StatsLeft = 72;
        private const int StatValueRight = 152;
        private const int GoalTop = 160;
        private const int TrainerTop = 184;
        private const int SpeechStep = 9;
        private const int SpeechPad = 8;
        private const int SpeechNameTop = 5;
        private const int SpeechTextTop = 17;
        private static readonly PixelRect SpeechPanel = new PixelRect(72, 174, 336, 44);

        /// <summary>Longest line of the speech, in characters, that fits the panel.</summary>
        public const int SpeechChars = 50;

        /// <summary>Most lines of the speech the panel holds.</summary>
        public const int MaxSpeechLines = 3;

        private readonly PixelCanvas _canvas;

        public ResultsPainter(PixelCanvas canvas)
        {
            _canvas = canvas;
        }

        public void Draw(ResultsContent content, bool canRetry)
        {
            _canvas.Fill(0, 0, PixelGrid.Width, PixelGrid.Height, PixelPalette.Dim);
            _canvas.Panel(Card.X, Card.Y, Card.Width, Card.Height, PixelPalette.PanelDark);
            DrawCentered(content.Title, Card.Y + 12, TitleScale, PixelPalette.Gold);
            DrawCentered(content.Verdict, VerdictTop, VerdictScale, content.Cleared ? PixelPalette.Green : PixelPalette.OrangeRed);
            DrawCentered(ResultsFormatter.BlessingsHeading, HeadingTop, 1, PixelPalette.Cream);
            DrawBlessings(content.Blessings);
            DrawStats(content);
            DrawCentered(content.Goal, GoalTop, 1, PixelPalette.Cream);
            DrawSpeech(content);
            DrawButtons(canRetry);
        }

        private void DrawBlessings(string blessings)
        {
            var bolt = _canvas.IconWidth(PixelIcon.PrayerHands) * TitleScale;
            var width = bolt + ScoreGap + PixelTextLayout.Width(blessings.Length, TitleScale);
            var x = PixelTextLayout.Centered(0, PixelGrid.Width, width);
            _canvas.Icon(PixelIcon.PrayerHands, x, ScoreTop, TitleScale, Color.white);
            _canvas.Text(blessings, x + bolt + ScoreGap, ScoreTop + 5, PixelPalette.Gold, TitleScale);
        }

        private void DrawStats(ResultsContent content)
        {
            for (var i = 0; i < content.Stats.Length; i++)
            {
                var line = content.Stats[i];
                var left = StatsLeft + i % StatColumns * StatColumnWidth;
                var y = StatsTop + i / StatColumns * StatStep;
                _canvas.Icon(line.Icon, left, y, 1, Color.white);
                _canvas.Text(line.Label, left + 16, y + 1, PixelPalette.Cream);
                var width = PixelTextLayout.Width(line.Value.Length, 1);
                _canvas.Text(line.Value, left + StatValueRight + 16 - width, y + 1, PixelPalette.Gold);
            }
        }

        private void DrawSpeech(ResultsContent content)
        {
            if (content.SpeechLines.Length == 0) return;
            var godPanel = content.Speaker.Length > 0;
            var top = godPanel ? SpeechPanel.Y + SpeechTextTop : TrainerTop;
            if (godPanel)
            {
                _canvas.Panel(SpeechPanel.X, SpeechPanel.Y, SpeechPanel.Width, SpeechPanel.Height, PixelPalette.Ink);
                _canvas.Text(content.Speaker + ":", SpeechPanel.X + SpeechPad, SpeechPanel.Y + SpeechNameTop, PixelPalette.Gold);
            }
            for (var i = 0; i < content.SpeechLines.Length; i++)
            {
                var line = content.SpeechLines[i];
                var x = godPanel ? SpeechPanel.X + SpeechPad : PixelTextLayout.Centered(0, PixelGrid.Width, PixelTextLayout.Width(line.Length, 1));
                _canvas.Text(line, x, top + i * SpeechStep, PixelPalette.Cream);
            }
        }

        private void DrawButtons(bool canRetry)
        {
            DrawButton(RetryButton, ResultsFormatter.RetryLabel, canRetry ? PixelPalette.Green : PixelPalette.Grey, canRetry ? PixelPalette.Ink : PixelPalette.Track);
            DrawButton(NextButton, ResultsFormatter.NextLotLabel, PixelPalette.Gold, PixelPalette.Ink);
            DrawCentered(canRetry ? KeyHint : NextOnlyHint, RetryButton.Y + RetryButton.Height + 4, 1, PixelPalette.Cream);
        }

        private void DrawButton(PixelRect rect, string label, Color fill, Color text)
        {
            _canvas.Panel(rect.X, rect.Y, rect.Width, rect.Height, fill);
            var width = PixelTextLayout.Width(label.Length, TitleScale);
            _canvas.Text(label, PixelTextLayout.Centered(rect.X, rect.Width, width),
                rect.Y + (rect.Height - PixelTextLayout.Height(TitleScale)) / 2, text, TitleScale, false);
        }

        private void DrawCentered(string text, int y, int scale, Color color)
        {
            var width = PixelTextLayout.Width(text.Length, scale);
            _canvas.Text(text, PixelTextLayout.Centered(0, PixelGrid.Width, width), y, color, scale);
        }
    }
}
