using UncannyValet.Core.Art;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure recipe-to-pixels generator: paints the shapes of one prop layer flat in palette colours, then gives the layer a one pixel dark
    /// outline (every filled pixel that touches a clear pixel or the picture edge takes the outline colour). Hard edges, no anti-aliasing.
    /// </summary>
    public static class PropPixels
    {
        /// <summary>The pixel rows of the layer, origin bottom-left; the picture is the recipe's size in pixels.</summary>
        public static Color32[] Render(PropRecipe recipe, PropLayer layer, WorldPalette palette)
        {
            var size = recipe.SizePixels;
            var pixels = new Color32[size.x * size.y];
            for (var i = 0; i < layer.Shapes.Count; i++)
                ShapeRaster.Paint(pixels, size, layer.Shapes[i], palette.Get(layer.Shapes[i].Colour));
            if (layer.Outlined) Outline(pixels, size, palette.Get(PaletteSlot.Outline));
            return pixels;
        }

        /// <summary>Turns every filled pixel that has a clear 4-neighbour (or lies on the picture edge) into <paramref name="outline"/>.</summary>
        public static void Outline(Color32[] pixels, Vector2Int size, Color32 outline)
        {
            var edge = new bool[pixels.Length];
            for (var y = 0; y < size.y; y++)
            for (var x = 0; x < size.x; x++)
                edge[y * size.x + x] = pixels[y * size.x + x].a > 0 && TouchesClear(pixels, size, x, y);
            for (var i = 0; i < edge.Length; i++)
                if (edge[i]) pixels[i] = outline;
        }

        private static bool TouchesClear(Color32[] pixels, Vector2Int size, int x, int y)
            => IsClear(pixels, size, x - 1, y) || IsClear(pixels, size, x + 1, y) || IsClear(pixels, size, x, y - 1) || IsClear(pixels, size, x, y + 1);

        private static bool IsClear(Color32[] pixels, Vector2Int size, int x, int y)
            => x < 0 || y < 0 || x >= size.x || y >= size.y || pixels[y * size.x + x].a == 0;
    }
}
