using UncannyValet.Core.Art;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Pure pixel rules of the prop shapes: paints one <see cref="PropShape"/> flat into a pixel buffer (origin bottom-left, rows upwards).</summary>
    public static class ShapeRaster
    {
        /// <summary>Paints the shape in <paramref name="colour"/>; pixels outside the buffer are skipped.</summary>
        public static void Paint(Color32[] pixels, Vector2Int size, in PropShape shape, Color32 colour)
        {
            var area = shape.Area;
            switch (shape.Kind)
            {
                case PropShapeKind.Text: PaintText(pixels, size, area, shape.Text, colour); return;
                case PropShapeKind.LineUp: PaintLine(pixels, size, area, false, colour); return;
                case PropShapeKind.LineDown: PaintLine(pixels, size, area, true, colour); return;
            }
            for (var y = area.yMin; y < area.yMax; y++)
            for (var x = area.xMin; x < area.xMax; x++)
                if (Covers(shape.Kind, area, x, y)) Set(pixels, size, x, y, colour);
        }

        /// <summary>True when pixel (x, y) is inside the filled kinds (Rect, Ellipse, Ring, TriangleUp) of <paramref name="area"/>.</summary>
        public static bool Covers(PropShapeKind kind, RectInt area, int x, int y)
        {
            switch (kind)
            {
                case PropShapeKind.Ellipse: return InEllipse(area, x, y, 0f);
                case PropShapeKind.Ring: return InEllipse(area, x, y, 0f) && !InEllipse(area, x, y, 1f);
                case PropShapeKind.TriangleUp: return InTriangle(area, x, y);
                default: return true;
            }
        }

        private static bool InEllipse(RectInt area, int x, int y, float shrink)
        {
            var rx = area.width * 0.5f - shrink;
            var ry = area.height * 0.5f - shrink;
            if (rx <= 0f || ry <= 0f) return false;
            var dx = (x + 0.5f - (area.xMin + area.width * 0.5f)) / rx;
            var dy = (y + 0.5f - (area.yMin + area.height * 0.5f)) / ry;
            return dx * dx + dy * dy <= 1f;
        }

        private static bool InTriangle(RectInt area, int x, int y)
        {
            var share = 1f - (y + 0.5f - area.yMin) / area.height;
            return Mathf.Abs(x + 0.5f - (area.xMin + area.width * 0.5f)) <= area.width * 0.5f * share;
        }

        private static void PaintLine(Color32[] pixels, Vector2Int size, RectInt area, bool down, Color32 colour)
        {
            var steps = Mathf.Max(area.width, area.height);
            for (var i = 0; i < steps; i++)
            {
                var t = steps == 1 ? 0f : i / (float)(steps - 1);
                var x = area.xMin + Mathf.RoundToInt(t * (area.width - 1));
                var y = area.yMin + Mathf.RoundToInt((down ? 1f - t : t) * (area.height - 1));
                Set(pixels, size, x, y, colour);
            }
        }

        private static void PaintText(Color32[] pixels, Vector2Int size, RectInt area, string text, Color32 colour)
        {
            if (string.IsNullOrEmpty(text)) return;
            var rows = PixelFont.Compose(text);
            for (var row = 0; row < rows.Length; row++)
            for (var column = 0; column < rows[row].Length; column++)
            {
                if (rows[row][column] != PixelFont.Ink || column >= area.width || row >= area.height) continue;
                Set(pixels, size, area.xMin + column, area.yMin + (rows.Length - 1 - row), colour);
            }
        }

        private static void Set(Color32[] pixels, Vector2Int size, int x, int y, Color32 colour)
        {
            if (x >= 0 && y >= 0 && x < size.x && y < size.y) pixels[y * size.x + x] = colour;
        }
    }
}
