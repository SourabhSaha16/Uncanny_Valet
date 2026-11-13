using NUnit.Framework;
using UncannyValet.Powers;

namespace UncannyValet.Tests.Powers
{
    public class PowerBarTests
    {
        [Test]
        public void StartsEmptyWithNoBucketOpen()
        {
            var bar = new PowerBar();
            Assert.AreEqual(0f, bar.Value);
            Assert.AreEqual(0, bar.OpenBuckets);
            Assert.IsFalse(bar.CanSpend);
        }

        [Test]
        public void FillClampsAtOneHundredAndReportsChange()
        {
            var bar = new PowerBar();
            Assert.IsTrue(bar.Add(60f));
            Assert.IsTrue(bar.Add(60f));
            Assert.AreEqual(100f, bar.Value);
            Assert.IsFalse(bar.Add(5f));
            Assert.IsFalse(bar.Add(-5f));
        }

        [TestCase(0f, 0)]
        [TestCase(33.2f, 0)]
        [TestCase(33.3f, 1)]
        [TestCase(66.6f, 1)]
        [TestCase(66.7f, 2)]
        [TestCase(99.9f, 2)]
        [TestCase(100f, 3)]
        public void BucketsOpenAtAThirdTwoThirdsAndFull(float value, int open)
        {
            var bar = new PowerBar();
            bar.Add(value);
            Assert.AreEqual(open, bar.OpenBuckets);
        }

        [Test]
        public void SpendingFromSeventyFiveLeavesFortyOneSevenWithBucketOneOpen()
        {
            var bar = new PowerBar();
            bar.Add(75f);
            Assert.AreEqual(2, bar.OpenBuckets);
            Assert.IsTrue(bar.TrySpend(out var bucket));
            Assert.AreEqual(2, bucket, "the last open bucket closes");
            Assert.AreEqual(41.7f, bar.Value, 0.001f);
            Assert.AreEqual(1, bar.OpenBuckets);
            bar.Add(24.9f);
            Assert.AreEqual(1, bar.OpenBuckets);
            bar.Add(0.1f);
            Assert.AreEqual(2, bar.OpenBuckets, "back at 66.7");
        }

        [Test]
        public void CannotSpendBelowOneBucket_AndNothingChanges()
        {
            var bar = new PowerBar();
            bar.Add(33.2f);
            Assert.IsFalse(bar.TrySpend(out var bucket));
            Assert.AreEqual(0, bucket);
            Assert.AreEqual(33.2f, bar.Value, 0.0001f);
        }

        [Test]
        public void ThreeSpendsFromFullEmptyTheBarToWithinAFewTenths()
        {
            var bar = new PowerBar();
            bar.Add(100f);
            Assert.IsTrue(bar.TrySpend(out var first));
            Assert.AreEqual(3, first);
            Assert.AreEqual(2, bar.OpenBuckets, "100 - 33.3 is still the 66.7 mark");
            Assert.IsTrue(bar.TrySpend(out var second));
            Assert.AreEqual(2, second);
            Assert.IsTrue(bar.TrySpend(out var third));
            Assert.AreEqual(1, third);
            Assert.IsFalse(bar.CanSpend);
            Assert.Less(bar.Value, 1f);
        }

        [Test]
        public void Reset_EmptiesOnlyWhenNotEmpty()
        {
            var bar = new PowerBar();
            Assert.IsFalse(bar.Reset());
            bar.Add(10f);
            Assert.IsTrue(bar.Reset());
            Assert.AreEqual(0f, bar.Value);
        }
    }
}
