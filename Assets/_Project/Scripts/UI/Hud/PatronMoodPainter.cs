using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Mood;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// A small chat bubble over each patron waiting at the curb for a requested pickup: green happy, yellow impatient, red angry
    /// (the same <see cref="EmotionStyle"/> as the plate flash). It follows the walking patron through <see cref="IPatronLocator"/>
    /// and disappears with the ticket or when the patron stops waiting (boards the car).
    /// </summary>
    public sealed class PatronMoodPainter
    {
        private const float LiftWorldUnits = 0.7f;

        private readonly PixelCanvas _canvas;
        private readonly EmotionStyle _style;
        private IPatronLocator _patrons;

        public PatronMoodPainter(PixelCanvas canvas, EmotionStyle style)
        {
            _canvas = canvas;
            _style = style;
        }

        public void Draw(List<TicketRow> rows, Camera camera)
        {
            if (camera == null) return;
            if (_patrons == null && !Services.TryGet(out _patrons)) return;
            for (var i = 0; i < rows.Count; i++)
                if (PickupMark.IsRequested(rows[i]) && _patrons.TryGetWaitingPatron(new CarId(rows[i].Car), out var world))
                    DrawBubble(camera, world, _style.Rule.Resolve(rows[i]));
        }

        private void DrawBubble(Camera camera, Vector2 world, Emotion emotion)
        {
            var screen = camera.WorldToScreenPoint(new Vector3(world.x, world.y + LiftWorldUnits, 0f));
            if (screen.z < 0f) return;
            var width = MoodBubbleLayout.Width;
            var height = MoodBubbleLayout.BoxHeight;
            var total = height + MoodBubbleLayout.TailHeight;
            var x = WorldTicketLayout.ClampToCanvas(_canvas.Grid.VirtualX(screen.x) - width / 2, width, PixelGrid.Width);
            var y = WorldTicketLayout.ClampToCanvas(_canvas.Grid.VirtualY(_canvas.ScreenHeight - screen.y) - total, total, PixelGrid.Height);
            _canvas.Fill(x, y, width, height, PixelPalette.Ink);
            _canvas.Fill(x + 1, y + 1, width - 2, height - 2, _style.ColorOf(emotion));
            DrawFace(MoodBubbleLayout.Face(emotion), x + MoodBubbleLayout.Frame, y + MoodBubbleLayout.Frame);
            var tailX = x + width / 2 - MoodBubbleLayout.TailWidth / 2;
            _canvas.Fill(tailX, y + height, MoodBubbleLayout.TailWidth, 1, PixelPalette.Ink);
            _canvas.Fill(tailX + 1, y + height + 1, 1, 1, PixelPalette.Ink);
        }

        private void DrawFace(string[] rows, int x, int y)
        {
            for (var row = 0; row < rows.Length; row++)
                for (var col = 0; col < rows[row].Length; col++)
                    if (rows[row][col] == '#') _canvas.Fill(x + col, y + row, 1, 1, PixelPalette.Ink);
        }
    }
}
