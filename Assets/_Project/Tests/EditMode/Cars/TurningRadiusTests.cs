using NUnit.Framework;
using UncannyValet.Cars;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    public class TurningRadiusTests
    {
        private const float Aisle = 4.6f;

        [Test]
        public void RMin_IsHalfTheAislePlusMargin() => Assert.AreEqual(3.3f, TurningRadius.RMin(Aisle, 1f), 1e-4f);

        [Test]
        public void RMin_SpecExample_SixUnitAisleGivesFour() => Assert.AreEqual(4f, TurningRadius.RMin(6f, 1f), 1e-4f);

        [Test]
        public void MaxTurnRate_IsFullSpeedOverRMin() => Assert.AreEqual(4f / 3.3f, TurningRadius.MaxTurnRate(4f, 3.3f), 1e-4f);

        [Test]
        public void MaxTurnRate_SpecExample_IsOneRadianPerSecond() => Assert.AreEqual(1f, TurningRadius.MaxTurnRate(4f, 4f), 1e-4f);

        [Test]
        public void RadiusAtSpeed_IsRMinBelowFullSpeed()
        {
            var rate = TurningRadius.MaxTurnRate(4f, 3.3f);
            Assert.AreEqual(3.3f, TurningRadius.RadiusAtSpeed(2f, 3.3f, rate), 1e-4f);
            Assert.AreEqual(3.3f, TurningRadius.RadiusAtSpeed(4f, 3.3f, rate), 1e-4f);
        }

        [Test]
        public void RadiusAtSpeed_GrowsAboveFullSpeed_AndIgnoresDirection()
        {
            var rate = TurningRadius.MaxTurnRate(4f, 4f);
            Assert.AreEqual(6f, TurningRadius.RadiusAtSpeed(6f, 4f, rate), 1e-4f);
            Assert.AreEqual(6f, TurningRadius.RadiusAtSpeed(-6f, 4f, rate), 1e-4f);
        }

        [Test]
        public void PullOut_RealAisleAndOneUnitCar_Clears()
        {
            Assert.AreEqual(4.1f, TurningRadius.PullOutAisle(3.3f, 1f, 0.3f), 1e-4f);
            Assert.IsTrue(TurningRadius.PullOutClears(Aisle, 3.3f, 1f, 0.3f));
        }

        [Test]
        public void PullOut_TooNarrowAisle_Fails() => Assert.IsFalse(TurningRadius.PullOutClears(4f, 3.3f, 1f, 0.3f));

        [Test]
        public void BaySlack_IsHalfTheSpareWidth()
        {
            Assert.AreEqual(0.8f, TurningRadius.BaySlack(2.6f, 1f), 1e-4f);
            Assert.AreEqual(0.7f, TurningRadius.BaySlack(2.6f, 1.2f), 1e-4f);
        }

        [Test]
        public void Config_DefaultsDeriveRMinAndMaxTurnRate()
        {
            var config = ScriptableObject.CreateInstance<CarHandlingConfig>();
            Assert.AreEqual(3.3f, config.RMin, 1e-4f);
            Assert.AreEqual(4f / 3.3f, config.MaxTurnRate, 1e-4f);
            Object.DestroyImmediate(config);
        }
    }
}
