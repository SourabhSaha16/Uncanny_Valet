using NUnit.Framework;
using UncannyValet.Lot;

namespace UncannyValet.Tests.Lot
{
    public class CurbFacingRuleTests
    {
        private readonly CurbFacingRule _rule = new CurbFacingRule(35f);

        [TestCase(180f)]
        [TestCase(150f)]
        [TestCase(215f)]
        [TestCase(-180f)]
        [TestCase(540f)]
        public void FacingDownWithinToleranceIsAccepted(float degrees) => Assert.IsTrue(_rule.IsAcceptable(degrees));

        [TestCase(0f)]
        [TestCase(90f)]
        [TestCase(270f)]
        [TestCase(-90f)]
        [TestCase(144f)]
        [TestCase(216f)]
        public void NotFacingDownIsRejected(float degrees) => Assert.IsFalse(_rule.IsAcceptable(degrees));

        [Test]
        public void ToleranceEdgeIsInclusive()
        {
            Assert.IsTrue(_rule.IsAcceptable(145f));
            Assert.IsTrue(_rule.IsAcceptable(215f));
        }

        [TestCase(180f, 0f)]
        [TestCase(0f, 180f)]
        [TestCase(-170f, 10f)]
        [TestCase(350f, 170f)]
        [TestCase(720f, 180f)]
        public void DeviationWrapsAroundTheFullTurn(float degrees, float expected)
            => Assert.AreEqual(expected, CurbFacingRule.DeviationFromDown(degrees), 1e-3f);

        [Test]
        public void ZeroToleranceAcceptsOnlyStraightDown()
        {
            var strict = new CurbFacingRule(0f);
            Assert.IsTrue(strict.IsAcceptable(180f));
            Assert.IsFalse(strict.IsAcceptable(179f));
        }

        [Test]
        public void NegativeToleranceIsTreatedAsZero() => Assert.IsTrue(new CurbFacingRule(-5f).IsAcceptable(180f));
    }
}
