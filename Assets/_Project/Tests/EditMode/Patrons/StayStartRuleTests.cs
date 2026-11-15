using NUnit.Framework;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    /// <summary>The two-flag rule behind the stay clock (issue 139): the later of patron inside and car out of the pickup spot.</summary>
    public class StayStartRuleTests
    {
        private static readonly CarId A = new CarId(1);
        private static readonly CarId B = new CarId(2);

        [Test]
        public void PatronFirst_ThenCar_StartsOnTheCar()
        {
            var rule = new StayStartRule();
            Assert.IsFalse(rule.PatronEntered(A));
            Assert.IsTrue(rule.CarLeft(A));
        }

        [Test]
        public void CarFirst_ThenPatron_StartsOnThePatron()
        {
            var rule = new StayStartRule();
            Assert.IsFalse(rule.CarLeft(A));
            Assert.IsTrue(rule.PatronEntered(A));
        }

        [Test]
        public void RepeatingOneMoment_NeverCompletesThePair()
        {
            var rule = new StayStartRule();
            Assert.IsFalse(rule.PatronEntered(A));
            Assert.IsFalse(rule.PatronEntered(A));
            Assert.AreEqual(1, rule.Pending);
        }

        [Test]
        public void CarsAreIndependent_AndForgetAndClearReleaseTheFlags()
        {
            var rule = new StayStartRule();
            rule.PatronEntered(A);
            Assert.IsFalse(rule.CarLeft(B));
            rule.Forget(A);
            Assert.IsFalse(rule.CarLeft(A), "A's patron flag was forgotten");
            rule.Clear();
            Assert.AreEqual(0, rule.Pending);
        }

        [Test]
        public void ACompletedPair_FreesItsEntry()
        {
            var rule = new StayStartRule();
            rule.PatronEntered(A);
            rule.CarLeft(A);
            Assert.AreEqual(0, rule.Pending);
        }
    }
}
