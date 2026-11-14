using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Art;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>The pure recipe-to-pixels generator: shapes, the one pixel outline, sizes, palette colours, hard edges.</summary>
    public sealed class PropPixelsTests
    {
        private static readonly Color32 Ink = new Color32(9, 9, 9, 255);
        private static readonly Color32 Fill = new Color32(200, 150, 90, 255);

        private WorldPalette _palette;

        [SetUp]
        public void SetUp()
        {
            var colours = new Color[WorldPalette.SlotCount];
            for (var i = 0; i < colours.Length; i++) colours[i] = Fill;
            colours[(int)PaletteSlot.Outline] = Ink;
            _palette = WorldPalette.Create("Test", colours);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_palette);

        private static int Key(Color32 c) => c.r | (c.g << 8) | (c.b << 16) | (c.a << 24);

        private static void Same(Color32 expected, Color32 actual, string message) => Assert.AreEqual(Key(expected), Key(actual), message);

        private static PropShape Shape(PropShapeKind kind, int x, int y, int w, int h, string text = null)
            => new PropShape(kind, new RectInt(x, y, w, h), PaletteSlot.Wall, text);

        private static Color32[] Draw(WorldPalette palette, Vector2Int units, bool outlined, params PropShape[] shapes)
        {
            var layer = PropLayer.Create("L", shapes, outlined);
            var recipe = PropRecipe.Create("Test", units, new[] { layer });
            var pixels = PropPixels.Render(recipe, layer, palette);
            Object.DestroyImmediate(recipe);
            return pixels;
        }

        [Test]
        public void ARect_FillsExactlyItsArea_OnTheGrid()
        {
            var pixels = Draw(_palette, Vector2Int.one, false, Shape(PropShapeKind.Rect, 4, 6, 10, 8));
            Assert.AreEqual(32 * 32, pixels.Length);
            var filled = 0;
            foreach (var p in pixels) if (p.a > 0) filled++;
            Assert.AreEqual(80, filled);
            Assert.AreEqual(255, pixels[6 * 32 + 4].a);
            Assert.AreEqual(0, pixels[5 * 32 + 4].a);
        }

        [Test]
        public void TheOutline_IsExactlyOnePixelAroundTheEdge_AndInsideStaysFlat()
        {
            var pixels = Draw(_palette, Vector2Int.one, true, Shape(PropShapeKind.Rect, 4, 4, 10, 8));
            for (var y = 4; y < 12; y++)
            for (var x = 4; x < 14; x++)
            {
                var edge = x == 4 || x == 13 || y == 4 || y == 11;
                Same(edge ? Ink : Fill, pixels[y * 32 + x], "pixel " + x + "," + y);
            }
        }

        [Test]
        public void AShapeOnThePictureEdge_StillGetsAnOutline()
        {
            var pixels = Draw(_palette, Vector2Int.one, true, Shape(PropShapeKind.Rect, 0, 0, 32, 32));
            Same(Ink, pixels[0], "corner");
            Same(Ink, pixels[16 * 32 + 31], "right edge");
            Same(Fill, pixels[16 * 32 + 16], "inside");
        }

        [Test]
        public void EllipseTriangleRingAndLines_PaintTheirKind()
        {
            var ellipse = Draw(_palette, Vector2Int.one, false, Shape(PropShapeKind.Ellipse, 0, 0, 32, 32));
            Assert.AreEqual(255, ellipse[16 * 32 + 16].a);
            Assert.AreEqual(0, ellipse[0].a, "corner is outside");
            var ring = Draw(_palette, Vector2Int.one, false, Shape(PropShapeKind.Ring, 0, 0, 32, 32));
            Assert.AreEqual(0, ring[16 * 32 + 16].a, "the ring is hollow");
            Assert.AreEqual(255, ring[16 * 32 + 0].a);
            var triangle = Draw(_palette, Vector2Int.one, false, Shape(PropShapeKind.TriangleUp, 0, 0, 32, 32));
            Assert.AreEqual(255, triangle[1 * 32 + 3].a, "wide at the bottom");
            Assert.AreEqual(0, triangle[30 * 32 + 3].a, "narrow at the top");
            var up = Draw(_palette, Vector2Int.one, false, Shape(PropShapeKind.LineUp, 0, 0, 32, 32));
            Assert.AreEqual(255, up[0].a);
            Assert.AreEqual(255, up[31 * 32 + 31].a);
            var down = Draw(_palette, Vector2Int.one, false, Shape(PropShapeKind.LineDown, 0, 0, 32, 32));
            Assert.AreEqual(255, down[31 * 32].a);
            Assert.AreEqual(255, down[31].a);
        }

        [Test]
        public void Text_IsDrawnInThePixelFont()
        {
            var pixels = Draw(_palette, new Vector2Int(2, 1), false, Shape(PropShapeKind.Text, 0, 0, 40, 7, "OPEN"));
            var filled = 0;
            foreach (var p in pixels) if (p.a > 0) filled++;
            Assert.Greater(filled, 40);
            Assert.AreEqual(0, pixels[7 * 64].a, "nothing above the letters");
        }

        [Test]
        public void EveryShippedRecipe_IsHardEdged_InItsPalette_WithAnOutlineWhereAsked()
        {
            foreach (var recipe in ShippedArt.All<PropRecipe>())
            {
                var palette = ShippedArt.PaletteFor(recipe);
                var allowed = new HashSet<int>();
                for (var i = 0; i < WorldPalette.SlotCount; i++) allowed.Add(Key(palette.Get((PaletteSlot)i)));
                foreach (var layer in recipe.Layers)
                {
                    var pixels = PropPixels.Render(recipe, layer, palette);
                    Assert.AreEqual(recipe.SizePixels.x * recipe.SizePixels.y, pixels.Length, recipe.name);
                    var filled = false;
                    foreach (var p in pixels)
                    {
                        if (p.a == 0) continue;
                        filled = true;
                        Assert.AreEqual(255, p.a, recipe.name + " has a soft edge");
                        Assert.IsTrue(allowed.Contains(Key(p)), recipe.name + " uses a colour outside its palette");
                    }
                    Assert.IsTrue(filled, recipe.name + " layer " + layer.Name + " draws nothing");
                    if (layer.Outlined) AssertOutlined(recipe, pixels, palette.Get(PaletteSlot.Outline));
                }
            }
        }

        private static void AssertOutlined(PropRecipe recipe, Color32[] pixels, Color32 ink)
        {
            var size = recipe.SizePixels;
            for (var y = 0; y < size.y; y++)
            for (var x = 0; x < size.x; x++)
            {
                if (pixels[y * size.x + x].a == 0) continue;
                var edge = x == 0 || y == 0 || x == size.x - 1 || y == size.y - 1
                    || pixels[y * size.x + x - 1].a == 0 || pixels[y * size.x + x + 1].a == 0
                    || pixels[(y - 1) * size.x + x].a == 0 || pixels[(y + 1) * size.x + x].a == 0;
                if (edge) Same(ink, pixels[y * size.x + x], recipe.name + " edge pixel " + x + "," + y + " is not the outline");
            }
        }
    }
}
