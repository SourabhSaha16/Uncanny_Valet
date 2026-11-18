using NUnit.Framework;
using UncannyValet.UI.Kit;

namespace UncannyValet.Tests.UI
{
    public class WindowShapeTests
    {
        [TestCase(2560, 1080)]
        [TestCase(1920, 1080)]
        [TestCase(1024, 768)]
        [TestCase(390, 844)]
        [TestCase(300, 200)]
        public void PixelGridStaysInsideAnyWindow(int width, int height)
        {
            var grid = new PixelGrid(width, height);
            Assert.GreaterOrEqual(grid.Scale, 1);
            if (width < PixelGrid.Width || height < PixelGrid.Height) return;
            Assert.GreaterOrEqual(grid.OffsetX, 0);
            Assert.GreaterOrEqual(grid.OffsetY, 0);
            Assert.LessOrEqual(grid.ScreenX(PixelGrid.Width), width);
            Assert.LessOrEqual(grid.ScreenY(PixelGrid.Height), height);
        }

        [Test]
        public void PanelKeepsItsWidthOnAWideScreen()
        {
            Assert.AreEqual(820f, PanelFit.Width(1920f, 820f, 10f));
        }

        [Test]
        public void PanelShrinksToFitANarrowScreen()
        {
            Assert.AreEqual(370f, PanelFit.Width(390f, 820f, 10f));
            Assert.AreEqual(0f, PanelFit.Width(10f, 820f, 10f));
        }
    }
}
