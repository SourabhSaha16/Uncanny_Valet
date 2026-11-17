using NUnit.Framework;
using UncannyValet.Economy;

namespace UncannyValet.Tests.Economy
{
    public class FavourTests
    {
        [Test]
        public void Favour_StartsAtOne_ClimbsOneStepPerHappyPatron_AndStopsAtTheCap()
        {
            var favour = new FavourTracker(4);
            Assert.AreEqual(1, favour.Multiplier);
            Assert.IsTrue(favour.RecordHappy());
            Assert.IsTrue(favour.RecordHappy());
            Assert.IsTrue(favour.RecordHappy());
            Assert.AreEqual(4, favour.Multiplier);
            Assert.IsFalse(favour.RecordHappy(), "already at the cap");
            Assert.AreEqual(4, favour.Multiplier);
        }

        [Test]
        public void Favour_ResetGoesBackToOne_AndReportsOnlyRealChanges()
        {
            var favour = new FavourTracker(4);
            Assert.IsFalse(favour.Reset(), "already 1x");
            favour.RecordHappy();
            favour.RecordHappy();
            Assert.IsTrue(favour.Reset());
            Assert.AreEqual(1, favour.Multiplier);
        }

        [Test]
        public void Favour_CapBelowOneStillGivesOneX()
        {
            var favour = new FavourTracker(0);
            Assert.IsFalse(favour.RecordHappy());
            Assert.AreEqual(1, favour.Multiplier);
        }
    }
}
