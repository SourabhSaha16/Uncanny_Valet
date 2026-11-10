using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class DinerSlicesTests
    {
        [Test]
        public void TextureRectFlipsTheTopLeftOriginToBottomLeft()
        {
            var rect = DinerSlices.TextureRect(new RectInt(11, 132, 149, 96), 320);
            Assert.AreEqual(new Rect(11f, 92f, 149f, 96f), rect);
        }

        [Test]
        public void PixelsPerUnitMakesTheSliceSpanTheDiner()
        {
            Assert.AreEqual(18.625f, DinerSlices.PixelsPerUnit(149, 8f), 1e-3f);
        }

        [Test]
        public void FacadeHeightIsWholeBandsThatFit()
        {
            Assert.AreEqual(5f, DinerSlices.FacadeHeight(1, 100, 20f, 20f), 1e-4f);
            Assert.AreEqual(10f, DinerSlices.FacadeHeight(5, 100, 20f, 11f), 1e-4f);
            Assert.AreEqual(0f, DinerSlices.FacadeHeight(2, 100, 20f, 4f), 1e-4f);
        }
    }
}
