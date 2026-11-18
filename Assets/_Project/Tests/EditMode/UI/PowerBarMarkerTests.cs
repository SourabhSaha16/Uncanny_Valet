using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Powers;
using UncannyValet.UI.Hud;

namespace UncannyValet.Tests.UI
{
    /// <summary>Issue #142: a bolt marker sits exactly where its power becomes usable, from one threshold list.</summary>
    public class PowerBarMarkerTests
    {
        [Test]
        public void FirstThreshold_IsTheBucketTheBarSpends()
        {
            Assert.AreEqual(PowerBar.BucketSize, PowerBarThresholds.ValueOf(1), 0f);
            Assert.AreEqual(PowerBar.BucketCount, PowerBarThresholds.Count);
        }

        [Test]
        public void FirstMarker_IsUsableAtExactlyItsFillAndNotBelow()
        {
            var marker = PowerBarThresholds.FractionOf(1);
            var bar = new PowerBar();
            bar.Add(marker * PowerBar.Full);
            Assert.IsTrue(bar.CanSpend, "usable when the fill touches the first marker");
            Assert.AreEqual(1, bar.OpenBuckets);

            var below = new PowerBar();
            below.Add(marker * PowerBar.Full - 0.05f);
            Assert.IsFalse(below.CanSpend, "not usable just under the first marker");
            Assert.AreEqual(0, below.OpenBuckets);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void EveryMarker_OpensItsBucketAtItsFraction(int level)
        {
            var bar = new PowerBar();
            bar.Add(PowerBarThresholds.FractionOf(level) * PowerBar.Full);
            Assert.AreEqual(level, bar.OpenBuckets);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void BoltIsFullExactlyAtItsThreshold(int level)
        {
            var view = new PowerBarView(1f, 1f, 1f);
            view.Set(PowerBarThresholds.ValueOf(level), level, 0);
            view.Tick(100f);
            Assert.AreEqual(1f, BoltFillMaths.Share(view.Fill, level), 1e-4f);
        }
    }
}
