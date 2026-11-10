using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class LabelFitTests
    {
        [Test]
        public void ShortText_KeepsTheMaxSize()
        {
            Assert.AreEqual(0.45f, LabelFit.Size(1, 2.6f, 0.45f, 0.9f, 0.7f), 0.0001f);
        }

        [Test]
        public void LongText_ShrinksSoItFitsWithMargin()
        {
            var size = LabelFit.Size(6, 2.6f, 5f, 0.9f, 0.7f);
            Assert.LessOrEqual(6 * 0.9f * size, 2.6f * 0.7f + 0.0001f);
        }

        [Test]
        public void DefaultConfig_FitsChargeAndWashInsideTheBay()
        {
            var c = ScriptableObject.CreateInstance<LotConfig>();
            foreach (var text in new[] { "CHARGE", "WASH" })
            {
                var size = LabelFit.Size(text.Length, c.SpotSize.x, c.StationLabelSize, c.LabelCharWidth, c.StationLabelFill);
                Assert.Less(text.Length * c.LabelCharWidth * size, c.SpotSize.x);
            }
        }
    }
}
