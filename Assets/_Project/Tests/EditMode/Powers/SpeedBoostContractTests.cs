using NUnit.Framework;
using UncannyValet.Audio;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Powers;
using UnityEngine;

namespace UncannyValet.Tests.Powers
{
    /// <summary>The speed boost as seen by the Valet module (IPowerBoost) and by the audio router.</summary>
    public class SpeedBoostContractTests
    {
        private PowerMeterConfig _config;
        private SpeedBoostEffect _effect;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<PowerMeterConfig>();
            _effect = new SpeedBoostEffect(new PowerSettings(_config));
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void Boost_StartsAtTheMultiplier_RunsFourSeconds_ThenEnds()
        {
            IPowerBoost boost = _effect;
            Assert.IsFalse(boost.IsBoosted);
            Assert.AreEqual(1f, boost.WalkMultiplier);
            _effect.Activate();
            Assert.IsTrue(boost.IsBoosted);
            Assert.AreEqual(1.4f, boost.WalkMultiplier, 0.0001f);
            _effect.Tick(3.9f);
            Assert.IsTrue(boost.IsBoosted);
            Assert.AreEqual(0.1f, boost.Remaining, 0.001f);
            _effect.Tick(0.2f);
            Assert.IsFalse(boost.IsBoosted);
            Assert.AreEqual(1f, boost.WalkMultiplier);
        }

        [Test]
        public void Boost_ActivatingAgainRefreshesWithoutStacking()
        {
            _effect.Activate();
            _effect.Tick(3f);
            _effect.Activate();
            Assert.AreEqual(4f, _effect.Remaining, 0.001f);
            Assert.AreEqual(1.4f, _effect.WalkMultiplier, 0.0001f);
        }

        [Test]
        public void Router_PlaysTheFanfareForTheSpeedBoostAndTheTimeStopForFreezeTime()
        {
            var bus = new EventBus();
            var played = new System.Collections.Generic.List<SfxId>();
            var router = new SfxEventRouter(bus, played.Add);
            router.Subscribe();
            bus.Publish(new PowerActivated(PowerKind.FreezeTime));
            bus.Publish(new PowerActivated(PowerKind.SpeedBoost));
            CollectionAssert.AreEqual(new[] { SfxId.TimeStop, SfxId.SpeedBoost }, played);
            router.Unsubscribe();
        }
    }
}
