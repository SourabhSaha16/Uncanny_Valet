using System;
using System.Text;
using NUnit.Framework;
using UncannyValet.UI.Kit;

namespace UncannyValet.Tests.UI
{
    public class PixelKitTests
    {
        [TestCase(1920, 1080, 4, 0, 0)]
        [TestCase(1280, 720, 2, 160, 90)]
        [TestCase(480, 270, 1, 0, 0)]
        [TestCase(300, 200, 1, -90, -35)]
        public void Grid_UsesLargestWholeScaleAndCentres(int w, int h, int scale, int offsetX, int offsetY)
        {
            var grid = new PixelGrid(w, h);
            Assert.AreEqual(scale, grid.Scale);
            Assert.AreEqual(offsetX, grid.OffsetX);
            Assert.AreEqual(offsetY, grid.OffsetY);
        }

        [Test]
        public void Grid_ScreenAndVirtualCoordinatesRoundTrip()
        {
            var grid = new PixelGrid(1280, 720);
            Assert.AreEqual(160 + 10 * 2, grid.ScreenX(10));
            Assert.AreEqual(10, grid.VirtualX(grid.ScreenX(10) + 1f));
            Assert.AreEqual(5, grid.VirtualY(grid.ScreenY(5)));
        }

        [Test]
        public void Glyphs_TableHasSevenRowsPerCharacterAndFiveBitRows()
        {
            Assert.AreEqual(PixelGlyphs.Chars.Length * PixelGlyphs.Height, PixelGlyphs.Rows.Length);
            foreach (var row in PixelGlyphs.Rows) Assert.Less((int)row, 1 << PixelGlyphs.Width);
        }

        [Test]
        public void Glyphs_LowerCaseMatchesUpperCaseAndUnknownIsQuestionMark()
        {
            Assert.AreEqual(PixelGlyphs.IndexOf('A'), PixelGlyphs.IndexOf('a'));
            Assert.AreEqual(PixelGlyphs.IndexOf('?'), PixelGlyphs.IndexOf('~'));
        }

        [Test]
        public void Glyphs_IHasTopBarAndCentreStem()
        {
            var glyph = PixelGlyphs.IndexOf('I');
            Assert.IsTrue(PixelGlyphs.IsLit(glyph, 0, 1));
            Assert.IsFalse(PixelGlyphs.IsLit(glyph, 0, 0));
            Assert.IsTrue(PixelGlyphs.IsLit(glyph, 3, 2));
            Assert.IsFalse(PixelGlyphs.IsLit(glyph, 3, 0));
        }

        [Test]
        public void Icons_AreRectangularAndUseOnlyPaletteKeys()
        {
            foreach (PixelIcon icon in Enum.GetValues(typeof(PixelIcon)))
            {
                var rows = PixelIconData.Rows(icon);
                foreach (var row in rows)
                {
                    Assert.AreEqual(rows[0].Length, row.Length, icon + " is not rectangular");
                    foreach (var c in row) Assert.IsTrue(c == '.' || PixelIconData.Keys.IndexOf(c) >= 0, icon + " has key " + c);
                }
            }
            Assert.AreEqual(PixelIconData.Keys.Length, PixelIconData.Colors.Length);
        }

        [Test]
        public void TextLayout_WidthLeavesOneColumnBetweenGlyphs()
        {
            Assert.AreEqual(0, PixelTextLayout.Width(0, 2));
            Assert.AreEqual(5, PixelTextLayout.Width(1, 1));
            Assert.AreEqual(11, PixelTextLayout.Width(2, 1));
            Assert.AreEqual(22, PixelTextLayout.Width(2, 2));
        }

        [Test]
        public void TextLayout_CentresAndFindsTheLongestLine()
        {
            Assert.AreEqual(10 + 15, PixelTextLayout.Centered(10, 40, 10));
            Assert.AreEqual(5, PixelTextLayout.LongestLine(new StringBuilder("AB\nCDEFG\nHI")));
        }

        [Test]
        public void Blink_IsOnForTheFirstHalfOfEachPeriod()
        {
            Assert.IsTrue(Blink.IsOn(0.1f, 1f));
            Assert.IsFalse(Blink.IsOn(0.6f, 1f));
            Assert.IsTrue(Blink.IsOn(2.2f, 1f));
        }

        [Test]
        public void SmallNumbers_AreCachedUpToTheMax()
        {
            Assert.AreSame(SmallNumbers.Get(7), SmallNumbers.Get(7));
            Assert.AreEqual("42", SmallNumbers.Get(42));
            Assert.AreEqual("0", SmallNumbers.Get(-3));
            Assert.AreEqual("150", SmallNumbers.Get(150));
        }

        [Test]
        public void CachedText_RebuildsOnlyWhenTheNumberChanges()
        {
            var text = new CachedText("$");
            Assert.IsTrue(text.Set(12));
            Assert.AreEqual("$12", text.Text.ToString());
            Assert.IsFalse(text.Set(12));
            Assert.IsTrue(text.Set(13));
            Assert.AreEqual("$13", text.Text.ToString());
        }

        [Test]
        public void PixelRect_ContainsItsOwnPixelsOnly()
        {
            var rect = new PixelRect(10, 20, 4, 3);
            Assert.IsTrue(rect.Contains(10, 20));
            Assert.IsTrue(rect.Contains(13, 22));
            Assert.IsFalse(rect.Contains(14, 20));
            Assert.IsFalse(rect.Contains(10, 23));
        }
    }
}
