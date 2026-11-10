using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class DashedOutlineTests
    {
        private static readonly Rect Box = new Rect(0f, 0f, 2f, 4f);

        [Test]
        public void EveryDashStaysInsideTheBox()
        {
            foreach (var d in DashedOutline.Dashes(Box, 0.1f, 0.4f, 0.2f))
            {
                Assert.GreaterOrEqual(d.xMin, Box.xMin - 0.0001f);
                Assert.LessOrEqual(d.xMax, Box.xMax + 0.0001f);
                Assert.GreaterOrEqual(d.yMin, Box.yMin - 0.0001f);
                Assert.LessOrEqual(d.yMax, Box.yMax + 0.0001f);
            }
        }

        [Test]
        public void EachEdgeStartsAndEndsOnADash()
        {
            var dashes = DashedOutline.Dashes(Box, 0.1f, 0.4f, 0.2f);
            Assert.IsTrue(dashes.Exists(d => Mathf.Approximately(d.xMin, 0f) && Mathf.Approximately(d.yMax, 4f)));
            Assert.IsTrue(dashes.Exists(d => Mathf.Approximately(d.xMax, 2f) && Mathf.Approximately(d.yMin, 0f)));
        }

        [Test]
        public void DashesAreSeparatedByTheGap()
        {
            var top = DashedOutline.Dashes(Box, 0.1f, 0.4f, 0.2f).FindAll(d => d.yMax > 3.99f && d.height < 0.2f);
            top.Sort((a, b) => a.xMin.CompareTo(b.xMin));
            Assert.Greater(top.Count, 2);
            for (var i = 1; i < top.Count; i++) Assert.GreaterOrEqual(top[i].xMin - top[i - 1].xMax, 0.19f);
        }

        [Test]
        public void ShortRunStillGetsOneDash()
        {
            Assert.AreEqual(1, DashedOutline.Count(0.1f, 0.4f, 0.2f));
        }
    }
}
