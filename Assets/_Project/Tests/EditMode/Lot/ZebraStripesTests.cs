using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class ZebraStripesTests
    {
        [Test]
        public void BarsRunAcrossAHorizontalWalk()
        {
            var area = new Rect(0f, 0f, 3f, 1.4f);
            var bars = ZebraStripes.Bars(area, 0.3f, 0.3f, 0.05f);
            Assert.AreEqual(5, bars.Length);
            foreach (var bar in bars)
            {
                Assert.AreEqual(0.3f, bar.width, 1e-4f);
                Assert.AreEqual(1.3f, bar.height, 1e-4f);
            }
        }

        [Test]
        public void BarsRunAcrossAVerticalWalk()
        {
            var bars = ZebraStripes.Bars(new Rect(0f, 0f, 1.4f, 3f), 0.3f, 0.3f, 0.05f);
            Assert.AreEqual(5, bars.Length);
            Assert.AreEqual(1.3f, bars[0].width, 1e-4f);
            Assert.AreEqual(0.3f, bars[0].height, 1e-4f);
        }

        [Test]
        public void BarsAreCentredAndSeparatedByTheGap()
        {
            var area = new Rect(2f, 0f, 3f, 1.4f);
            var bars = ZebraStripes.Bars(area, 0.3f, 0.3f, 0f);
            Assert.AreEqual(area.center.x, (bars[0].xMin + bars[bars.Length - 1].xMax) * 0.5f, 1e-4f);
            for (var i = 1; i < bars.Length; i++) Assert.AreEqual(0.3f, bars[i].xMin - bars[i - 1].xMax, 1e-4f);
        }

        [Test]
        public void TooShortACrosswalkGetsNoBars()
        {
            Assert.AreEqual(0, ZebraStripes.Bars(new Rect(0f, 0f, 0.2f, 1.4f), 0.3f, 0.3f, 0f).Length);
        }

        [Test]
        public void CrosswalksDoNotOverlapEachOther()
        {
            var config = ScriptableObject.CreateInstance<LotConfig>();
            var layout = new WalkwayLayout(new LotGeometry(config), 1.4f);
            for (var i = 0; i < layout.Crosswalks.Length; i++)
                for (var j = i + 1; j < layout.Crosswalks.Length; j++)
                    Assert.IsFalse(layout.Crosswalks[i].Overlaps(layout.Crosswalks[j]), "crosswalks " + i + " and " + j);
            Object.DestroyImmediate(config);
        }
    }
}
