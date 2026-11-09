using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// The small amber hint over the waiting patron while their car sits in the curb zone facing the wrong way:
    /// "PLEASE PARK IT PROPERLY". Without a visible patron it floats over the car instead. Projected like the info box.
    /// </summary>
    public sealed class CurbHintPainter
    {
        public const string LineOne = "PLEASE PARK";
        public const string LineTwo = "IT PROPERLY";

        private const float LiftWorldUnits = 1.1f;

        private readonly PixelCanvas _canvas;
        private readonly ICarFactory _cars;
        private readonly CurbHintTracker _tracker;
        private IPatronLocator _patrons;

        public CurbHintPainter(PixelCanvas canvas, ICarFactory cars, CurbHintTracker tracker)
        {
            _canvas = canvas;
            _cars = cars;
            _tracker = tracker;
        }

        /// <summary>Pixel width of the hint panel.</summary>
        public static int PanelWidth => PixelTextLayout.Width(Mathf.Max(LineOne.Length, LineTwo.Length), 1) + 2 * WorldTicketLayout.Border;

        /// <summary>Pixel height of the hint panel (two text lines).</summary>
        public static int PanelHeight => 2 * WorldTicketLayout.Border + PixelTextLayout.LineHeight(1) + PixelGlyphs.Height;

        public void Draw(Camera camera)
        {
            if (camera == null) return;
            if (_patrons == null) Services.TryGet(out _patrons);
            var wrong = _tracker.WrongCars;
            for (var i = 0; i < wrong.Count; i++)
                if (TryAnchor(wrong[i], out var anchor)) DrawAt(camera, anchor);
        }

        private bool TryAnchor(CarId car, out Vector2 anchor)
        {
            anchor = Vector2.zero;
            if (_patrons != null && _patrons.TryGetWaitingPatron(car, out anchor)) return true;
            if (!_cars.TryGet(car, out var drivable) || drivable.Transform == null) return false;
            anchor = drivable.Transform.position;
            return true;
        }

        private void DrawAt(Camera camera, Vector2 world)
        {
            var screen = camera.WorldToScreenPoint(new Vector3(world.x, world.y + LiftWorldUnits, 0f));
            if (screen.z < 0f) return;
            var width = PanelWidth;
            var height = PanelHeight;
            var x = WorldTicketLayout.ClampToCanvas(_canvas.Grid.VirtualX(screen.x) - width / 2, width, PixelGrid.Width);
            var y = WorldTicketLayout.ClampToCanvas(_canvas.Grid.VirtualY(_canvas.ScreenHeight - screen.y) - height, height, PixelGrid.Height);
            _canvas.Panel(x, y, width, height, PixelPalette.PanelDark);
            var textX = x + WorldTicketLayout.Border;
            var textY = y + WorldTicketLayout.Border;
            _canvas.Text(LineOne, textX, textY, PixelPalette.Amber);
            _canvas.Text(LineTwo, textX, textY + PixelTextLayout.LineHeight(1), PixelPalette.Amber);
        }
    }
}
