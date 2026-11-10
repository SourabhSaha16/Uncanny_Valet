using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// The small amber "OCCUPIED" hint over the wash slot while a driven car is bumping into it. Projected from the
    /// slot's world position like the info box, so it shares the pixel scale.
    /// </summary>
    public sealed class OccupiedHintPainter
    {
        public const string Text = "OCCUPIED";

        private const float LiftWorldUnits = 1.2f;

        private readonly PixelCanvas _canvas;
        private readonly OccupiedHintTracker _tracker;

        public OccupiedHintPainter(PixelCanvas canvas, OccupiedHintTracker tracker)
        {
            _canvas = canvas;
            _tracker = tracker;
        }

        /// <summary>Pixel width of the hint panel.</summary>
        public static int PanelWidth => PixelTextLayout.Width(Text.Length, 1) + 2 * WorldTicketLayout.Border;

        /// <summary>Pixel height of the hint panel (one text line).</summary>
        public static int PanelHeight => 2 * WorldTicketLayout.Border + PixelGlyphs.Height;

        public void Draw(Camera camera)
        {
            if (camera == null || !_tracker.IsVisible) return;
            var world = _tracker.Anchor;
            var screen = camera.WorldToScreenPoint(new Vector3(world.x, world.y + LiftWorldUnits, 0f));
            if (screen.z < 0f) return;
            var width = PanelWidth;
            var height = PanelHeight;
            var x = WorldTicketLayout.ClampToCanvas(_canvas.Grid.VirtualX(screen.x) - width / 2, width, PixelGrid.Width);
            var y = WorldTicketLayout.ClampToCanvas(_canvas.Grid.VirtualY(_canvas.ScreenHeight - screen.y) - height, height, PixelGrid.Height);
            _canvas.Panel(x, y, width, height, PixelPalette.PanelDark);
            _canvas.Text(Text, x + WorldTicketLayout.Border, y + WorldTicketLayout.Border, PixelPalette.Amber);
        }
    }
}
