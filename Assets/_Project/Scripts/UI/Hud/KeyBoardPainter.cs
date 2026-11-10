using System;
using System.Collections.Generic;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// The key board panel at the bottom left: one row per parked car (in a spot or station) with its coloured key tag (the same colour
    /// as the car body), the licence plate, the spot number and a patience bar. Most urgent tickets first.
    /// </summary>
    public sealed class KeyBoardPainter
    {
        private const int Margin = 4;
        private const int Width = 118;
        private const int RowHeight = 13;
        private const int HeaderHeight = 10;
        private const int Border = 3;
        private const int MaxRows = 5;
        private const int PlateOffset = 13;
        private const int SpotOffset = 42;
        private const int BarOffset = 66;
        private const int BarWidth = 30;
        private const int BellOffset = 100;
        private const int BarHeight = 5;
        private const float LitGlow = 0.4f;
        private const float DimGlow = 0.12f;

        private readonly PixelCanvas _canvas;
        private readonly PlatePainter _plates;

        public KeyBoardPainter(PixelCanvas canvas)
        {
            _canvas = canvas;
            _plates = new PlatePainter(canvas);
        }

        public void Draw(List<TicketRow> rows, float time)
        {
            if (rows.Count == 0) return;
            var shown = Math.Min(rows.Count, MaxRows);
            var height = Border * 2 + HeaderHeight + shown * RowHeight;
            var x = Margin;
            var y = PixelGrid.Height - Margin - height;
            _canvas.Panel(x, y, Width, height, PixelPalette.PanelDark);
            _canvas.Text("CAR SLOTS", x + 5, y + 4, PixelPalette.Gold);
            if (rows.Count > shown) DrawOverflow(rows.Count - shown, x + Width - 22, y + 4);
            for (var i = 0; i < shown; i++)
                DrawRow(rows[i], x + Border, y + Border + HeaderHeight + i * RowHeight, time);
        }

        private void DrawOverflow(int hidden, int x, int y)
        {
            _canvas.Char('+', x, y, PixelPalette.Cream, 1);
            _canvas.Text(SmallNumbers.Get(hidden), x + PixelTextLayout.Advance, y, PixelPalette.Cream);
        }

        private void DrawRow(TicketRow row, int x, int y, float time)
        {
            if (PickupMark.IsRequested(row)) DrawPickupMark(row, x, y, time);
            _canvas.Icon(PixelIcon.KeyTag, x + 2, y + 2, 1, PixelPalette.TagColor(row.Tag));
            if (row.HasPlate) _plates.Draw(row.Plate, x + PlateOffset, y + 1, PixelPalette.Ink);
            DrawSpot(row.Spot, x + SpotOffset, y + 3);
            DrawBar(row, x + BarOffset, y + 4);
        }

        // Cyan row highlight and a bell, both flashing with the plate's urgency flash.
        private void DrawPickupMark(TicketRow row, int x, int y, float time)
        {
            var lit = PickupMark.IsLit(row, time);
            var glow = PickupMark.Tint(row);
            glow.a = lit ? LitGlow : DimGlow;
            _canvas.Fill(x, y, Width - Border * 2, RowHeight - 1, glow);
            if (lit) _canvas.Icon(PixelIcon.Bell, x + BellOffset, y + 2, 1, Color.white);
        }

        private void DrawSpot(int spot, int x, int y)
        {
            if (spot <= 0)
            {
                _canvas.Char('-', x + PixelTextLayout.Advance, y, PixelPalette.Grey, 1);
                return;
            }
            _canvas.Char('#', x, y, PixelPalette.Cream, 1);
            _canvas.Text(SmallNumbers.Get(spot), x + PixelTextLayout.Advance, y, PixelPalette.Cream);
        }

        // The patience bar belongs to the pickup timer, so there is no bar until a pickup is requested.
        private void DrawBar(TicketRow row, int x, int y)
        {
            if (row.State != TicketRowState.PickupRequested) return;
            _canvas.Fill(x - 1, y - 1, BarWidth + 2, BarHeight + 2, PixelPalette.Track);
            var calm = PixelPalette.Cyan;
            if (!row.HasTimer && !row.Expired)
            {
                _canvas.Fill(x, y, BarWidth, BarHeight, calm);
                return;
            }
            var elapsed = row.Expired ? 1f : row.Elapsed01;
            var color = AlarmColors.For(Alarm.ForPatience(elapsed, row.Expired), calm);
            _canvas.Fill(x, y, row.Expired ? BarWidth : Alarm.PatienceFill(elapsed, BarWidth), BarHeight, color);
        }
    }
}
