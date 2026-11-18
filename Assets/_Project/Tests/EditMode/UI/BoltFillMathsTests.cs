using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.UI.Hud;

namespace UncannyValet.Tests.UI
{
    /// <summary>Issue #184: the power meter is three bolts that fill one after another.</summary>
    public class BoltFillMathsTests
    {
        [Test]
        public void EmptyBar_FillsNoBolt()
        {
            for (var bolt = 1; bolt <= 3; bolt++) Assert.AreEqual(0f, BoltFillMaths.Share(0f, bolt));
        }

        [Test]
        public void FullBar_FillsEveryBolt()
        {
            for (var bolt = 1; bolt <= 3; bolt++) Assert.AreEqual(1f, BoltFillMaths.Share(1f, bolt), 1e-4f);
        }

        [Test]
        public void FirstBoltFillsBeforeTheSecondStarts()
        {
            var half = PowerBarThresholds.FractionOf(1) / 2f;
            Assert.AreEqual(0.5f, BoltFillMaths.Share(half, 1), 1e-4f);
            Assert.AreEqual(0f, BoltFillMaths.Share(half, 2));
            Assert.AreEqual(0f, BoltFillMaths.Share(half, 3));
        }

        [Test]
        public void SecondBoltTakesTheOverflowOfTheFirst()
        {
            var fill = PowerBarThresholds.FractionOf(1) + (PowerBarThresholds.FractionOf(2) - PowerBarThresholds.FractionOf(1)) / 2f;
            Assert.AreEqual(1f, BoltFillMaths.Share(fill, 1), 1e-4f);
            Assert.AreEqual(0.5f, BoltFillMaths.Share(fill, 2), 1e-4f);
            Assert.AreEqual(0f, BoltFillMaths.Share(fill, 3));
        }

        [Test]
        public void SpendingOneBolt_EmptiesExactlyTheHighestFilledOne()
        {
            var before = PowerBarThresholds.FractionOf(2);
            var after = before - PowerBarThresholds.FractionOf(1);
            Assert.AreEqual(1f, BoltFillMaths.Share(before, 2), 1e-4f);
            Assert.AreEqual(0f, BoltFillMaths.Share(after, 2), 0.01f);
            Assert.AreEqual(1f, BoltFillMaths.Share(after, 1), 1e-4f);
        }

        [TestCase(0f, 0)]
        [TestCase(0.01f, 1)]
        [TestCase(0.5f, 5)]
        [TestCase(1f, 10)]
        public void LitRows_RoundsAndKeepsOneRowForAnyFill(float share, int expected)
        {
            Assert.AreEqual(expected, BoltFillMaths.LitRows(share, 10));
        }

        [Test]
        public void Pulse_IsATriangleWaveAndZeroWithoutARate()
        {
            Assert.AreEqual(0f, BoltFillMaths.Pulse(0f, 2f), 1e-4f);
            Assert.AreEqual(1f, BoltFillMaths.Pulse(0.25f, 2f), 1e-4f);
            Assert.AreEqual(0f, BoltFillMaths.Pulse(0.5f, 2f), 1e-4f);
            Assert.AreEqual(0f, BoltFillMaths.Pulse(3f, 0f));
        }

        [Test]
        public void Row_FitsRightOfTheCentreAndBoltsDoNotOverlap()
        {
            var left = BoltRowLayout.Left(480, 4);
            Assert.Greater(left, 240);
            Assert.GreaterOrEqual(BoltRowLayout.BoltX(left, 2), BoltRowLayout.BoltX(left, 1) + BoltRowLayout.BoltWidth);
            Assert.LessOrEqual(BoltRowLayout.BoltX(left, 3) + BoltRowLayout.BoltWidth, 480 - 4);
        }
    }
}
