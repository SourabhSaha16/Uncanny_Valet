using NUnit.Framework;
using UncannyValet.Valet;

namespace UncannyValet.Tests.Valet
{
    /// <summary>Pure rules of the valet's speed boost: speed factor, barge decision, rainbow and trail rate.</summary>
    public class BoostTests
    {
        [Test]
        public void Multiplier_IsOneUnlessBoosted_AndNeverBelowOne()
        {
            Assert.AreEqual(1f, BoostMath.Multiplier(false, 1.4f), 0.0001f);
            Assert.AreEqual(1.4f, BoostMath.Multiplier(true, 1.4f), 0.0001f);
            Assert.AreEqual(1f, BoostMath.Multiplier(true, 0.5f), 0.0001f);
        }

        [Test]
        public void Hue_WrapsEveryCycle()
        {
            Assert.AreEqual(0.5f, BoostMath.Hue(1f / 3f, 1.5f), 0.0001f);
            Assert.AreEqual(0.25f, BoostMath.Hue(2.25f, 1f), 0.0001f);
        }

        [Test]
        public void Outline_ShowsSteadilyThenBlinksInTheLastSeconds()
        {
            Assert.IsTrue(BoostMath.OutlineVisible(3f, 1f, 0.3f, 4f));
            Assert.IsTrue(BoostMath.OutlineVisible(0.5f, 1f, 0.0f, 4f));
            Assert.IsFalse(BoostMath.OutlineVisible(0.5f, 1f, 0.13f, 4f));
            Assert.IsTrue(BoostMath.OutlineVisible(0.5f, 1f, 0.26f, 4f));
        }

        [Test]
        public void Barge_OnlyWhileBoosted()
        {
            var rule = new BargeRule(4, 1f);
            Assert.IsFalse(rule.TryBarge(false, 7, 0f));
            Assert.IsTrue(rule.TryBarge(true, 7, 0f));
        }

        [Test]
        public void Barge_SamePersonWaitsForTheCooldown_OthersDoNot()
        {
            var rule = new BargeRule(4, 1f);
            Assert.IsTrue(rule.TryBarge(true, 7, 0f));
            Assert.IsFalse(rule.TryBarge(true, 7, 0.5f));
            Assert.IsTrue(rule.TryBarge(true, 8, 0.5f));
            Assert.IsTrue(rule.TryBarge(true, 7, 1.01f));
        }

        [Test]
        public void Barge_NobodyIsNotBarged_AndClearForgets()
        {
            var rule = new BargeRule(2, 5f);
            Assert.IsFalse(rule.TryBarge(true, 0, 0f));
            Assert.IsTrue(rule.TryBarge(true, 3, 0f));
            rule.Clear();
            Assert.IsTrue(rule.TryBarge(true, 3, 0.1f));
        }

        [Test]
        public void Barge_ForgetsTheOldestWhenFull()
        {
            var rule = new BargeRule(2, 10f);
            rule.TryBarge(true, 1, 0f);
            rule.TryBarge(true, 2, 0f);
            rule.TryBarge(true, 3, 0f);
            Assert.IsTrue(rule.TryBarge(true, 1, 1f), "1 was pushed out by 3");
            Assert.IsFalse(rule.TryBarge(true, 3, 1f));
        }

        [Test]
        public void Trail_EmitsTheExactRateOverTimeCarryingTheFraction()
        {
            var emitter = new TrailEmitter();
            var total = 0;
            for (var i = 0; i < 8; i++) total += emitter.Step(0.25f, 6f);
            Assert.AreEqual(12, total);
            Assert.AreEqual(0, new TrailEmitter().Step(0.01f, 24f));
            Assert.AreEqual(0, new TrailEmitter().Step(1f, 0f));
        }

        [Test]
        public void SparkleStar_HasAPlusShapeWithABrightCentre()
        {
            Assert.IsTrue(SparkleSprite.IsLit(2, 2));
            Assert.IsTrue(SparkleSprite.IsLit(2, 0));
            Assert.IsTrue(SparkleSprite.IsLit(1, 1));
            Assert.IsFalse(SparkleSprite.IsLit(0, 0));
            Assert.IsFalse(SparkleSprite.IsLit(0, 1));
        }
    }
}
