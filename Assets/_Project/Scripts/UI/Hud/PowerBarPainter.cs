using UncannyValet.Core.Events;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Draws the power bar on the right edge of the 480x270 canvas: a thin vertical bar that fills continuously with three
    /// lightning-bolt icons over it, one per bucket (dim until the bucket opens, lit when usable, white flash when spent),
    /// and a countdown ring icon while freeze time runs. Nothing is drawn before Thor owns a power. Rectangles only.
    /// </summary>
    public sealed class PowerBarPainter
    {
        private const int BarWidth = 3;
        private const int BarHeight = 112;
        private const int BarTop = 46;
        private const int ColumnWidth = BoltGlyph.Width + 2;
        private const int IconSide = 13;
        private const int IconGap = 3;
        private const float GlowAlpha = 0.75f;
        private const int Buckets = 3;

        private readonly PixelCanvas _canvas;
        private readonly PowerBarTracker _tracker;

        public PowerBarPainter(PixelCanvas canvas, PowerBarTracker tracker)
        {
            _canvas = canvas;
            _tracker = tracker;
        }

        /// <summary>Draws the bar, the bolts and the active power icon; time is unscaled seconds.</summary>
        public void Draw(float time)
        {
            _tracker.Tick(time);
            if (!_tracker.IsVisible) return;
            var column = PixelGrid.Width - HudLayout.Margin - ColumnWidth;
            var barX = column + (ColumnWidth - BarWidth) / 2;
            DrawBar(barX);
            for (var bucket = 1; bucket <= Buckets; bucket++) DrawBolt(column + 1, bucket);
            DrawFreezeIcon(column - IconGap - IconSide);
        }

        private void DrawBar(int x)
        {
            var view = _tracker.View;
            _canvas.Fill(x - 1, BarTop - 1, BarWidth + 2, BarHeight + 2, PixelPalette.Ink);
            _canvas.Fill(x, BarTop, BarWidth, BarHeight, PixelPalette.Track);
            var filled = Mathf.RoundToInt(view.Fill * BarHeight);
            _canvas.Fill(x, BarTop + BarHeight - filled, BarWidth, filled, PowerColors.Gold);
            if (view.Pulse01 <= 0f) return;
            var glow = PowerColors.Gold;
            glow.a = view.Pulse01 * GlowAlpha;
            _canvas.Fill(x - 1, BarTop - 1, 1, BarHeight + 2, glow);
            _canvas.Fill(x + BarWidth, BarTop - 1, 1, BarHeight + 2, glow);
        }

        private void DrawBolt(int x, int bucket)
        {
            var segment = BarHeight / Buckets;
            var centre = BarTop + BarHeight - (bucket * segment - segment / 2);
            BoltGlyph.Draw(_canvas, x, centre - BoltGlyph.Height / 2, BoltColor(bucket));
        }

        private Color BoltColor(int bucket)
        {
            var view = _tracker.View;
            var open = view.OpenBuckets >= bucket;
            var color = open ? PowerColors.Gold : PowerColors.Dimmed(PowerColors.Gold);
            if (view.SpentBucket == bucket && view.Flash01 > 0f) color = Color.Lerp(color, Color.white, view.Flash01);
            return color;
        }

        private void DrawFreezeIcon(int x)
        {
            const PowerKind freeze = PowerKind.FreezeTime;
            if (!_tracker.Active.IsActive(freeze)) return;
            var y = BarTop + BarHeight - IconSide;
            _canvas.Fill(x, y, IconSide, IconSide, PixelPalette.Track);
            var lit = SquareRing.Lit(IconSide, _tracker.Active.Remaining01(freeze));
            for (var i = 0; i < lit; i++)
            {
                SquareRing.Cell(IconSide, i, out var dx, out var dy);
                _canvas.Fill(x + dx, y + dy, 1, 1, PowerColors.IceBlue);
            }
            _canvas.Char('F', x + (IconSide - PixelGlyphs.Width) / 2, y + (IconSide - PixelGlyphs.Height) / 2, PowerColors.IceBlue, 1);
        }
    }
}
