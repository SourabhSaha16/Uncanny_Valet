using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Floating tickets over the cars: car letter, request icons (bell for a pickup, wash, plug) and a patience bar,
    /// projected from the car's world position onto the pixel canvas so they share its pixel scale.
    /// </summary>
    public sealed class WorldTicketPainter
    {
        private const float LiftWorldUnits = 1.5f;
        private const float BlinkPeriod = 0.5f;

        private readonly PixelCanvas _canvas;
        private readonly ICarFactory _cars;

        public WorldTicketPainter(PixelCanvas canvas, ICarFactory cars)
        {
            _canvas = canvas;
            _cars = cars;
        }

        public void Draw(List<TicketRow> rows, Camera camera, float time)
        {
            if (camera == null) return;
            var grid = _canvas.Grid;
            for (var i = rows.Count - 1; i >= 0; i--) // least urgent first, so urgent tickets draw on top
            {
                var row = rows[i];
                if (!_cars.TryGet(new CarId(row.Car), out var car) || car.Transform == null) continue;
                var screen = camera.WorldToScreenPoint(car.Transform.position + Vector3.up * LiftWorldUnits);
                if (screen.z < 0f) continue;
                DrawTicket(row, grid.VirtualX(screen.x), grid.VirtualY(_canvas.ScreenHeight - screen.y), time);
            }
        }

        private void DrawTicket(TicketRow row, int anchorX, int anchorY, float time)
        {
            var pickup = row.State == TicketRowState.PickupRequested;
            var icons = (pickup ? 1 : 0) + (row.NeedsWash ? 1 : 0) + (row.NeedsCharge ? 1 : 0);
            var width = WorldTicketLayout.Width(icons);
            var height = WorldTicketLayout.Height;
            var x = WorldTicketLayout.ClampToCanvas(anchorX - width / 2, width, PixelGrid.Width);
            var y = WorldTicketLayout.ClampToCanvas(anchorY - height, height, PixelGrid.Height);
            _canvas.Panel(x, y, width, height, pickup ? PixelPalette.PanelCyan : PixelPalette.PanelDark);
            var inner = x + WorldTicketLayout.Border;
            var top = y + WorldTicketLayout.Border;
            _canvas.Char(row.Letter, inner, top + 1, PixelPalette.Cream, 1);
            var iconX = inner + WorldTicketLayout.LetterWidth + WorldTicketLayout.LetterGap;
            if (pickup) iconX = DrawIcon(PixelIcon.Bell, iconX, top);
            if (row.NeedsWash) iconX = DrawIcon(PixelIcon.Wash, iconX, top);
            if (row.NeedsCharge) DrawIcon(PixelIcon.Plug, iconX, top);
            DrawBar(row, inner, top + WorldTicketLayout.IconSize + WorldTicketLayout.BarGap,
                width - WorldTicketLayout.Border * 2, time);
        }

        private int DrawIcon(PixelIcon icon, int x, int y)
        {
            _canvas.Icon(icon, x, y, 1, Color.white);
            return x + WorldTicketLayout.IconSize + WorldTicketLayout.IconGap;
        }

        private void DrawBar(TicketRow row, int x, int y, int width, float time)
        {
            _canvas.Fill(x, y, width, WorldTicketLayout.BarHeight, PixelPalette.Track);
            if (!row.HasTimer && !row.Expired) return;
            var pickup = row.State == TicketRowState.PickupRequested;
            var elapsed = row.Expired ? 1f : row.Elapsed01;
            var level = Alarm.ForPatience(elapsed, row.Expired);
            var color = AlarmColors.For(level, pickup ? PixelPalette.Cyan : PixelPalette.Green);
            if (row.Expired && !Blink.IsOn(time, BlinkPeriod)) color = PixelPalette.Cream;
            var fill = row.Expired ? width : Alarm.PatienceFill(elapsed, width);
            _canvas.Fill(x, y, fill, WorldTicketLayout.BarHeight, color);
        }
    }
}
