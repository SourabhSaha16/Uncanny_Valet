using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Screens
{
    /// <summary>
    /// The title screen from the style guide's Scene 13: UNCANNY VALET in big chunky letters over the (still) lot,
    /// a row of key tags, and a blinking PRESS ANY KEY. Mrs. Okafor and the cast are left for real art.
    /// </summary>
    public sealed class TitlePainter
    {
        public const string Line1 = "UNCANNY";
        public const string Line2 = "VALET";
        public const string Prompt = "PRESS ANY KEY";

        private const int TitleScale = 7;
        private const int Line1Top = 52;
        private const int Line2Top = 106;
        private const int TagScale = 3;
        private const int TagTop = 160;
        private const int TagCount = 5;
        private const int TagStep = 28;
        private const int PromptTop = 224;
        private const int PromptScale = 2;
        private const float BlinkPeriod = 1f;
        private static readonly Color Veil = new Color(0f, 0f, 0f, 0.35f);

        private readonly PixelCanvas _canvas;

        public TitlePainter(PixelCanvas canvas)
        {
            _canvas = canvas;
        }

        public void Draw(float time)
        {
            _canvas.Fill(0, 0, PixelGrid.Width, PixelGrid.Height, Veil);
            DrawCentered(Line1, Line1Top, TitleScale, PixelPalette.Gold);
            DrawCentered(Line2, Line2Top, TitleScale, PixelPalette.OrangeRed);
            DrawTags();
            if (Blink.IsOn(time, BlinkPeriod)) DrawCentered(Prompt, PromptTop, PromptScale, PixelPalette.Cream);
        }

        private void DrawTags()
        {
            var size = _canvas.IconWidth(PixelIcon.KeyTag) * TagScale;
            var left = (PixelGrid.Width - ((TagCount - 1) * TagStep + size)) / 2;
            for (var i = 0; i < TagCount; i++)
                _canvas.Icon(PixelIcon.KeyTag, left + i * TagStep, TagTop, TagScale, PixelPalette.KeyTag(i + 1));
        }

        private void DrawCentered(string text, int y, int scale, Color color)
        {
            var width = PixelTextLayout.Width(text.Length, scale);
            _canvas.Text(text, PixelTextLayout.Centered(0, PixelGrid.Width, width), y, color, scale);
        }
    }
}
