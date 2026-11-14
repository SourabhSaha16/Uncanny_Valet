using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Pedestrians;
using UnityEngine;

namespace UncannyValet.Tests.Pedestrians
{
    /// <summary>The comic knockback of a pedestrian hit by a car: distance and tumble grow with the hit speed.</summary>
    public class PedestrianKnockbackTests
    {
        private static KnockbackCurve Defaults() => KnockbackCurve.FromTuning(new TuningStore(new EventBus()));

        [Test]
        public void BelowWalkingSpeed_GivesTheGentleKnockback()
        {
            var curve = Defaults();
            Assert.AreEqual(0.7f, curve.Distance(0.2f), 0.0001f);
            Assert.AreEqual(0.4f, curve.Seconds(0.2f), 0.0001f);
            Assert.AreEqual(0.7f, curve.Distance(1f), 0.0001f);
        }

        [Test]
        public void AtFourUnitsPerSecond_IsAboutAThirdOfTheWayUp()
        {
            var curve = Defaults();
            var share = 3f / 7f;
            Assert.AreEqual(Mathf.Lerp(0.7f, 4f, share), curve.Distance(4f), 0.0001f);
            Assert.AreEqual(Mathf.Lerp(0.4f, 0.8f, share), curve.Seconds(4f), 0.0001f);
        }

        [Test]
        public void AtTopSpeedAndAbove_IsCappedAtTheFarValues()
        {
            var curve = Defaults();
            Assert.AreEqual(4f, curve.Distance(8f), 0.0001f);
            Assert.AreEqual(0.8f, curve.Seconds(8f), 0.0001f);
            Assert.AreEqual(4f, curve.Distance(20f), 0.0001f);
            Assert.AreEqual(0.8f, curve.Seconds(20f), 0.0001f);
        }

        [Test]
        public void FollowsTheTuningValues()
        {
            var tuning = new TuningStore(new EventBus());
            PedestrianHitRows.Seed(tuning, false);
            tuning.Set(TuningKey.PedestrianKnockbackFar, 2f);
            Assert.AreEqual(2f, KnockbackCurve.FromTuning(tuning).Distance(8f), 0.0001f);
        }

        [Test]
        public void Stumble_UsesTheGivenDistanceAndTimeAndSettles()
        {
            var rule = new PedestrianStumbleRule();
            rule.Configure(0.1f, 1f);
            Assert.IsTrue(rule.TryStart(Vector2.right, 0.8f, 4f));
            for (var i = 0; i < 19; i++) rule.Step(0.02f);
            Assert.IsTrue(rule.Active);
            Assert.Greater(rule.Offset.x, 1f);
            Assert.LessOrEqual(rule.Offset.x, 4f);
            rule.Step(1f);
            Assert.IsFalse(rule.Active);
            Assert.AreEqual(Vector2.zero, rule.Offset);
        }

        [Test]
        public void Stumble_RefusesAnEmptyDistance()
        {
            Assert.IsFalse(new PedestrianStumbleRule().TryStart(Vector2.right, 0.4f, 0f));
        }
    }
}
