using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Kit;
using UncannyValet.UI.Mood;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Every placed ticketed car carries its licence plate just under it; the plate border and glow flash in the
    /// patron's emotion colour once they want the car back. While the car is washed or charged a small droplet or
    /// plug icon sits beside the plate (the slot itself lights up in the lot). Projected from the car's world
    /// position so it shares the pixel scale.
    /// </summary>
    public sealed class WorldTicketPainter
    {
        private const float PlateDropWorldUnits = 1.3f;

        private readonly PixelCanvas _canvas;
        private readonly ICarFactory _cars;
        private readonly PlatePainter _plates;
        private readonly EmotionStyle _style;

        public WorldTicketPainter(PixelCanvas canvas, ICarFactory cars, EmotionStyle style)
        {
            _canvas = canvas;
            _cars = cars;
            _style = style;
            _plates = new PlatePainter(canvas);
        }

        public void Draw(List<TicketRow> rows, Camera camera, float time)
        {
            if (camera == null) return;
            for (var i = rows.Count - 1; i >= 0; i--) // least urgent first, so urgent plates draw on top
                if (rows[i].ShowsPlate && TryProject(rows[i], camera, -PlateDropWorldUnits, out var px, out var py))
                    DrawPlate(rows[i], px, py, CarWidthPixels(rows[i], camera), time);
        }

        // Canvas position of the point <offsetY> world units above the car; false when the car is gone or behind the camera.
        private bool TryProject(TicketRow row, Camera camera, float offsetY, out int x, out int y)
        {
            x = 0;
            y = 0;
            if (!_cars.TryGet(new CarId(row.Car), out var car) || car.Transform == null) return false;
            var screen = camera.WorldToScreenPoint(car.Transform.position + Vector3.up * offsetY);
            if (screen.z < 0f) return false;
            x = _canvas.Grid.VirtualX(screen.x);
            y = _canvas.Grid.VirtualY(_canvas.ScreenHeight - screen.y);
            return true;
        }

        // The car's width (its short side, in world units) as virtual canvas pixels at the camera's current zoom.
        private int CarWidthPixels(TicketRow row, Camera camera)
        {
            if (!_cars.TryGet(new CarId(row.Car), out var car) || car.Transform == null) return 0;
            var pixelsPerUnit = _canvas.ScreenHeight / (2f * camera.orthographicSize) / _canvas.Grid.Scale;
            return Mathf.RoundToInt(car.Transform.lossyScale.x * pixelsPerUnit);
        }

        private void DrawPlate(TicketRow row, int anchorX, int anchorY, int carWidth, float time)
        {
            var width = PlateLayout.WidthFor(row.Plate, carWidth);
            var x = WorldTicketLayout.ClampToCanvas(anchorX - width / 2, width, PixelGrid.Width);
            var y = WorldTicketLayout.ClampToCanvas(anchorY, PlateLayout.Height, PixelGrid.Height);
            var wanted = row.State == TicketRowState.PickupRequested;
            var emotion = _style.Rule.Resolve(row);
            if (wanted && _style.IsLit(emotion, time))
                _plates.DrawGlowing(row.Plate, x, y, width, _style.ColorOf(emotion));
            else
                _plates.Draw(row.Plate, x, y, width, PixelPalette.Ink);
            if (ServiceIcons.TryGet(row, out var icon)) DrawServiceIcon(icon, x, y, width);
        }

        private void DrawServiceIcon(PixelIcon icon, int plateX, int plateY, int plateWidth)
        {
            var iconWidth = _canvas.IconWidth(icon);
            var x = WorldTicketLayout.ClampToCanvas(PlateLayout.IconX(plateX, plateWidth), iconWidth, PixelGrid.Width);
            var y = PlateLayout.IconY(plateY, _canvas.IconHeight(icon));
            _canvas.Icon(icon, x, y, 1, Color.white);
        }
    }
}
