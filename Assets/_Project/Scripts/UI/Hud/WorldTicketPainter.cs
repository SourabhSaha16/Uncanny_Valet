using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// From the moment a car spawns, small modifier icons (droplet, plug, thumbs up) just under it say what it still needs. Projected from the
    /// car's world position so they share the pixel scale.
    /// </summary>
    public sealed class WorldTicketPainter
    {
        private const int IconGap = 2;

        private readonly PixelCanvas _canvas;
        private readonly ICarFactory _cars;

        public WorldTicketPainter(PixelCanvas canvas, ICarFactory cars)
        {
            _canvas = canvas;
            _cars = cars;
        }

        public void Draw(List<TicketRow> rows, Camera camera)
        {
            if (camera == null) return;
            for (var i = 0; i < rows.Count; i++)
                if (ServiceIcons.Count(rows[i]) > 0 && TryProject(rows[i], camera, out var x, out var y))
                    DrawIcons(rows[i], x - IconsWidth(rows[i]) / 2, y);
        }

        // Canvas position just under the car; false when the car is gone or behind the camera.
        private bool TryProject(TicketRow row, Camera camera, out int x, out int y)
        {
            x = 0;
            y = 0;
            if (!_cars.TryGet(new CarId(row.Car), out var car) || car.Transform == null) return false;
            var screen = camera.WorldToScreenPoint(car.Transform.position + Vector3.down * WorldTicketLayout.IconDropWorldUnits);
            if (screen.z < 0f) return false;
            x = _canvas.Grid.VirtualX(screen.x);
            y = _canvas.Grid.VirtualY(_canvas.ScreenHeight - screen.y);
            return true;
        }

        // Total width of the row's icons side by side.
        private int IconsWidth(TicketRow row)
        {
            var width = 0;
            for (var i = 0; ServiceIcons.TryGet(row, i, out var icon); i++)
                width += _canvas.IconWidth(icon) + (i > 0 ? IconGap : 0);
            return width;
        }

        // Draws the icons left to right from <left>, top at <top>.
        private void DrawIcons(TicketRow row, int left, int top)
        {
            var x = WorldTicketLayout.ClampToCanvas(left, IconsWidth(row), PixelGrid.Width);
            for (var i = 0; ServiceIcons.TryGet(row, i, out var icon); i++)
            {
                _canvas.Icon(icon, x, WorldTicketLayout.ClampToCanvas(top, _canvas.IconHeight(icon), PixelGrid.Height), 1, Color.white);
                x += _canvas.IconWidth(icon) + IconGap;
            }
        }
    }
}
