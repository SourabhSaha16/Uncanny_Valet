using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    // Speeds are along the normal from the other car to this one, so a car driving into the other is negative. Each case
    // also evaluates the other car's view of the same collision (normal flipped, so its speeds change sign).
    public class CarBounceRuleTests
    {
        private const float Min = 0.5f;

        private static float Own(float u, float v, float m, float n, float e) => CarBounceRule.FinalNormalSpeed(u, v, m, n, e, Min);

        [Test]
        public void EqualMasses_HeadOn_TheCarsSeparateAtRestitutionTimesApproach()
        {
            var a = Own(-3f, 0f, 20f, 20f, 0.4f);
            var b = -Own(0f, 3f, 20f, 20f, 0.4f); // the other car's speed along the same normal
            Assert.AreEqual(0.4f * 3f, a - b, 1e-5f);
            Assert.AreEqual(-3f * 20f, a * 20f + b * 20f, 1e-4f, "momentum is conserved");
        }

        [Test]
        public void UnequalMasses_ConserveMomentum()
        {
            var a = Own(-3f, 1f, 20f, 30f, 0.4f);
            var b = -Own(-1f, 3f, 30f, 20f, 0.4f);
            Assert.AreEqual(20f * -3f + 30f * 1f, 20f * a + 30f * b, 1e-3f);
            Assert.AreEqual(0.4f * 4f, a - b, 1e-4f, "separation speed = restitution * approach speed");
        }

        [Test]
        public void HeavyParkedCar_BouncesTheDrivenCarBackAndBarelyMoves()
        {
            var driven = Own(-3f, 0f, 20f, 160f, 0.4f);
            var parked = -Own(0f, 3f, 160f, 20f, 0.4f);
            Assert.Greater(driven, 0.5f, "the driven car rebounds away from the other");
            Assert.Less(Mathf.Abs(parked), 0.6f, "the parked car only shuffles");
        }

        [Test]
        public void HeavierCar_ChangesLessThanTheLighterOne()
        {
            var heavy = Own(-3f, 3f, 30f, 20f, 0.4f);
            var light = -Own(-3f, 3f, 20f, 30f, 0.4f);
            Assert.Less(Mathf.Abs(heavy - -3f), Mathf.Abs(light - 3f));
        }

        [Test]
        public void TinyImpact_IsLeftAlone()
        {
            Assert.AreEqual(-0.49f, Own(-0.49f, 0f, 20f, 20f, 0.4f));
            Assert.IsFalse(CarBounceRule.IsImpact(0.49f, Min));
            Assert.IsTrue(CarBounceRule.IsImpact(0.5f, Min));
        }

        [Test]
        public void ZeroBounciness_LeavesBothCarsAtTheSameSpeed()
        {
            var a = Own(-3f, 0f, 20f, 20f, 0f);
            var b = -Own(0f, 3f, 20f, 20f, 0f);
            Assert.AreEqual(a, b, 1e-5f);
        }

        [Test]
        public void ApproachSpeed_IsPositiveWhenClosing()
        {
            Assert.AreEqual(3f, CarBounceRule.ApproachSpeed(-3f, 0f));
            Assert.AreEqual(-2f, CarBounceRule.ApproachSpeed(1f, -1f));
        }

        [Test]
        public void Strength_RunsFromZeroAtTheThresholdToOneAtFullSpeed()
        {
            Assert.AreEqual(0f, CarBounceRule.Strength01(0.5f, 0.5f, 6f), 1e-5f);
            Assert.AreEqual(1f, CarBounceRule.Strength01(6f, 0.5f, 6f), 1e-5f);
            Assert.AreEqual(1f, CarBounceRule.Strength01(12f, 0.5f, 6f), 1e-5f);
            Assert.AreEqual(0.5f, CarBounceRule.Strength01(3.25f, 0.5f, 6f), 1e-4f);
        }

        [Test]
        public void Limiter_PassesOncePerPairPerIntervalRegardlessOfOrder()
        {
            var limiter = new CarBumpLimiter(0.25f);
            Assert.IsTrue(limiter.TryPass(new CarId(1), new CarId(2), 10f));
            Assert.IsFalse(limiter.TryPass(new CarId(2), new CarId(1), 10.1f));
            Assert.IsTrue(limiter.TryPass(new CarId(1), new CarId(2), 10.25f));
        }

        [Test]
        public void Limiter_TreatsDifferentPairsIndependently()
        {
            var limiter = new CarBumpLimiter(0.25f);
            Assert.IsTrue(limiter.TryPass(new CarId(1), new CarId(2), 5f));
            Assert.IsTrue(limiter.TryPass(new CarId(1), new CarId(3), 5f));
            Assert.IsTrue(limiter.TryPass(new CarId(2), new CarId(3), 5f));
        }

        [Test]
        public void Limiter_PrunesOldPairsWithoutForgettingRecentOnes()
        {
            var limiter = new CarBumpLimiter(0.25f);
            for (var i = 2; i < 100; i++) Assert.IsTrue(limiter.TryPass(new CarId(1), new CarId(i), 0f));
            Assert.IsTrue(limiter.TryPass(new CarId(1), new CarId(200), 10f));
            Assert.IsFalse(limiter.TryPass(new CarId(1), new CarId(200), 10.1f));
            Assert.IsTrue(limiter.TryPass(new CarId(1), new CarId(2), 10.1f));
        }

        [Test]
        public void Config_BounceDefaultsAreInRange()
        {
            var config = ScriptableObject.CreateInstance<CarHandlingConfig>();
            try
            {
                Assert.AreEqual(0.4f, config.Bounciness);
                Assert.AreEqual(0.25f, config.BumpPairInterval);
                Assert.GreaterOrEqual(config.ParkedMassScale, 1f);
                Assert.Less(config.MinImpactSpeed, config.FullImpactSpeed);
            }
            finally { Object.DestroyImmediate(config); }
        }
    }
}
