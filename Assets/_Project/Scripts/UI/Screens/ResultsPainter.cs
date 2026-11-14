using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Screens
{
    /// <summary>
    /// The results screen: a dimmed lot, one big panel, the verdict (CLEARED or FAILED), the blessings against the
    /// threshold with the bolt, stats lines, and Retry and Next Lot buttons (the god's verdict panel is task 85's: it replaces or shrinks the stats block, which is the one place with room).
    /// </summary>
    public sealed class ResultsPainter
    {
        /// <summary>The Retry button, in canvas pixels. Clicks are tested against it.</summary>
        public static readonly PixelRect RetryButton = new PixelRect(96, 224, 128, 24);

        /// <summary>The Next Lot button, in canvas pixels. Clicks are tested against it.</summary>
        public static readonly PixelRect NextButton = new PixelRect(256, 224, 128, 24);
        private static readonly PixelRect Card = new PixelRect(48, 12, 384, 252);

        private const string KeyHint = "ENTER: RETRY   ANY OTHER KEY: NEXT LOT";
        private const int TitleScale = 2;
        private const int VerdictScale = 4;
        private const int VerdictTop = 44;
        private const int HeadingTop = 82;
        private const int ScoreTop = 92;
        private const int ScoreGap = 6;
        private const int StatsLeft = 76;
        private const int StatsTop = 124;
        private const int StatsRight = 292;
        private const int StatStep = 16;
        private const int GoalTop = 200;

        private readonly PixelCanvas _canvas;

        public ResultsPainter(PixelCanvas canvas)
        {
            _canvas = canvas;
        }

        public void Draw(ResultsContent content)
        {
            _canvas.Fill(0, 0, PixelGrid.Width, PixelGrid.Height, PixelPalette.Dim);
            _canvas.Panel(Card.X, Card.Y, Card.Width, Card.Height, PixelPalette.PanelDark);
            DrawCentered(content.Title, Card.Y + 12, TitleScale, PixelPalette.Gold);
            DrawCentered(content.Verdict, VerdictTop, VerdictScale, content.Cleared ? PixelPalette.Green : PixelPalette.OrangeRed);
            DrawCentered(ResultsFormatter.BlessingsHeading, HeadingTop, 1, PixelPalette.Cream);
            DrawBlessings(content.Blessings);
            DrawStats(content);
            DrawCentered(content.Goal, GoalTop, 1, PixelPalette.Cream);
            DrawButtons();
        }

        private void DrawBlessings(string blessings)
        {
            var bolt = _canvas.IconWidth(PixelIcon.Bolt) * TitleScale;
            var width = bolt + ScoreGap + PixelTextLayout.Width(blessings.Length, TitleScale);
            var x = PixelTextLayout.Centered(0, PixelGrid.Width, width);
            _canvas.Icon(PixelIcon.Bolt, x, ScoreTop, TitleScale, Color.white);
            _canvas.Text(blessings, x + bolt + ScoreGap, ScoreTop + 5, PixelPalette.Gold, TitleScale);
        }

        private void DrawStats(ResultsContent content)
        {
            for (var i = 0; i < content.Stats.Length; i++)
            {
                var line = content.Stats[i];
                var y = StatsTop + i * StatStep;
                _canvas.Icon(line.Icon, StatsLeft, y, 1, Color.white);
                _canvas.Text(line.Label, StatsLeft + 16, y + 1, PixelPalette.Cream);
                var width = PixelTextLayout.Width(line.Value.Length, 1);
                _canvas.Text(line.Value, StatsRight - width, y + 1, PixelPalette.Gold);
            }
        }

        private void DrawButtons()
        {
            DrawButton(RetryButton, ResultsFormatter.RetryLabel, PixelPalette.Green, PixelPalette.Ink);
            DrawButton(NextButton, ResultsFormatter.NextLotLabel, PixelPalette.Gold, PixelPalette.Ink);
            DrawCentered(KeyHint, RetryButton.Y + RetryButton.Height + 4, 1, PixelPalette.Cream);
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
