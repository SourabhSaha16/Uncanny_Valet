using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Screens
{
    /// <summary>
    /// The results screen from the style guide's Scene 11: a dimmed lot, one big panel, three stamped stars,
    /// total tips with a coin, stats lines, a placeholder box for Mrs. Okafor, and Retry / Next Lot buttons
    /// (Next Lot is disabled and marked Soon because there is only one lot).
    /// </summary>
    public sealed class ResultsPainter
    {
        /// <summary>The Retry button, in canvas pixels. Clicks are tested against it.</summary>
        public static readonly PixelRect RetryButton = new PixelRect(96, 224, 128, 24);

        private static readonly PixelRect NextButton = new PixelRect(256, 224, 128, 24);
        private static readonly PixelRect Card = new PixelRect(48, 12, 384, 252);
        private static readonly PixelRect BossBox = new PixelRect(324, 84, 88, 100);

        private const string RetryHint = "ENTER TO RETRY";
        private const int TitleScale = 2;
        private const int StarScale = 4;
        private const int StarStep = 44;
        private const int StarsTop = 40;
        private const int ScoreTop = 92;
        private const int StatsLeft = 76;
        private const int StatsTop = 124;
        private const int StatsRight = 292;
        private const int StatStep = 16;
        private const int BurstRadius = 26;
        private const int BurstGrow = 22;
        private const int GoalTop = 200;

        private static readonly int[] DirX = { 1, 1, 0, -1, -1, -1, 0, 1 };
        private static readonly int[] DirY = { 0, 1, 1, 1, 0, -1, -1, -1 };

        private readonly PixelCanvas _canvas;

        public ResultsPainter(PixelCanvas canvas)
        {
            _canvas = canvas;
        }

        public void Draw(ResultsContent content, float secondsOpen)
        {
            _canvas.Fill(0, 0, PixelGrid.Width, PixelGrid.Height, PixelPalette.Dim);
            _canvas.Panel(Card.X, Card.Y, Card.Width, Card.Height, PixelPalette.PanelDark);
            DrawCentered(content.Title, Card.Y + 12, TitleScale, PixelPalette.Gold);
            for (var star = 0; star < ResultsFormatter.MaxStars; star++) DrawStar(star, content.Stars, secondsOpen);
            DrawScore(content.Score);
            DrawStats(content);
            DrawBoss();
            DrawCentered(content.Goal, GoalTop, 1, PixelPalette.Cream);
            DrawButtons();
        }

        private void DrawStar(int star, int earned, float seconds)
        {
            var centerX = PixelGrid.Width / 2 + (star - 1) * StarStep;
            var size = _canvas.IconWidth(PixelIcon.StarLit);
            var centerY = StarsTop + size * StarScale / 2;
            _canvas.Icon(PixelIcon.StarUnlit, centerX - size * StarScale / 2, StarsTop, StarScale, Color.white);
            if (star >= earned) return;
            var scale = Mathf.RoundToInt(StarScale * StarStamp.Scale(star, seconds));
            if (scale > 0)
                _canvas.Icon(PixelIcon.StarLit, centerX - size * scale / 2, centerY - size * scale / 2, scale, Color.white);
            var burst = StarStamp.BurstProgress(star, earned, seconds);
            if (burst >= 0f) DrawBurst(centerX, centerY, burst);
        }

        private void DrawBurst(int cx, int cy, float progress)
        {
            var radius = BurstRadius + (int)(progress * BurstGrow);
            var color = PixelPalette.Gold;
            color.a = 1f - progress;
            for (var i = 0; i < DirX.Length; i++)
            {
                var diagonal = DirX[i] != 0 && DirY[i] != 0;
                var d = diagonal ? radius * 7 / 10 : radius;
                _canvas.Fill(cx + DirX[i] * d - 1, cy + DirY[i] * d - 1, 3, 3, color);
            }
        }

        private void DrawScore(string score)
        {
            var coin = _canvas.IconWidth(PixelIcon.Coin) * TitleScale;
            var width = coin + 6 + PixelTextLayout.Width(score.Length, TitleScale);
            var x = PixelTextLayout.Centered(0, PixelGrid.Width, width);
            _canvas.Icon(PixelIcon.Coin, x, ScoreTop, TitleScale, Color.white);
            _canvas.Text(score, x + coin + 6, ScoreTop + 1, PixelPalette.Gold, TitleScale);
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

        private void DrawBoss()
        {
            _canvas.Panel(BossBox.X, BossBox.Y, BossBox.Width, BossBox.Height, PixelPalette.PanelCream);
            var width = PixelTextLayout.Width(ResultsFormatter.BossName.Length, 1);
            _canvas.Text(ResultsFormatter.BossName, PixelTextLayout.Centered(BossBox.X, BossBox.Width, width),
                BossBox.Y + BossBox.Height / 2 - 3, PixelPalette.Ink, 1, false);
        }

        private void DrawButtons()
        {
            DrawButton(RetryButton, ResultsFormatter.RetryLabel, PixelPalette.Green, PixelPalette.Ink);
            DrawButton(NextButton, ResultsFormatter.NextLotLabel, PixelPalette.Grey, PixelPalette.Track);
            var tag = PixelTextLayout.Width(ResultsFormatter.SoonLabel.Length, 1);
            _canvas.Text(ResultsFormatter.SoonLabel, NextButton.X + NextButton.Width - tag - 6, NextButton.Y - 10,
                PixelPalette.Amber);
            DrawCentered(RetryHint, RetryButton.Y + RetryButton.Height + 4, 1, PixelPalette.Cream);
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
