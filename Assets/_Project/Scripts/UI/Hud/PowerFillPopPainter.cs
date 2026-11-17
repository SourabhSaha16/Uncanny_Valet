using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Draws the "+X" bolt pops left of the power icons, half way up the bar: the text, then the lightning bolt, rising and fading.
    /// Pops arriving together are stacked by slot (see <see cref="PowerFillPops"/>). Draws only while the bar itself is shown.
    /// </summary>
    public sealed class PowerFillPopPainter
    {
        private const int BoltGap = 2;

        private readonly PixelCanvas _canvas;
        private readonly PowerFillPops _pops;
        private readonly PowerFillPopSettings _settings;
        private readonly PowerBarTracker _bar;

        public PowerFillPopPainter(PixelCanvas canvas, PowerFillPops pops, PowerFillPopSettings settings, PowerBarTracker bar)
        {
            _canvas = canvas;
            _pops = pops;
            _settings = settings;
            _bar = bar;
        }

        public void Draw(float now)
        {
            _pops.Expire(now);
            if (!_bar.IsVisible) return;
            for (var i = 0; i < _pops.Capacity; i++)
            {
                var pop = _pops[i];
                if (pop.Active) DrawPop(pop, now - pop.StartedAt);
            }
        }

        private void DrawPop(PowerFillPops.Pop pop, float age)
        {
            var scale = _settings.FontScale;
            var textWidth = PixelTextLayout.Width(pop.Text.Length, scale);
            var rise = Mathf.RoundToInt(_settings.RisePixels * BlessingPopText.Rise(age, _settings.DurationSeconds));
            var right = PixelGrid.Width - HudLayout.Margin - PowerBarPainter.ColumnWidth - PowerBarPainter.IconGap
                - PowerBarPainter.IconSide - _settings.GapPixels;
            var x = right - (textWidth + BoltGap + BoltGlyph.Width);
            var y = PowerBarPainter.BarTop + PowerBarPainter.BarHeight / 2 + pop.Slot * _settings.SlotSpacingPixels - rise;
            var alpha = BlessingPopText.Alpha(age, _settings.DurationSeconds, _settings.HoldFraction);
            var color = PowerFillPopRule.ColorOf(pop.Source, _settings.GoldPull);
            var shadow = PixelCanvas.ShadowOffset(scale);
            var textY = y + (BoltGlyph.Height - PixelTextLayout.Height(scale)) / 2;
            _canvas.Text(pop.Text, x + shadow, textY + shadow, WithAlpha(PixelPalette.Ink, alpha), scale, false);
            _canvas.Text(pop.Text, x, textY, WithAlpha(color, alpha), scale, false);
            BoltGlyph.Draw(_canvas, x + textWidth + BoltGap, y, color, alpha);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
