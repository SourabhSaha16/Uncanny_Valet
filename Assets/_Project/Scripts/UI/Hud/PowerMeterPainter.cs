using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Draws the power meter on the right edge of the 480x270 canvas: a vertical bar with three coloured notches that
    /// pulses when it fills, a star at the level reached, and a letter icon with a countdown ring per active power.
    /// Rectangles and glyphs only, so nothing is allocated and no art is needed.
    /// </summary>
    public sealed class PowerMeterPainter
    {
        private const int BarWidth = 10;
        private const int BarHeight = 112;
        private const int BarTop = 46;
        private const int IconSide = 13;
        private const int IconGap = 3;
        private const float ReadyBlinkPeriod = 0.5f;
        private const float GlowAlpha = 0.75f;

        private static readonly char[] Letters = { 'S', 'F', 'P' };

        private readonly PixelCanvas _canvas;
        private readonly PowerMeterTracker _tracker;
        private readonly PowerMeterSettings _settings;

        public PowerMeterPainter(PixelCanvas canvas, PowerMeterTracker tracker, PowerMeterSettings settings)
        {
            _canvas = canvas;
            _tracker = tracker;
            _settings = settings;
        }

        /// <summary>Draws the meter and the active power icons; time is unscaled seconds.</summary>
        public void Draw(float time)
        {
            _tracker.Tick(time);
            var x = PixelGrid.Width - HudLayout.Margin - BarWidth;
            DrawBar(x, time);
            DrawIcons(x - IconGap - IconSide);
        }

        private void DrawBar(int x, float time)
        {
            var view = _tracker.View;
            var innerH = BarHeight - 2;
            var bottom = BarTop + BarHeight - 1;
            _canvas.Fill(x, BarTop, BarWidth, BarHeight, PixelPalette.Ink);
            _canvas.Fill(x + 1, BarTop + 1, BarWidth - 2, innerH, PixelPalette.Track);
            DrawFill(x + 1, bottom, innerH, view.Fill);
            for (var level = 1; level <= 3; level++) DrawNotch(x, bottom, innerH, level, time);
            DrawGlow(x, view.Pulse01, PowerColors.ForLevel(Mathf.Max(view.Level, 1)));
        }

        private void DrawFill(int x, int bottom, int innerH, float fill)
        {
            var filled = Mathf.RoundToInt(fill * innerH);
            var from = 0;
            for (var level = 1; level <= 3 && from < filled; level++)
            {
                var to = Mathf.Min(filled, Mathf.RoundToInt(_settings.NotchFill(level) * innerH));
                if (to <= from) continue;
                _canvas.Fill(x, bottom - to, BarWidth - 2, to - from, PowerColors.ForLevel(level));
                from = to;
            }
        }

        private void DrawNotch(int x, int bottom, int innerH, int level, float time)
        {
            var y = bottom - Mathf.RoundToInt(_settings.NotchFill(level) * innerH);
            var reached = _tracker.View.Level >= level;
            var color = reached ? PowerColors.ForLevel(level) : PowerColors.Dimmed(level);
            _canvas.Fill(x - 2, y, BarWidth + 4, 2, PixelPalette.Ink);
            _canvas.Fill(x - 1, y, BarWidth + 2, 1, color);
            if (_tracker.View.Level != level) return;
            var lit = _tracker.View.Ready01 > 0f || Blink.IsOn(time, ReadyBlinkPeriod);
            var star = lit ? PixelIcon.StarLit : PixelIcon.StarUnlit;
            _canvas.Icon(star, x - 2 - _canvas.IconWidth(star), y - _canvas.IconHeight(star) / 2, 1, color);
        }

        private void DrawGlow(int x, float pulse01, Color color)
        {
            if (pulse01 <= 0f) return;
            color.a = pulse01 * GlowAlpha;
            _canvas.Fill(x - 1, BarTop - 1, BarWidth + 2, 1, color);
            _canvas.Fill(x - 1, BarTop + BarHeight, BarWidth + 2, 1, color);
            _canvas.Fill(x - 1, BarTop, 1, BarHeight, color);
            _canvas.Fill(x + BarWidth, BarTop, 1, BarHeight, color);
        }

        private void DrawIcons(int x)
        {
            var slot = 0;
            for (var power = 1; power <= ActivePowers.Count; power++)
            {
                if (!_tracker.Active.IsActive(power)) continue;
                DrawIcon(x, BarTop + BarHeight - IconSide - slot * (IconSide + IconGap), power);
                slot++;
            }
        }

        private void DrawIcon(int x, int y, int power)
        {
            var color = PowerColors.ForLevel(power);
            _canvas.Fill(x, y, IconSide, IconSide, PixelPalette.Track);
            var lit = SquareRing.Lit(IconSide, _tracker.Active.Remaining01(power));
            for (var i = 0; i < lit; i++)
            {
                SquareRing.Cell(IconSide, i, out var dx, out var dy);
                _canvas.Fill(x + dx, y + dy, 1, 1, color);
            }
            var glyphX = x + (IconSide - PixelGlyphs.Width) / 2;
            _canvas.Char(Letters[power - 1], glyphX, y + (IconSide - PixelGlyphs.Height) / 2, color, 1);
        }
    }
}
