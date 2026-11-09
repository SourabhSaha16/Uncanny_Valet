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
    /// no needs get no box. Projected from the car's world position so it shares the pixel scale.
    /// </summary>
    public sealed class WorldTicketPainter
    {
        private const float LiftWorldUnits = 1.5f;

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
            var grid = _canvas.Grid;
            for (var i = rows.Count - 1; i >= 0; i--) // least urgent first, so urgent boxes draw on top
            {
                var row = rows[i];
                if (!row.ShowsInfoBox) continue;
                if (!_cars.TryGet(new CarId(row.Car), out var car) || car.Transform == null) continue;
                var screen = camera.WorldToScreenPoint(car.Transform.position + Vector3.up * LiftWorldUnits);
                if (screen.z < 0f) continue;
                DrawBox(row, grid.VirtualX(screen.x), grid.VirtualY(_canvas.ScreenHeight - screen.y));
            }
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
