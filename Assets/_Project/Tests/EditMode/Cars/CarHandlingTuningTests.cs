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
            Assert.AreEqual(2.2f, live.RMin, 1e-4f);
            Assert.AreEqual(1.818f, live.MaxTurnRate, 1e-3f);
        }

        [Test]
        public void DriveScales_DefaultToTheCarsOwnValuesAndMultiplyWhenChanged()
        {
            var car = ScriptableObject.CreateInstance<CarDefinition>();
            var live = new CarHandlingLive(_config);
            var d = live.DriveFor(car);
            Assert.AreEqual(car.TopSpeed, d.TopSpeed);
            Assert.AreEqual(car.Acceleration, d.Acceleration);
            Assert.AreEqual(car.Braking, d.Braking);
            Assert.AreEqual(car.CoastDrag, d.CoastDrag);
            Assert.AreEqual(car.ReverseFactor, d.ReverseFactor);
            live.TrySet(TuningKey.HandlingTopSpeedScale, 1.5f);
            live.TrySet(TuningKey.HandlingAccelerationScale, 2f);
            live.TrySet(TuningKey.HandlingBrakingScale, 0.5f);
            live.TrySet(TuningKey.HandlingCoastDragScale, 3f);
            live.TrySet(TuningKey.HandlingReverseScale, 10f);
            d = live.DriveFor(car);
            Assert.AreEqual(car.TopSpeed * 1.5f, d.TopSpeed, 1e-4f);
            Assert.AreEqual(car.Acceleration * 2f, d.Acceleration, 1e-4f);
            Assert.AreEqual(car.Braking * 0.5f, d.Braking, 1e-4f);
            Assert.AreEqual(car.CoastDrag * 3f, d.CoastDrag, 1e-4f);
            Assert.AreEqual(1f, d.ReverseFactor, "reverse is capped at the top speed");
            Object.DestroyImmediate(car);
        }

        [Test]
        public void DriveScales_ArePerCar_SoDifferentCarsKeepTheirDifference()
        {
            var a = ScriptableObject.CreateInstance<CarDefinition>();
            var live = new CarHandlingLive(_config);
            live.TrySet(TuningKey.HandlingTopSpeedScale, 2f);
            Assert.AreEqual(a.TopSpeed * 2f, live.DriveFor(a).TopSpeed, 1e-4f);
            Object.DestroyImmediate(a);
        }

        [Test]
        public void SkidDefaultsMatchTheSpecAndTheLiveValuesFollowTheKeys()
        {
            var live = new CarHandlingLive(_config);
            var s = live.Skid;
            Assert.AreEqual(1.2f, s.SlideThreshold);
            Assert.AreEqual(2f, s.ShiftSlideSpeed);
            Assert.AreEqual(0.8f, s.MaxOpacity);
            Assert.AreEqual(4f, s.HardBrakeSpeed);
            Assert.AreEqual(6f, live.MarkFadeSeconds);
            Assert.AreEqual(40, _config.Skid.DecalCap);
            live.TrySet(TuningKey.HandlingSlideThreshold, 2f);
            live.TrySet(TuningKey.HandlingShiftSlideSpeed, 3f);
            live.TrySet(TuningKey.HandlingMarkOpacity, 0.5f);
            live.TrySet(TuningKey.HandlingMarkFadeSeconds, 9f);
            Assert.AreEqual(2f, live.Skid.SlideThreshold);
            Assert.AreEqual(3f, live.Skid.ShiftSlideSpeed);
            Assert.AreEqual(0.5f, live.Skid.MaxOpacity);
            Assert.AreEqual(9f, live.MarkFadeSeconds);
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
        public void TrySet_RejectsOtherKeys() => Assert.IsFalse(new CarHandlingLive(_config).TrySet(TuningKey.BaseBlessing, 1f));

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
