using System;
using System.Collections.Generic;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// The key board panel at the bottom left: one row per open ticket with its coloured key tag (the same colour
    /// as the tag on the car), a car icon, the spot number and a patience bar. Most urgent tickets first.
    /// </summary>
    public sealed class KeyBoardPainter
    {
        private const int Margin = 4;
        private const int Width = 104;
        private const int RowHeight = 13;
        private const int HeaderHeight = 10;
        private const int Border = 3;
        private const int MaxRows = 5;
        private const int BarOffset = 56;
        private const int BarWidth = 42;
        private const int BarHeight = 5;
        private const float BlinkPeriod = 0.5f;
        private const float PickupGlow = 0.35f;

        private readonly PixelCanvas _canvas;

        public KeyBoardPainter(PixelCanvas canvas)
        {
            _canvas = canvas;
        }

        public void Draw(List<TicketRow> rows, float time)
        {
            if (rows.Count == 0) return;
            var shown = Math.Min(rows.Count, MaxRows);
            var height = Border * 2 + HeaderHeight + shown * RowHeight;
            var x = Margin;
            var y = PixelGrid.Height - Margin - height;
            _canvas.Panel(x, y, Width, height, PixelPalette.PanelDark);
            _canvas.Text("KEY BOARD", x + 5, y + 4, PixelPalette.Gold);
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
            var pickup = row.State == TicketRowState.PickupRequested;
            if (pickup)
            {
                var glow = PixelPalette.Cyan;
                glow.a = Blink.IsOn(time, BlinkPeriod) ? PickupGlow : PickupGlow * 0.5f;
                _canvas.Fill(x, y, Width - Border * 2, RowHeight - 1, glow);
            }
            _canvas.Icon(PixelIcon.KeyTag, x + 2, y + 2, 1, PixelPalette.TagColor(row.Tag));
            _canvas.Icon(PixelIcon.Car, x + 15, y + 3, 1, Color.white);
            DrawSpot(row.Spot, x + 30, y + 3);
            DrawBar(row, x + BarOffset - Border, y + 4);
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

        private void DrawBar(TicketRow row, int x, int y)
        {
            _canvas.Fill(x - 1, y - 1, BarWidth + 2, BarHeight + 2, PixelPalette.Track);
            var pickup = row.State == TicketRowState.PickupRequested;
            var calm = pickup ? PixelPalette.Cyan : PixelPalette.Green;
            if (!row.HasTimer && !row.Expired)
            {
                if (row.State != TicketRowState.AwaitingPark) _canvas.Fill(x, y, BarWidth, BarHeight, calm);
                return;
            }
            var elapsed = row.Expired ? 1f : row.Elapsed01;
            var color = AlarmColors.For(Alarm.ForPatience(elapsed, row.Expired), calm);
            _canvas.Fill(x, y, row.Expired ? BarWidth : Alarm.PatienceFill(elapsed, BarWidth), BarHeight, color);
        }
    }
}
