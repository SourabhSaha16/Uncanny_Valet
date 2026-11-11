using System;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// The freeze time look, drawn over the world on the 480x270 canvas: a cold blue wash with a clear bubble round the
    /// valet or the car he drives (they stay in full colour), then frost bands and ice spikes at the screen edge.
    /// Allocates nothing; draws nothing while the fade is 0.
    /// </summary>
    public sealed class FreezeVeilPainter
    {
        private readonly PixelCanvas _canvas;
        private readonly FreezeFade _fade;
        private IPlayerFocus _focus;

        public FreezeVeilPainter(PixelCanvas canvas, FreezeFade fade)
        {
            _canvas = canvas;
            _fade = fade;
        }

        /// <summary>Draws the wash and the frost for the current fade.</summary>
        public void Draw(Camera camera)
        {
            var strength = _fade.Alpha01;
            if (strength <= 0f) return;
            if (_focus == null) Services.TryGet(out _focus);
            DrawWash(strength, camera);
            DrawFrost(strength);
        }

        private void DrawWash(float strength, Camera camera)
        {
            var full = Tinted(FreezeLook.Wash, FreezeLook.WashAlpha * strength);
            if (camera == null || _focus == null)
            {
                _canvas.Fill(0, 0, PixelGrid.Width, PixelGrid.Height, full);
                return;
            }
            var screen = camera.WorldToScreenPoint(_focus.Position);
            var cx = _canvas.Grid.VirtualX(screen.x);
            var cy = _canvas.Grid.VirtualY(_canvas.ScreenHeight - screen.y);
            var reach = FreezeLook.BubbleRadiusY + FreezeLook.BubbleRim;
            var top = Math.Min(PixelGrid.Height, Math.Max(0, (cy - reach) / FreezeLook.RowHeight * FreezeLook.RowHeight));
            var bottom = Math.Min(PixelGrid.Height, Math.Max(top, (cy + reach) / FreezeLook.RowHeight * FreezeLook.RowHeight + FreezeLook.RowHeight));
            _canvas.Fill(0, 0, PixelGrid.Width, top, full);
            _canvas.Fill(0, bottom, PixelGrid.Width, PixelGrid.Height - bottom, full);
            var rim = Tinted(FreezeLook.Wash, FreezeLook.WashAlpha * FreezeLook.RimFraction * strength);
            for (var y = top; y < bottom; y += FreezeLook.RowHeight) DrawRow(y, cx, cy, full, rim);
        }

        private void DrawRow(int y, int cx, int cy, Color full, Color rim)
        {
            var offset = y + FreezeLook.RowHeight / 2 - cy;
            var inner = FreezeLook.BubbleHalfWidth(offset, FreezeLook.BubbleRadiusX, FreezeLook.BubbleRadiusY);
            var outer = FreezeLook.BubbleHalfWidth(offset, FreezeLook.BubbleRadiusX + FreezeLook.BubbleRim,
                FreezeLook.BubbleRadiusY + FreezeLook.BubbleRim);
            Span(0, cx - outer, y, full);
            Span(cx + outer, PixelGrid.Width, y, full);
            if (inner == 0) { Span(cx - outer, cx + outer, y, rim); return; }
            Span(cx - outer, cx - inner, y, rim);
            Span(cx + inner, cx + outer, y, rim);
        }

        private void Span(int from, int to, int y, Color color)
        {
            from = Math.Max(0, from);
            to = Math.Min(PixelGrid.Width, to);
            if (to > from) _canvas.Fill(from, y, to - from, FreezeLook.RowHeight, color);
        }

        private void DrawFrost(float strength)
        {
            var band = Tinted(FreezeLook.Frost, FreezeLook.BandAlpha * strength);
            var spike = Tinted(FreezeLook.Frost, FreezeLook.SpikeAlpha * strength);
            for (var i = 0; i < FreezeLook.BandCount; i++)
            {
                var inset = i * FreezeLook.BandThickness;
                var thick = FreezeLook.BandThickness;
                var length = PixelGrid.Width - inset * 2;
                var height = PixelGrid.Height - inset * 2 - thick * 2;
                _canvas.Fill(inset, inset, length, thick, band);
                _canvas.Fill(inset, PixelGrid.Height - inset - thick, length, thick, band);
                _canvas.Fill(inset, inset + thick, thick, height, band);
                _canvas.Fill(PixelGrid.Width - inset - thick, inset + thick, thick, height, band);
            }
            DrawSpikes(spike, strength);
        }

        private void DrawSpikes(Color color, float strength)
        {
            for (var x = 0; x < PixelGrid.Width; x += FreezeLook.SpikeSpacing)
            {
                var index = x / FreezeLook.SpikeSpacing;
                var top = Reach(FreezeLook.SpikeLength(index, 0), strength);
                var bottom = Reach(FreezeLook.SpikeLength(index, 1), strength);
                _canvas.Fill(x, 0, FreezeLook.SpikeWidth, top, color);
                _canvas.Fill(x, PixelGrid.Height - bottom, FreezeLook.SpikeWidth, bottom, color);
            }
            for (var y = 0; y < PixelGrid.Height; y += FreezeLook.SpikeSpacing)
            {
                var index = y / FreezeLook.SpikeSpacing;
                var left = Reach(FreezeLook.SpikeLength(index, 2), strength);
                var right = Reach(FreezeLook.SpikeLength(index, 3), strength);
                _canvas.Fill(0, y, left, FreezeLook.SpikeWidth, color);
                _canvas.Fill(PixelGrid.Width - right, y, right, FreezeLook.SpikeWidth, color);
            }
        }

        // Spikes grow in with the fade.
        private static int Reach(int length, float strength) => (int)(length * strength);

        private static Color Tinted(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);
    }
}
