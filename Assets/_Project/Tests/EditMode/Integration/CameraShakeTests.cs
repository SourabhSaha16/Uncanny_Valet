using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    public class CameraShakeTests
    {
        [Test]
        public void Amplitude_IsZeroAtOrBelowTheDamageLimit()
        {
            Assert.AreEqual(0f, CameraShake.Amplitude(1f, 2f, 8f, 0.5f));
            Assert.AreEqual(0f, CameraShake.Amplitude(2f, 2f, 8f, 0.5f));
        }

        [Test]
        public void Amplitude_ScalesWithForceAndCapsAtFullForce()
        {
            Assert.AreEqual(0.25f, CameraShake.Amplitude(5f, 2f, 8f, 0.5f), 1e-5f);
            Assert.AreEqual(0.5f, CameraShake.Amplitude(8f, 2f, 8f, 0.5f), 1e-5f);
            Assert.AreEqual(0.5f, CameraShake.Amplitude(50f, 2f, 8f, 0.5f), 1e-5f);
        }

        [Test]
        public void Offset_StaysWithinTheAmplitude()
        {
            var shake = new CameraShake();
            shake.Start(0.4f, 1f, 10f);
            for (var t = 10f; t < 11f; t += 0.01f) Assert.LessOrEqual(shake.Offset(t).magnitude, 0.4f + 1e-4f);
        }

        [Test]
        public void Offset_IsZeroAtTheEndOfTheLengthAndBeforeAnyShake()
        {
            var shake = new CameraShake();
            Assert.AreEqual(Vector2.zero, shake.Offset(5f));
            shake.Start(0.4f, 0.5f, 10f);
            Assert.AreNotEqual(Vector2.zero, shake.Offset(10.01f));
            Assert.AreEqual(Vector2.zero, shake.Offset(10.5f));
            Assert.AreEqual(Vector2.zero, shake.Offset(20f));
        }

        [Test]
        public void Offset_IsDeterministic()
        {
            var a = new CameraShake();
            var b = new CameraShake();
            a.Start(0.4f, 1f, 3f);
            b.Start(0.4f, 1f, 3f);
            Assert.AreEqual(a.Offset(3.2f), b.Offset(3.2f));
        }

        [Test]
        public void Start_WeakerHitDoesNotReplaceAStrongerRunningShake()
        {
            var shake = new CameraShake();
            shake.Start(0.5f, 1f, 0f);
            shake.Start(0.05f, 1f, 0.1f);
            Assert.AreEqual(Vector2.zero, shake.Offset(1.05f), "the first shake ends on its own schedule");
        }

        [Test]
        public void Start_StrongerHitRefreshesAndNeverExceedsTheLargerAmplitude()
        {
            var shake = new CameraShake();
            shake.Start(0.2f, 1f, 0f);
            shake.Start(0.4f, 1f, 0.9f);
            for (var t = 0.9f; t < 1.9f; t += 0.01f) Assert.LessOrEqual(shake.Offset(t).magnitude, 0.4f + 1e-4f);
            Assert.AreEqual(Vector2.zero, shake.Offset(1.95f));
        }

        [Test]
        public void Start_IgnoresZeroAmplitudeOrLength()
        {
            var shake = new CameraShake();
            shake.Start(0f, 1f, 0f);
            shake.Start(1f, 0f, 0f);
            Assert.AreEqual(Vector2.zero, shake.Offset(0.1f));
        }

        [Test]
        public void ShakeLink_PlayerImpactStartsAShakeAndOthersDoNot()
        {
            var bus = new EventBus();
            var tuning = new TuningStore(bus);
            tuning.Seed(TuningKey.DamageMinForce, 2f);
            CameraShake.SeedDefaults(tuning);
            float size = 0f, seconds = 0f;
            var calls = 0;
            var link = new ShakeLink(bus, tuning, (s, l) => { size = s; seconds = l; calls++; });

            bus.Publish(new CarImpact(new CarId(1), 5f, false));
            Assert.AreEqual(0, calls);
            bus.Publish(new CarImpact(new CarId(1), 5f, true));
            Assert.AreEqual(1, calls);
            Assert.Greater(size, 0f);
            Assert.AreEqual(CameraShake.DefaultSeconds, seconds, 1e-5f);

            link.Dispose();
            bus.Publish(new CarImpact(new CarId(1), 8f, true));
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void ShakeLink_HitBelowTheDamageLimitDoesNothing()
        {
            var bus = new EventBus();
            var tuning = new TuningStore(bus);
            tuning.Seed(TuningKey.DamageMinForce, 2f);
            CameraShake.SeedDefaults(tuning);
            var calls = 0;
            using (new ShakeLink(bus, tuning, (s, l) => calls++))
            {
                bus.Publish(new CarImpact(new CarId(1), 1.5f, true));
            }
            Assert.AreEqual(0, calls);
        }
    }
}
