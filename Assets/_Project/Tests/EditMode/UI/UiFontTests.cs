using NUnit.Framework;
using UncannyValet.UI.Kit;
using UncannyValet.UI.Screens;
using UnityEngine;

namespace UncannyValet.Tests.UI
{
    public class UiFontTests
    {
        private const int TestFontSize = 100;

        [TestCase(1, 4f, 1.37f, 96, 1, 38)]
        [TestCase(2, 4f, 1.37f, 96, 1, 77)]
        [TestCase(1, 1f, 1.37f, 96, 1, 10)]
        [TestCase(7, 4f, 1.37f, 96, 1, 96)]
        [TestCase(1, 4f, 1.37f, 96, 8, 40)]
        [TestCase(1, 0.2f, 1.37f, 96, 8, 8)]
        public void Size_FitsCapitalsToTheRow_SnapsAndCaps(int scale, float unit, float em, int max, int snap, int expected)
        {
            Assert.AreEqual(expected, UiFontSizing.Size(scale, unit, em, max, snap));
        }

        [Test]
        public void Size_TextScaleBelowOneCountsAsOne()
        {
            Assert.AreEqual(UiFontSizing.Size(1, 3f, 1.37f, 96, 1), UiFontSizing.Size(0, 3f, 1.37f, 96, 1));
        }

        [Test]
        public void Config_ShipsWithAMonospacedFontThatFitsTheTextCell()
        {
            var config = Resources.Load<UiFontConfig>(UiFontConfig.ResourcePath);
            Assert.IsNotNull(config, "Assets/_Project/Resources/UiFontConfig.asset is missing");
            Assert.IsNotNull(config.Font, "UiFontConfig has no font");
            config.Font.RequestCharactersInTexture(PixelGlyphs.Chars, TestFontSize);
            var cellWidth = TestFontSize / config.EmPerRow * PixelTextLayout.Advance / PixelGlyphs.Height;
            foreach (var c in PixelGlyphs.Chars)
            {
                Assert.IsTrue(config.Font.GetCharacterInfo(c, out var info, TestFontSize), "no glyph for '" + c + "'");
                Assert.LessOrEqual(info.advance, cellWidth + 0.5f, "'" + c + "' is wider than the text cell");
            }
        }

        [Test]
        public void TitleLines_FitTheCanvasAtTheTitleScale()
        {
            const int titleScale = 7;
            Assert.AreEqual("VALET TO VALHALLA", TitlePainter.Line1 + " " + TitlePainter.Line2);
            Assert.LessOrEqual(PixelTextLayout.Width(TitlePainter.Line1.Length, titleScale), PixelGrid.Width);
            Assert.LessOrEqual(PixelTextLayout.Width(TitlePainter.Line2.Length, titleScale), PixelGrid.Width);
        }
    }
}
