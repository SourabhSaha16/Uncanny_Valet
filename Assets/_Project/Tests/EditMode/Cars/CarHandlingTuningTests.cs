using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    public class CarHandlingTuningTests
    {
        private CarHandlingConfig _config;

        [SetUp] public void SetUp() => _config = ScriptableObject.CreateInstance<CarHandlingConfig>();

        [TearDown] public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void Config_DefaultsMatchTheSpec()
        {
            var live = new CarHandlingLive(_config);
            var d = live.Drift;
            Assert.AreEqual(8f, d.LateralFriction);
            Assert.AreEqual(0.6f, d.ForwardGrip);
            Assert.AreEqual(3f, d.GripLowSpeed);
            Assert.AreEqual(6f, d.GripHighSpeed);
            Assert.AreEqual(0.55f, d.GripAtSpeed);
            Assert.AreEqual(0.25f, d.DriftGripScale);
            Assert.AreEqual(4f, live.TurnFullSpeed);
            Assert.AreEqual(3.3f, live.RMin, 1e-4f);
            Assert.AreEqual(1.212f, live.MaxTurnRate, 1e-3f);
        }

        [Test]
        public void EveryCatalogRowSeedsAndTrySetAcceptsIt()
        {
            var live = new CarHandlingLive(_config);
            var tuning = new TuningStore(new EventBus());
            live.Seed(tuning);
            foreach (var row in HandlingRows.Rows)
            {
                Assert.IsTrue(tuning.TryGet(row.Key, out var value), row.Key + " not seeded");
                Assert.IsTrue(live.TrySet(row.Key, value), row.Key + " not handled");
                Assert.AreEqual(HandlingRows.Group, row.Group);
                Assert.IsTrue(row.Advanced);
            }
        }

        [Test]
        public void TrySet_RejectsOtherKeys() => Assert.IsFalse(new CarHandlingLive(_config).TrySet(TuningKey.BaseTip, 1f));

        [Test]
        public void Link_AppliesPanelChangesToTheLiveValues()
        {
            var bus = new EventBus();
            var tuning = new TuningStore(bus);
            var live = new CarHandlingLive(_config);
            live.Seed(tuning);
            using (new CarHandlingTuningLink(bus, tuning, live))
            {
                tuning.Set(TuningKey.HandlingForwardGrip, 0.9f);
                tuning.Set(TuningKey.HandlingMinTurnRadius, 5f);
                Assert.AreEqual(0.9f, live.Drift.ForwardGrip, 1e-5f);
                Assert.AreEqual(4f / 5f, live.MaxTurnRate, 1e-5f);
            }
        }

        [Test]
        public void Link_AppliesValuesAlreadyInTheStoreAndStopsAfterDispose()
        {
            var bus = new EventBus();
            var tuning = new TuningStore(bus);
            var live = new CarHandlingLive(_config);
            live.Seed(tuning);
            tuning.Set(TuningKey.HandlingLateralFriction, 12f);
            var link = new CarHandlingTuningLink(bus, tuning, live);
            Assert.AreEqual(12f, live.Drift.LateralFriction, 1e-5f);
            link.Dispose();
            tuning.Set(TuningKey.HandlingLateralFriction, 3f);
            Assert.AreEqual(12f, live.Drift.LateralFriction, 1e-5f);
        }
    }
}
