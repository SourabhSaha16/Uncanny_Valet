using System;
using System.Collections.Generic;
using UncannyValet.UI.Kit;
using UncannyValet.UI.Mood;
using UncannyValet.UI.Status;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// The key board panel at the bottom left: one row per parked car (in a spot or station) with its coloured key tag (the same colour
    /// as the car body), the licence plate, the spot number and a status word (Parked, Washing, Pick Up ...). Most urgent tickets first.
    /// </summary>
    public sealed class KeyBoardPainter
    {
        private const int Margin = 4;
        // Wide enough for the longest status word ("Cust. Angry", 65 px) beside the bell.
        private const int Width = 134;
        private const int RowHeight = 13;
        private const int HeaderHeight = 10;
        private const int Border = 3;
        private const int MaxRows = 5;
        private const int PlateOffset = 13;
        private const int SpotOffset = 42;
        private const int StatusOffset = 56;
        private const int BellOffset = 122;
        private const int DamageOffset = 132;
        private const float LitGlow = 0.4f;
        private const float DimGlow = 0.12f;

        private readonly PixelCanvas _canvas;
        private readonly PlatePainter _plates;
        private readonly CarStatusResolver _status;
        private readonly EmotionStyle _style;
        private int _width = Width;

        public KeyBoardPainter(PixelCanvas canvas, EmotionStyle style)
        {
            _canvas = canvas;
            _style = style;
            _status = CarStatusResolver.CreateDefault(style.Rule);
            _plates = new PlatePainter(canvas);
        }

        public void Draw(List<TicketRow> rows, float time)
        {
            if (rows.Count == 0) return;
            var shown = Math.Min(rows.Count, MaxRows);
            var height = Border * 2 + HeaderHeight + shown * RowHeight;
            var x = Margin;
            var y = PixelGrid.Height - Margin - height;
            _width = Width + (AnyDamage(rows, shown) ? DamageTag.ExtraWidth : 0);
            _canvas.Panel(x, y, _width, height, PixelPalette.PanelDark);
            _canvas.Text("CAR SLOTS", x + 5, y + 4, PixelPalette.Gold);
            if (rows.Count > shown) DrawOverflow(rows.Count - shown, x + _width - 22, y + 4);
            for (var i = 0; i < shown; i++)
                DrawRow(rows[i], x + Border, y + Border + HeaderHeight + i * RowHeight, time);
        }

        private static bool AnyDamage(List<TicketRow> rows, int shown)
        {
            for (var i = 0; i < shown; i++)
                if (DamageTag.IsShown(rows[i])) return true;
            return false;
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
            if (row.Spot > 0) DrawSpot(row.Spot, x + SpotOffset, y + 3);
            DrawStatus(row, x + StatusOffset, y + 3);
            if (DamageTag.IsShown(row)) DamageTag.Draw(_canvas, row, x + DamageOffset, y + 3);
        }

        // Emotion-coloured row highlight and a bell, both flashing like the plate.
        private void DrawPickupMark(TicketRow row, int x, int y, float time)
        {
            var lit = PickupMark.IsLit(row, time, _style);
            var glow = PickupMark.Tint(row, _style);
            glow.a = lit ? LitGlow : DimGlow;
            _canvas.Fill(x, y, _width - Border * 2, RowHeight - 1, glow);
            if (lit) _canvas.Icon(PixelIcon.Bell, x + BellOffset, y + 2, 1, Color.white);
        }

        // The status text replaces the old timer bar; a label wider than the room left in the row is shortened.
        private void DrawStatus(TicketRow row, int x, int y)
        {
            var status = _status.Resolve(row);
            var label = CarStatusTable.LabelFitting(status, row, Width - Border * 2 - StatusOffset);
            _canvas.Text(label, x, y, CarStatusTable.ColorOf(status));
        }

        private void DrawSpot(int spot, int x, int y)
        {
            _canvas.Char('#', x, y, PixelPalette.Cream, 1);
            _canvas.Text(SmallNumbers.Get(spot), x + PixelTextLayout.Advance, y, PixelPalette.Cream);
        }
    }
}
