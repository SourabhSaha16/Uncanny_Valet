using System.Collections.Generic;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Screens
{
    /// <summary>
    /// The how-to-play screen: a heading, bullet rules and controls on a panel, key words in gold, and a blinking
    /// PRESS ANY KEY. The wrapped lines are laid out once at construction, so drawing allocates nothing.
    /// </summary>
    public sealed class HowToPlayPainter
    {
        private const int PanelMargin = 10;
        private const int TextLeft = 26;
        private const int TextRight = 26;
        private const int BulletIndent = 10;
        private const int BodyScale = 1;
        private const int HeadingScale = 2;
        private const int HeadingTop = 20;
        private const int SectionGap = 14;
        private const int BulletGap = 4;
        private const int PromptTop = 236;
        private const int PromptScale = 2;
        private const float BlinkPeriod = 1f;
        private const string Bullet = "-";
        private static readonly Color Veil = new Color(0f, 0f, 0f, 0.6f);

        private readonly struct Row
        {
            public readonly string Text;
            public readonly bool[] Mask;
            public readonly WordWrap.Span Span;
            public readonly int Y;
            public readonly bool BulletMark;

            public Row(string text, bool[] mask, WordWrap.Span span, int y, bool bulletMark)
            {
                Text = text; Mask = mask; Span = span; Y = y; BulletMark = bulletMark;
            }
        }

        private readonly PixelCanvas _canvas;
        private readonly List<Row> _rows = new List<Row>();
        private readonly List<WordWrap.Span> _spans = new List<WordWrap.Span>();
        private readonly int _controlsHeadingY;

        public HowToPlayPainter(PixelCanvas canvas)
        {
            _canvas = canvas;
            var y = AddSection(HowToPlayText.Rules, HeadingTop + PixelTextLayout.LineHeight(HeadingScale) + BulletGap);
            _controlsHeadingY = y + SectionGap;
            AddSection(HowToPlayText.Controls, _controlsHeadingY + PixelTextLayout.LineHeight(HeadingScale) + BulletGap);
        }

        public void Draw(float time)
        {
            _canvas.Fill(0, 0, PixelGrid.Width, PixelGrid.Height, Veil);
            _canvas.Panel(PanelMargin, PanelMargin, PixelGrid.Width - 2 * PanelMargin, PixelGrid.Height - 2 * PanelMargin,
                PixelPalette.PanelDark);
            _canvas.Text(HowToPlayText.Heading, TextLeft, HeadingTop, PixelPalette.Gold, HeadingScale);
            _canvas.Text(HowToPlayText.ControlsHeading, TextLeft, _controlsHeadingY, PixelPalette.Gold, HeadingScale);
            for (var i = 0; i < _rows.Count; i++) DrawRow(_rows[i]);
            if (Blink.IsOn(time, BlinkPeriod)) DrawPrompt();
        }

        private int AddSection(string[] items, int y)
        {
            var maxChars = (PixelGrid.Width - TextLeft - TextRight - BulletIndent) / (PixelTextLayout.Advance * BodyScale);
            for (var i = 0; i < items.Length; i++)
            {
                _spans.Clear();
                WordWrap.Wrap(items[i], maxChars, _spans);
                var mask = HighlightMask.Build(items[i], HowToPlayText.Highlights);
                for (var s = 0; s < _spans.Count; s++)
                {
                    _rows.Add(new Row(items[i], mask, _spans[s], y, s == 0));
                    y += PixelTextLayout.LineHeight(BodyScale);
                }
                y += BulletGap;
            }
            return y;
        }

        private void DrawRow(Row row)
        {
            if (row.BulletMark) _canvas.Text(Bullet, TextLeft, row.Y, PixelPalette.Cream, BodyScale);
            var offset = PixelCanvas.ShadowOffset(BodyScale);
            for (var i = 0; i < row.Span.Length; i++)
            {
                var index = row.Span.Start + i;
                var x = TextLeft + BulletIndent + i * PixelTextLayout.Advance * BodyScale;
                _canvas.Char(row.Text[index], x + offset, row.Y + offset, PixelPalette.Ink, BodyScale);
                _canvas.Char(row.Text[index], x, row.Y, row.Mask[index] ? PixelPalette.Gold : PixelPalette.Cream, BodyScale);
            }
        }

        private void DrawPrompt()
        {
            var width = PixelTextLayout.Width(HowToPlayText.Prompt.Length, PromptScale);
            _canvas.Text(HowToPlayText.Prompt, PixelTextLayout.Centered(0, PixelGrid.Width, width), PromptTop,
                PixelPalette.Cream, PromptScale);
        }
    }
}
