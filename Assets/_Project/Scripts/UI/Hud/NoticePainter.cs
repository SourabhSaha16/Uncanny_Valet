using System.Text;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Transient notices: the cyan pickup banner (which car, which spot) under the clock, the interact prompt
    /// at the bottom centre and the small tuning hint at the bottom right.
    /// </summary>
    public sealed class NoticePainter
    {
        public const string TuningHint = "F2 TUNING";

        private const int MaxPickupLines = 3;
        private const int BannerTop = 28;
        private const int BellScale = 2;
        private const int BannerPad = 5;
        private const int PromptHeight = 18;
        private const int PromptBottom = 8;
        private const int KeyCapSize = 11;
        private const int HintMargin = 4;
        private const float BlinkPeriod = 0.6f;
        private const float HintAlpha = 0.7f;

        private readonly PixelCanvas _canvas;
        private readonly TicketBoard _board;
        private readonly StringBuilder _pickups = new StringBuilder(96);
        private int _pickupLines;
        private int _boardVersion = -1;

        public NoticePainter(PixelCanvas canvas, TicketBoard board)
        {
            _canvas = canvas;
            _board = board;
        }

        public void Draw(string prompt, float time)
        {
            if (_board.PickupCount > 0) DrawBanner(time);
            if (!string.IsNullOrEmpty(prompt)) DrawPrompt(prompt);
            DrawHint();
        }

        private void DrawBanner(float time)
        {
            if (_boardVersion != _board.Version)
            {
                _boardVersion = _board.Version;
                _pickups.Clear();
                _pickupLines = _board.AppendPickups(_pickups, MaxPickupLines);
            }
            var textWidth = PixelTextLayout.Width(PixelTextLayout.LongestLine(_pickups), 1);
            var bellWidth = _canvas.IconWidth(PixelIcon.Bell) * BellScale;
            var width = BannerPad * 3 + bellWidth + textWidth + WorldTicketLayout.Border;
            var textHeight = PixelTextLayout.LineHeight(1) * _pickupLines;
            var height = System.Math.Max(bellWidth, textHeight) + WorldTicketLayout.Border * 2 + BannerPad;
            var x = (PixelGrid.Width - width) / 2;
            var fill = Blink.IsOn(time, BlinkPeriod) ? PixelPalette.PanelCyanBright : PixelPalette.PanelCyan;
            _canvas.Panel(x, BannerTop, width, height, fill);
            var iconY = BannerTop + (height - bellWidth) / 2;
            _canvas.Icon(PixelIcon.Bell, x + BannerPad + 1, iconY, BellScale, Color.white);
            var textY = BannerTop + (height - textHeight) / 2 + 1;
            _canvas.Text(_pickups, x + BannerPad * 2 + bellWidth, textY, PixelPalette.White);
        }

        private void DrawPrompt(string prompt)
        {
            var textWidth = PixelTextLayout.Width(prompt.Length, 1);
            var width = KeyCapSize + BannerPad * 3 + textWidth;
            var x = (PixelGrid.Width - width) / 2;
            var y = PixelGrid.Height - PromptBottom - PromptHeight;
            _canvas.Panel(x, y, width, PromptHeight, PixelPalette.PanelDark);
            _canvas.Fill(x + BannerPad, y + 3, KeyCapSize, KeyCapSize, PixelPalette.Cream);
            _canvas.Char('E', x + BannerPad + 3, y + 5, PixelPalette.Ink, 1);
            _canvas.Text(prompt, x + BannerPad * 2 + KeyCapSize, y + 5, PixelPalette.Cream);
        }

        private void DrawHint()
        {
            var color = PixelPalette.Cream;
            color.a = HintAlpha;
            var width = PixelTextLayout.Width(TuningHint.Length, 1);
            _canvas.Text(TuningHint, PixelGrid.Width - HintMargin - width, PixelGrid.Height - HintMargin - PixelGlyphs.Height,
                color, 1, false);
        }
    }
}
