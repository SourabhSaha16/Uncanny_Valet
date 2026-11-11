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
    /// patron's emotion colour once they want the car back. Once the car has arrived, small droplet / plug icons say
    /// what it still needs: beside the plate, or just under the car while no plate shows yet. Projected from the car's world
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
                else if (!rows[i].ShowsPlate && ServiceIcons.Count(rows[i]) > 0
                         && TryProject(rows[i], camera, -PlateDropWorldUnits, out var ix, out var iy))
                    DrawIcons(rows[i], ix - IconsWidth(rows[i]) / 2, iy); // no plate yet (car still in the queue): icons just under the car
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
            DrawIcons(row, PlateLayout.IconX(x, width), y);
        }

        // Total width of the row's icons side by side.
        private int IconsWidth(TicketRow row)
        {
            var width = 0;
            for (var i = 0; ServiceIcons.TryGet(row, i, out var icon); i++)
                width += _canvas.IconWidth(icon) + (i > 0 ? PlateLayout.IconGap : 0);
            return width;
        }

        // Draws the icons left to right from <left>, centred on the plate row at <plateY>.
        private void DrawIcons(TicketRow row, int left, int plateY)
        {
            var x = WorldTicketLayout.ClampToCanvas(left, IconsWidth(row), PixelGrid.Width);
            for (var i = 0; ServiceIcons.TryGet(row, i, out var icon); i++)
            {
                _canvas.Icon(icon, x, PlateLayout.IconY(plateY, _canvas.IconHeight(icon)), 1, Color.white);
                x += _canvas.IconWidth(icon) + PlateLayout.IconGap;
            }
        }
    }
}
