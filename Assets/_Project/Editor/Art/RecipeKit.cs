using System.Collections.Generic;
using UncannyValet.Core.Art;
using UnityEngine;

namespace UncannyValet.EditorTools.Art
{
    /// <summary>Short builders for the shapes of the first-pass prop recipes (pixels from the bottom-left of the prop picture).</summary>
    internal static class RecipeKit
    {
        public static PropShape Rect(int x, int y, int w, int h, PaletteSlot c) => new PropShape(PropShapeKind.Rect, new RectInt(x, y, w, h), c);

        public static PropShape Ellipse(int x, int y, int w, int h, PaletteSlot c) => new PropShape(PropShapeKind.Ellipse, new RectInt(x, y, w, h), c);

        public static PropShape Ring(int x, int y, int w, int h, PaletteSlot c) => new PropShape(PropShapeKind.Ring, new RectInt(x, y, w, h), c);

        public static PropShape Triangle(int x, int y, int w, int h, PaletteSlot c) => new PropShape(PropShapeKind.TriangleUp, new RectInt(x, y, w, h), c);

        public static PropShape LineUp(int x, int y, int w, int h, PaletteSlot c) => new PropShape(PropShapeKind.LineUp, new RectInt(x, y, w, h), c);

        public static PropShape LineDown(int x, int y, int w, int h, PaletteSlot c) => new PropShape(PropShapeKind.LineDown, new RectInt(x, y, w, h), c);

        public static PropShape Text(int x, int y, int w, int h, string text, PaletteSlot c) => new PropShape(PropShapeKind.Text, new RectInt(x, y, w, h), c, text);

        /// <summary>A two pixel rim, four spokes and a hub of a wheel centred on <paramref name="centre"/>.</summary>
        public static void Wheel(List<PropShape> shapes, Vector2Int centre, int radius, PaletteSlot rim, PaletteSlot hub)
        {
            var d = Mathf.RoundToInt(radius * 0.7071f);
            shapes.Add(Ring(centre.x - radius, centre.y - radius, radius * 2, radius * 2, rim));
            shapes.Add(Ring(centre.x - radius + 1, centre.y - radius + 1, radius * 2 - 2, radius * 2 - 2, rim));
            shapes.Add(LineUp(centre.x - d, centre.y - d, d * 2 + 1, d * 2 + 1, rim));
            shapes.Add(LineDown(centre.x - d, centre.y - d, d * 2 + 1, d * 2 + 1, rim));
            shapes.Add(Rect(centre.x, centre.y - radius + 2, 1, radius * 2 - 3, rim));
            shapes.Add(Rect(centre.x - radius + 2, centre.y, radius * 2 - 3, 1, rim));
            shapes.Add(Ellipse(centre.x - 6, centre.y - 6, 12, 12, hub));
        }

        /// <summary>A coloured basket with its own outline, hanging on the rim at <paramref name="at"/>.</summary>
        public static void Basket(List<PropShape> shapes, Vector2Int at, PaletteSlot colour)
        {
            shapes.Add(Rect(at.x - 6, at.y - 6, 12, 12, PaletteSlot.Outline));
            shapes.Add(Rect(at.x - 5, at.y - 5, 10, 10, colour));
        }

        /// <summary>A point on a circle, rounded to a pixel.</summary>
        public static Vector2Int OnCircle(Vector2Int centre, float radius, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            return centre + new Vector2Int(Mathf.RoundToInt(Mathf.Cos(radians) * radius), Mathf.RoundToInt(Mathf.Sin(radians) * radius));
        }

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var colour);
            return colour;
        }
    }
}
