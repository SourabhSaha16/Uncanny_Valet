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

        public SpeechBubblePainter(PixelCanvas canvas, SpeechBubbleTracker tracker)
        {
            _canvas = canvas;
            _tracker = tracker;
        }

        public void Draw(Camera camera, float now)
        {
            if (camera == null || !_tracker.TryGetCurrent(now, out var request)) return;
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
            _canvas.Panel(x, y, width, height, Cream);
            DrawTail(Mathf.Clamp(tipX, x + SpeechBubbleLayout.Padding, x + width - SpeechBubbleLayout.Padding), y + height);
            var left = x + SpeechBubbleLayout.Padding;
            _canvas.Text(request.Speaker, left, y + SpeechBubbleLayout.Padding, NameColor, 1, false);
            var top = y + SpeechBubbleLayout.Padding + PixelGlyphs.Height + SpeechBubbleLayout.NameGap;
            for (var i = 0; i < lines.Count; i++) DrawLine(request.Text, lines[i], left, top + i * PixelTextLayout.LineHeight(1));
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
