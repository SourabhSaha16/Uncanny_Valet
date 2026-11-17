using UncannyValet.UI.Hud;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Flow
{
    /// <summary>
    /// Draws a <see cref="StoryCard"/> on the 480x270 canvas in the how-to-play screen's style: the dark panel, an accent bar on top and the
    /// world's second accent below, the laid-out lines, the power icon of a reward card and a blinking prompt.
    /// </summary>
    public sealed class StoryCardPainter
    {
        private const int PanelMargin = 10;
        private const int BarInset = 16;
        private const int BarHeight = 3;
        private const int BarTop = 16;
        private const int BarBottom = 250;
        private const int PromptTop = 234;
        private const int PromptScale = 2;
        private const int IconBorder = 2;
        private const int IconLetterScale = 3;
        private const float BlinkPeriod = 1f;
        private static readonly Color Veil = new Color(0f, 0f, 0f, 0.6f);

        private readonly PixelCanvas _canvas;

        public StoryCardPainter(PixelCanvas canvas)
        {
            _canvas = canvas;
        }

        public void Draw(StoryCard card, float time)
        {
            _canvas.Fill(0, 0, PixelGrid.Width, PixelGrid.Height, Veil);
            _canvas.Panel(PanelMargin, PanelMargin, PixelGrid.Width - 2 * PanelMargin, PixelGrid.Height - 2 * PanelMargin, PixelPalette.PanelDark);
            _canvas.Fill(BarInset, BarTop, PixelGrid.Width - 2 * BarInset, BarHeight, card.Accent);
            _canvas.Fill(BarInset, BarBottom, PixelGrid.Width - 2 * BarInset, BarHeight, card.AccentSecond);
            for (var i = 0; i < card.Lines.Count; i++) DrawLine(card.Lines[i]);
            if (card.HasIcon) DrawIcon(card);
            if (!Blink.IsOn(time, BlinkPeriod)) return;
            var width = PixelTextLayout.Width(card.Prompt.Length, PromptScale);
            _canvas.Text(card.Prompt, PixelTextLayout.Centered(0, PixelGrid.Width, width), PromptTop, PixelPalette.Cream, PromptScale);
        }

        private void DrawLine(CardLine line) => _canvas.Text(line.Text, line.X, line.Y, line.Color, line.Scale);

        // The power icon as on the power bar (a dark box with the power's letter in its colour), big.
        private void DrawIcon(StoryCard card)
        {
            var color = PowerColors.Of(card.IconPower);
            var side = StoryCardLayout.IconSide;
            _canvas.Fill(card.IconX, card.IconY, side, side, color);
            _canvas.Fill(card.IconX + IconBorder, card.IconY + IconBorder, side - 2 * IconBorder, side - 2 * IconBorder, PixelPalette.Track);
            var letter = PowerColors.LetterOf(card.IconPower);
            var x = card.IconX + (side - PixelGlyphs.Width * IconLetterScale) / 2;
            var y = card.IconY + (side - PixelGlyphs.Height * IconLetterScale) / 2;
            _canvas.Char(letter, x, y, color, IconLetterScale);
        }
    }
}
