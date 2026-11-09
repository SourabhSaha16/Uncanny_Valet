using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Transient notices: the interact prompt at the bottom centre and the small tuning hint at the bottom right.
    /// Pickups have no banner: the key board row of the car rings a bell instead.
    /// </summary>
    public sealed class NoticePainter
    {
        public const string TuningHint = "F2 TUNING";

        private const int BannerPad = 5;
        private const int PromptHeight = 18;
        private const int PromptBottom = 8;
        private const int KeyCapSize = 11;
        private const int HintMargin = 4;
        private const float HintAlpha = 0.7f;

        private readonly PixelCanvas _canvas;

        public NoticePainter(PixelCanvas canvas)
        {
            _canvas = canvas;
        }

        public void Draw(string prompt)
        {
            if (!string.IsNullOrEmpty(prompt)) DrawPrompt(prompt);
            DrawHint();
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
