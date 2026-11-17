using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>The palette swap used to re-skin the diner (red roof to green roof, grey walls to tan).</summary>
    public sealed class PixelRecolourTests
    {
        private static readonly Color32 Green = new Color32(60, 140, 70, 255);
        private static readonly Color32 Tan = new Color32(216, 186, 138, 255);

        [Test]
        public void ARedRoofPixel_BecomesGreen_KeepingItsShade()
        {
            var bright = PixelRecolour.Remap(new Color32(200, 20, 20, 255), Green, Tan);
            var dark = PixelRecolour.Remap(new Color32(100, 10, 10, 255), Green, Tan);
            Assert.Greater(bright.g, bright.r);
            Assert.Greater(dark.g, dark.r);
            Assert.Greater(bright.g, dark.g, "the lighter roof pixel stays lighter");
        }

        [Test]
        public void AGreyWallPixel_BecomesTan()
        {
            var wall = PixelRecolour.Remap(new Color32(225, 225, 225, 255), Green, Tan);
            Assert.Greater(wall.r, wall.b, "warm tan, not grey");
        }

        [Test]
        public void WindowsOutlinesAndClearPixels_AreLeftAlone()
        {
            var blue = new Color32(70, 130, 230, 255);
            var black = new Color32(20, 20, 24, 255);
            var clear = new Color32(0, 0, 0, 0);
            Assert.AreEqual(blue.b, PixelRecolour.Remap(blue, Green, Tan).b);
            Assert.AreEqual(black.r, PixelRecolour.Remap(black, Green, Tan).r);
            Assert.AreEqual(0, PixelRecolour.Remap(clear, Green, Tan).a);
        }

        [Test]
        public void Apply_RecoloursABuffer_AndKeepsAlpha()
        {
            var pixels = new[] { new Color32(200, 20, 20, 255), new Color32(225, 225, 225, 128) };
            PixelRecolour.Apply(pixels, Green, Tan);
            Assert.Greater(pixels[0].g, pixels[0].r);
            Assert.AreEqual(128, pixels[1].a);
        }
    }
}
