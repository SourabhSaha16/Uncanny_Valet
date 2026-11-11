using NUnit.Framework;
using UncannyValet.Core.Ids;
using UncannyValet.Powers;
using UnityEngine;

namespace UncannyValet.Tests.Powers
{
    public class PowerMeterTests
    {
        private PowerMeterConfig _config;
        private PowerSettings _settings;
        private PowerMeter _meter;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<PowerMeterConfig>();
            _settings = new PowerSettings(_config);
            _meter = new PowerMeter(_settings);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void StartsEmpty()
        {
            Assert.AreEqual(0, _meter.Level);
            Assert.AreEqual(0f, _meter.Fill01);
        }

        [TestCase(24.9f, 0)]
        [TestCase(25f, 1)]
        [TestCase(54.9f, 1)]
        [TestCase(55f, 2)]
        [TestCase(99.9f, 2)]
        [TestCase(100f, 3)]
        public void Level_FollowsTheThresholds(float value, int level)
        {
            _meter.Add(value);
            Assert.AreEqual(level, _meter.Level);
        }

        [Test]
        public void Add_ClampsToTheFullBar()
        {
            _meter.Add(250f);
            Assert.AreEqual(1f, _meter.Fill01, 0.0001f);
            Assert.IsFalse(_meter.Add(5f));
        }

        [Test]
        public void Add_IgnoresZeroAndNegative()
        {
            Assert.IsFalse(_meter.Add(0f));
            Assert.IsFalse(_meter.Add(-3f));
            Assert.AreEqual(0f, _meter.Value);
        }

        [Test]
        public void Fill01_IsValueOverFull()
        {
            _meter.Add(50f);
            Assert.AreEqual(0.5f, _meter.Fill01, 0.0001f);
        }

        [Test]
        public void Reset_EmptiesTheMeter()
        {
            _meter.Add(60f);
            Assert.IsTrue(_meter.Reset());
            Assert.AreEqual(0, _meter.Level);
            Assert.IsFalse(_meter.Reset());
        }

        [Test]
        public void DefaultsMatchTheSpec()
        {
            Assert.AreEqual(18f, _settings.FillPerfectPark);
            Assert.AreEqual(10f, _settings.FillPerfectDropOff);
            Assert.AreEqual(0.6f, _settings.FillDriftPerSecond, 0.0001f);
            Assert.AreEqual(4f, _settings.FillDriftCap);
            Assert.AreEqual(4f, _settings.SpeedBoostSeconds);
            Assert.AreEqual(1.4f, _settings.WalkMultiplier, 0.0001f);
            Assert.AreEqual(6f, _settings.FreezeSeconds);
            Assert.AreEqual(0f, _settings.FreezeTimeScale);
        }

        [Test]
        public void Drift_FillsPerSecondAndStopsAtTheCapPerSlide()
        {
            var drift = new DriftFillTracker(_settings);
            drift.Begin(new CarId(1));
            Assert.AreEqual(0.6f, drift.Tick(1f), 0.0001f);
            var total = 0.6f;
            for (var i = 0; i < 20; i++) total += drift.Tick(1f);
            Assert.AreEqual(4f, total, 0.0001f);
            Assert.AreEqual(0f, drift.Tick(1f));
        }

        [Test]
        public void Drift_EachSlideHasItsOwnCap()
        {
            var drift = new DriftFillTracker(_settings);
            drift.Begin(new CarId(1));
            for (var i = 0; i < 20; i++) drift.Tick(1f);
            drift.End(new CarId(1));
            drift.Begin(new CarId(1));
            Assert.Greater(drift.Tick(1f), 0f);
        }

        [Test]
        public void Drift_NoSlideNoFill()
        {
            var drift = new DriftFillTracker(_settings);
            Assert.AreEqual(0f, drift.Tick(1f));
            drift.Begin(new CarId(2));
            drift.End(new CarId(2));
            Assert.AreEqual(0, drift.Active);
        }
    }
}
