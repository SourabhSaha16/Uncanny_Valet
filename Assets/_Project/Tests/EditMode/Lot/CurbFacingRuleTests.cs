using NUnit.Framework;
using UncannyValet.Lot;

namespace UncannyValet.Tests.Lot
{
    public class CurbFacingRuleTests
    {
        private readonly CurbFacingRule _rule = new CurbFacingRule(80f);

        [TestCase(0f)]
        [TestCase(30f)]
        [TestCase(-35f)]
        [TestCase(360f)]
        [TestCase(-360f)]
        [TestCase(80f)]
        [TestCase(280f)]
        public void FacingUpWithinToleranceIsAccepted(float degrees) => Assert.IsTrue(_rule.IsAcceptable(degrees));

        [TestCase(180f)]
        [TestCase(90f)]
        [TestCase(270f)]
        [TestCase(-90f)]
        [TestCase(81f)]
        [TestCase(279f)]
        public void NotFacingUpIsRejected(float degrees) => Assert.IsFalse(_rule.IsAcceptable(degrees));

        [Test]
        public void ToleranceEdgeIsInclusive()
        {
            Assert.IsTrue(_rule.IsAcceptable(80f));
            Assert.IsTrue(_rule.IsAcceptable(-80f));
        }

        [TestCase(0f, 0f)]
        [TestCase(180f, 180f)]
        [TestCase(-170f, 170f)]
        [TestCase(350f, 10f)]
        [TestCase(720f, 0f)]
        public void DeviationWrapsAroundTheFullTurn(float degrees, float expected)
            => Assert.AreEqual(expected, CurbFacingRule.DeviationFromRequired(degrees), 1e-3f);

        [Test]
        public void ZeroToleranceAcceptsOnlyStraightUp()
        {
            var strict = new CurbFacingRule(0f);
            Assert.IsTrue(strict.IsAcceptable(0f));
            Assert.IsFalse(strict.IsAcceptable(1f));
        }

        [Test]
        public void NegativeToleranceIsTreatedAsZero() => Assert.IsTrue(new CurbFacingRule(-5f).IsAcceptable(0f));

        [Test]
        public void ADeliveryFacingDownIsRejectedAndOneFacingUpIsAccepted()
        {
            Assert.IsFalse(_rule.IsAcceptable(180f), "the old down-facing hand-over no longer counts");
            Assert.IsTrue(_rule.IsAcceptable(0f));
        }
    }
}
