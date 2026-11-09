using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// The info box floating over a car that has finished arriving: its key-tag colour, a dirt meter that drains
    /// while the car is washed and a charge meter that fills while it charges (amber when full). Cars with
    /// no needs get no box. Every ticketed car also carries its licence plate just under it; the plate border and
    /// glow flash once the patron wants the car back. Projected from the car's world position so it shares the pixel scale.
    /// </summary>
    public sealed class WorldTicketPainter
    {
        private const float LiftWorldUnits = 1.5f;
        private const float PlateDropWorldUnits = 1.3f;

        private readonly PixelCanvas _canvas;
        private readonly ICarFactory _cars;
        private readonly PlatePainter _plates;

        public WorldTicketPainter(PixelCanvas canvas, ICarFactory cars)
        {
            _canvas = canvas;
            _cars = cars;
            _plates = new PlatePainter(canvas);
        }

        public void Draw(List<TicketRow> rows, Camera camera, float time)
        {
            if (camera == null) return;
            for (var i = rows.Count - 1; i >= 0; i--) // least urgent first, so urgent plates and boxes draw on top
                if (rows[i].HasPlate && TryProject(rows[i], camera, -PlateDropWorldUnits, out var px, out var py))
                    DrawPlate(rows[i], px, py, time);
            for (var i = rows.Count - 1; i >= 0; i--)
                if (rows[i].ShowsInfoBox && TryProject(rows[i], camera, LiftWorldUnits, out var bx, out var by))
                    DrawBox(rows[i], bx, by);
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

        private void DrawPlate(TicketRow row, int anchorX, int anchorY, float time)
        {
            var width = PlateLayout.Width(row.Plate.Length);
            var x = WorldTicketLayout.ClampToCanvas(anchorX - width / 2, width, PixelGrid.Width);
            var y = WorldTicketLayout.ClampToCanvas(anchorY, PlateLayout.Height, PixelGrid.Height);
            var wanted = row.State == TicketRowState.PickupRequested;
            if (wanted && Blink.IsOn(time, PlateLayout.FlashPeriod(row.Elapsed01, row.Expired)))
                _plates.DrawGlowing(row.Plate, x, y, AlarmColors.For(Alarm.ForPatience(row.Elapsed01, row.Expired), PixelPalette.Cyan));
            else
                _plates.Draw(row.Plate, x, y, PixelPalette.Ink);
        }

        private void DrawBox(TicketRow row, int anchorX, int anchorY)
        {
            var lines = (row.ShowsDirtMeter ? 1 : 0) + (row.ShowsChargeMeter ? 1 : 0);
            var width = WorldTicketLayout.Width;
            var height = WorldTicketLayout.Height(lines);
            var x = WorldTicketLayout.ClampToCanvas(anchorX - width / 2, width, PixelGrid.Width);
            var y = WorldTicketLayout.ClampToCanvas(anchorY - height, height, PixelGrid.Height);
            _canvas.Panel(x, y, width, height, PixelPalette.PanelDark);
            _canvas.Icon(PixelIcon.KeyTag, x + WorldTicketLayout.Border, y + WorldTicketLayout.Border, 1,
                PixelPalette.TagColor(row.Tag));
            var line = 0;
            if (row.ShowsDirtMeter)
                DrawLine(PixelIcon.Wash, row.Dirt01, PixelPalette.Mud, x, y + WorldTicketLayout.LineY(line++));
            if (row.ShowsChargeMeter)
            {
                var full = row.ChargeShown >= TicketRow.FullCharge;
                DrawLine(PixelIcon.Plug, row.ChargeShown, full ? PixelPalette.Amber : PixelPalette.Cyan,
                    x, y + WorldTicketLayout.LineY(line));
            }
        }

        private void DrawLine(PixelIcon icon, float level01, Color fill, int boxX, int lineY)
        {
            var iconX = boxX + WorldTicketLayout.LineX;
            _canvas.Icon(icon, iconX, lineY, 1, Color.white);
            var meterX = iconX + WorldTicketLayout.IconSize + WorldTicketLayout.IconGap;
            var meterY = lineY + (WorldTicketLayout.IconSize - WorldTicketLayout.MeterHeight) / 2;
            _canvas.Fill(meterX - 1, meterY - 1, WorldTicketLayout.MeterWidth + 2, WorldTicketLayout.MeterHeight + 2,
                PixelPalette.Track);
            _canvas.Fill(meterX, meterY, WorldTicketLayout.MeterFill(level01), WorldTicketLayout.MeterHeight, fill);
        }
    }
}
