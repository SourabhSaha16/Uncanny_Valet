using System.Collections.Generic;
using UncannyValet.UI.Kit;
using UncannyValet.UI.Screens;
using UnityEngine;

namespace UncannyValet.UI.Speech
{
    /// <summary>
    /// Draws the current speech bubble in the pixel style: a cream panel with the speaker's name over the line, and a small tail pointing
    /// down at the speaker. Projected from the speaker's world position so it shares the pixel scale; clamped to stay on the canvas.
    /// </summary>
    public sealed class SpeechBubblePainter
    {
        private static readonly Color Ink = PixelPalette.Ink;
        private static readonly Color NameColor = PixelPalette.PanelDark;
        private static readonly Color Cream = PixelPalette.PanelCream;

        private readonly PixelCanvas _canvas;
        private readonly SpeechBubbleTracker _tracker;
        private readonly float _scale;
        private readonly float _viewMargin;

        public SpeechBubblePainter(PixelCanvas canvas, SpeechBubbleTracker tracker, SpeechBubbleSettings settings = null)
        {
            _canvas = canvas;
            _tracker = tracker;
            _scale = settings != null ? settings.Scale : SpeechBubbleSettings.DefaultScale;
            _viewMargin = settings != null ? settings.ViewMargin : SpeechBubbleSettings.DefaultViewMargin;
        }

        public void Draw(Camera camera, float now)
        {
            if (camera == null || !_tracker.TryGetCurrent(now, out var request)) return;
            var view = camera.WorldToViewportPoint(request.CurrentPosition);
            if (!SpeechBubbleVisibility.IsInView(view.x, view.y, view.z, _viewMargin)) return;
            var screen = camera.WorldToScreenPoint(request.CurrentPosition + Vector2.up * SpeechBubbleLayout.LiftWorldUnits);
            if (screen.z < 0f) return;
            var lines = _tracker.Lines;
            var longest = 0;
            for (var i = 0; i < lines.Count; i++) longest = Mathf.Max(longest, lines[i].Length);
            var width = SpeechBubbleLayout.Width(request.Speaker.Length, longest);
            var height = SpeechBubbleLayout.Height(Mathf.Max(1, lines.Count));
            var tipX = _canvas.Grid.VirtualX(screen.x);
            var tipY = _canvas.Grid.VirtualY(_canvas.ScreenHeight - screen.y);
            var x = SpeechBubbleLayout.Clamp(tipX - width / 2, width, PixelGrid.Width);
            var y = SpeechBubbleLayout.Clamp(tipY - SpeechBubbleLayout.TailHeight - height, height, PixelGrid.Height);
            _canvas.BeginZoom(x + width / 2, y + height, _scale); // shrinks towards the tail, so the box stays inside what was clamped
            Paint(request.Speaker, request.Text, lines, x, y, width, height, tipX);
            _canvas.EndZoom();
        }

        private void Paint(string speaker, string text, IReadOnlyList<WordWrap.Span> lines, int x, int y, int width, int height, int tipX)
        {
            _canvas.Panel(x, y, width, height, Cream);
            DrawTail(Mathf.Clamp(tipX, x + SpeechBubbleLayout.Padding, x + width - SpeechBubbleLayout.Padding), y + height);
            var left = x + SpeechBubbleLayout.Padding;
            _canvas.Text(speaker, left, y + SpeechBubbleLayout.Padding, NameColor, 1, false);
            var top = y + SpeechBubbleLayout.Padding + PixelGlyphs.Height + SpeechBubbleLayout.NameGap;
            for (var i = 0; i < lines.Count; i++) DrawLine(text, lines[i], left, top + i * PixelTextLayout.LineHeight(1));
        }

        // One glyph at a time, so no substring is allocated per frame.
        private void DrawLine(string text, WordWrap.Span span, int left, int y)
        {
            for (var j = 0; j < span.Length; j++) _canvas.Char(text[span.Start + j], left + j * PixelTextLayout.Advance, y, Ink, 1);
        }

        // A little stepped triangle under the panel: cream rows with an ink edge on each side.
        private void DrawTail(int centerX, int top)
        {
            for (var row = 0; row < SpeechBubbleLayout.TailHeight; row++)
            {
                var half = SpeechBubbleLayout.TailHeight - 1 - row;
                _canvas.Fill(centerX - half - 1, top - 1 + row, half * 2 + 3, 1, Ink);
                _canvas.Fill(centerX - half, top - 1 + row, half * 2 + 1, 1, Cream);
            }
        }
    }
}
