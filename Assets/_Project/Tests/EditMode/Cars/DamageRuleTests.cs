using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Tests.Cars
{
    public class DamageRuleTests
    {
        private const float Min = 1.5f;
        private const float Per = 8f;

        [TestCase(0f)]
        [TestCase(0.5f)]
        [TestCase(1.5f)]
        public void HitsAtOrBelowTheThreshold_AreFree(float force)
            => Assert.AreEqual(0f, DamageRule.Delta01(force, Min, Per));

        [Test]
        public void AboveTheThreshold_DamageGrowsLinearlyInPercentPerForce()
        {
            Assert.AreEqual(0.08f, DamageRule.Delta01(2.5f, Min, Per), 1e-5f);
            Assert.AreEqual(0.24f, DamageRule.Delta01(4.5f, Min, Per), 1e-5f);
            Assert.AreEqual(0.52f, DamageRule.Delta01(8f, Min, Per), 1e-5f);
        }

        [Test]
        public void NeverNegative_WhateverTheInput()
        {
            Assert.AreEqual(0f, DamageRule.Delta01(-5f, Min, Per));
            Assert.AreEqual(0f, DamageRule.Delta01(10f, Min, -3f));
            Assert.AreEqual(0.3f, DamageRule.Add(0.3f, -0.5f), 1e-5f);
            Assert.AreEqual(0f, DamageRule.Force(-4f, 20f));
            Assert.AreEqual(0f, DamageRule.Force(4f, 0f));
        }

        [Test]
        public void TheMeterStopsAtAWreck()
        {
            Assert.AreEqual(1f, DamageRule.Add(0.9f, 0.5f));
            Assert.AreEqual(DamageRule.Wrecked, DamageRule.Add(1f, 0.2f));
            Assert.AreEqual(0.7f, DamageRule.Add(0.3f, 0.4f), 1e-5f);
        }

        [Test]
        public void Force_IsTheImpulseOverTheMass()
            => Assert.AreEqual(4f, DamageRule.Force(80f, 20f), 1e-5f);

        [Test]
        public void TheDamageRowsAreInTheCatalogAndSeededWithTheConfigDefaults()
        {
            var config = UnityEngine.ScriptableObject.CreateInstance<CarHandlingConfig>();
            var live = new CarHandlingLive(config);
            Assert.AreEqual(config.DamageMinForce, live.DamageMinForce);
            Assert.IsTrue(live.TrySet(TuningKey.DamagePercentPerForce, 12f));
            Assert.AreEqual(12f, live.DamagePercentPerForce);
            foreach (var row in DamageRows.Rows) Assert.IsTrue(System.Linq.Enumerable.Contains(TuningCatalog.All, row));
            UnityEngine.Object.DestroyImmediate(config);
        }
    }
}
