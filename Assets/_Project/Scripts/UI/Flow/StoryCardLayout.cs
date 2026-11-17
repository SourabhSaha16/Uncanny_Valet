using System.Collections.Generic;
using UncannyValet.UI.Kit;
using UncannyValet.UI.Screens;
using UnityEngine;

namespace UncannyValet.UI.Flow
{
    /// <summary>Lays text onto a <see cref="StoryCard"/> from top to bottom: wraps each text to the panel's width and centres every line.</summary>
    public sealed class StoryCardLayout
    {
        /// <summary>Width of the icon box on a reward card, in canvas pixels.</summary>
        public const int IconSide = 36;

        private const int TextMargin = 28;
        private readonly StoryCard _card;
        private readonly List<WordWrap.Span> _spans = new List<WordWrap.Span>();
        private int _y;

        public StoryCardLayout(StoryCard card, int top)
        {
            _card = card;
            _y = top;
        }

        /// <summary>The next free row.</summary>
        public int Y => _y;

        /// <summary>The largest of the scales (counting down) at which the text fits one line; the smallest when none does.</summary>
        public static int FittingScale(string text, int largest, int smallest)
        {
            for (var scale = largest; scale > smallest; scale--)
                if (PixelTextLayout.Width(text.Length, scale) <= PixelGrid.Width - 2 * TextMargin) return scale;
            return smallest;
        }

        /// <summary>Adds the text after <paramref name="gapBefore"/> blank pixels, wrapped to the panel and centred.</summary>
        public void Add(string text, int scale, Color color, int gapBefore)
        {
            if (string.IsNullOrEmpty(text)) return;
            _y += gapBefore;
            var maxChars = (PixelGrid.Width - 2 * TextMargin) / (PixelTextLayout.Advance * scale);
            _spans.Clear();
            WordWrap.Wrap(text, maxChars, _spans);
            for (var i = 0; i < _spans.Count; i++)
            {
                var line = text.Substring(_spans[i].Start, _spans[i].Length);
                var x = PixelTextLayout.Centered(0, PixelGrid.Width, PixelTextLayout.Width(line.Length, scale));
                _card.Lines.Add(new CardLine(line, x, _y, scale, color));
                _y += PixelTextLayout.LineHeight(scale);
            }
        }

        /// <summary>Reserves a centred box for the power icon after <paramref name="gapBefore"/> blank pixels.</summary>
        public void AddIcon(UncannyValet.Core.Ids.StoryPower power, int gapBefore)
        {
            _y += gapBefore;
            _card.HasIcon = true;
            _card.IconPower = power;
            _card.IconX = PixelTextLayout.Centered(0, PixelGrid.Width, IconSide);
            _card.IconY = _y;
            _y += IconSide;
        }
    }
}
