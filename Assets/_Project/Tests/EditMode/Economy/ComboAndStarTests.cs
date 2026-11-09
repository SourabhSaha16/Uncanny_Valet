using NUnit.Framework;
using UncannyValet.Economy;

namespace UncannyValet.Tests.Economy
{
    public class ComboAndStarTests
    {
        [Test]
        public void Combo_StartsAtOne_ClimbsOneStepPerHappyPatron_AndStopsAtTheCap()
        {
            var combo = new ComboTracker(4);
            Assert.AreEqual(1, combo.Multiplier);
            Assert.IsTrue(combo.RecordHappy());
            Assert.IsTrue(combo.RecordHappy());
            Assert.IsTrue(combo.RecordHappy());
            Assert.AreEqual(4, combo.Multiplier);
            Assert.IsFalse(combo.RecordHappy(), "already at the cap");
            Assert.AreEqual(4, combo.Multiplier);
        }

        [Test]
        public void Combo_ResetGoesBackToOne_AndReportsOnlyRealChanges()
        {
            var combo = new ComboTracker(4);
            Assert.IsFalse(combo.Reset(), "already 1x");
            combo.RecordHappy();
            combo.RecordHappy();
            Assert.IsTrue(combo.Reset());
            Assert.AreEqual(1, combo.Multiplier);
        }

        [Test]
        public void Combo_CapBelowOneStillGivesOneX()
        {
            var combo = new ComboTracker(0);
            Assert.IsFalse(combo.RecordHappy());
            Assert.AreEqual(1, combo.Multiplier);
        }

        [TestCase(0, 0)]
        [TestCase(149, 0)]
        [TestCase(150, 1)]
        [TestCase(299, 1)]
        [TestCase(300, 2)]
        [TestCase(449, 2)]
        [TestCase(450, 3)]
        [TestCase(9999, 3)]
        public void Stars_FollowTheThresholds(int score, int stars)
            => Assert.AreEqual(stars, StarRating.Stars(score, 150, 300, 450));
    }
}
