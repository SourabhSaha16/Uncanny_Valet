using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Powers;
using UnityEngine;

namespace UncannyValet.Tests.Powers
{
    public class ThunderStrideTests
    {
        private bool _driving;
        private ThunderStridePower _power;
        private PowerStack _stack;
        private FakeBucket _bucket;

        [SetUp]
        public void SetUp()
        {
            _driving = false;
            _power = new ThunderStridePower(() => _driving);
            _bucket = new FakeBucket { Open = 3 };
            _stack = new PowerStack(new EventBus(), new IPower[] { _power }, new PowerCooldowns());
        }

        private static PowerLevelValues Values(int level) => PowerDefaults.Values(StoryPower.ThunderStride, level);

        [TestCase(1, 5f, 1.5f)]
        [TestCase(2, 8f, 2.2f)]
        public void TheShippedNumbersFollowTheSpec(int level, float dash, float boost)
        {
            var v = Values(level);
            Assert.AreEqual(dash, v.Distance);
            Assert.AreEqual(0.2f, v.Secondary, 0.0001f);
            Assert.AreEqual(4f, v.Cooldown);
            Assert.AreEqual(boost, v.Duration, 0.0001f);
            Assert.AreEqual(1.5f, v.PushStrength, "car top speed x1.5");
            Assert.AreEqual(2f, v.Width, "car acceleration x2");
        }

        [Test]
        public void ItCanAlwaysStart()
        {
            Assert.IsTrue(_power.CanActivate());
            _driving = true;
            Assert.IsTrue(_power.CanActivate());
        }

        [Test]
        public void OnFootItLastsTheDashTime_InACarTheBoostTime()
        {
            Assert.AreEqual(0.2f, ThunderStridePower.UseSeconds(Values(1), false), 0.0001f);
            Assert.AreEqual(1.5f, ThunderStridePower.UseSeconds(Values(1), true), 0.0001f);
            Assert.AreEqual(2.2f, ThunderStridePower.UseSeconds(Values(2), true), 0.0001f);
        }

        [Test]
        public void OneUseSpendsOneBucket_AndTheCooldownStartsOnlyAfterTheDashEnds()
        {
            Assert.AreEqual(FireResult.Started, _stack.TryFire(StoryPower.ThunderStride, 1, Values(1), _bucket));
            Assert.AreEqual(1, _bucket.Spent);
            Assert.IsTrue(_power.IsActive);
            Assert.AreEqual(PowerPhase.Active, _stack.PhaseOf(StoryPower.ThunderStride, out _));
            _stack.Tick(0.1f);
            Assert.IsTrue(_stack.Cooldowns.IsReady(StoryPower.ThunderStride), "no cooldown while the dash runs");
            _stack.Tick(0.15f);
            Assert.IsFalse(_power.IsActive);
            Assert.AreEqual(PowerPhase.Cooling, _stack.PhaseOf(StoryPower.ThunderStride, out var left));
            Assert.AreEqual(1f, left, 0.05f);
            Assert.AreEqual(FireResult.NotReady, _stack.TryFire(StoryPower.ThunderStride, 1, Values(1), _bucket));
            _stack.Tick(3.9f);
            Assert.AreEqual(PowerPhase.Cooling, _stack.PhaseOf(StoryPower.ThunderStride, out _), "4 s after the use ended");
            _stack.Tick(0.3f);
            Assert.AreEqual(FireResult.Started, _stack.TryFire(StoryPower.ThunderStride, 1, Values(1), _bucket));
            Assert.AreEqual(2, _bucket.Spent);
        }

        [Test]
        public void InACarTheUseLastsTheBoostThenTheCooldownStarts()
        {
            _driving = true;
            _stack.TryFire(StoryPower.ThunderStride, 2, Values(2), _bucket);
            _stack.Tick(2.1f);
            Assert.IsTrue(_power.IsActive);
            _stack.Tick(0.2f);
            Assert.IsFalse(_power.IsActive);
            Assert.AreEqual(PowerPhase.Cooling, _stack.PhaseOf(StoryPower.ThunderStride, out _));
        }

        [Test]
        public void TheDrivingTrackerFollowsTheCarEvents()
        {
            var bus = new EventBus();
            var tracker = new DrivingTracker(bus);
            Assert.IsFalse(tracker.IsDriving);
            bus.Publish(new CarEntered(new CarId(1)));
            Assert.IsTrue(tracker.IsDriving);
            bus.Publish(new CarExited(new CarId(1)));
            Assert.IsFalse(tracker.IsDriving);
            tracker.Dispose();
            bus.Publish(new CarEntered(new CarId(1)));
            Assert.IsFalse(tracker.IsDriving);
        }

        [Test]
        public void TheSecondaryFieldHasItsOwnSeededAdvancedRows()
        {
            var config = ScriptableObject.CreateInstance<PowerBarConfig>();
            var settings = new PowerSettings(config);
            Assert.IsTrue(PowerTuningKeys.TryGetKey(StoryPower.ThunderStride, 2, PowerField.Secondary, out var key));
            Assert.AreEqual(TuningKey.PowerThunderStrideL2Secondary, key);
            Assert.IsTrue(settings.Apply(key, 0.3f));
            settings.TryGetValues(StoryPower.ThunderStride, 2, out var values);
            Assert.AreEqual(0.3f, values.Secondary, 0.0001f);
            Assert.AreEqual("dash time (s)", PowerLevelRows.FieldName(StoryPower.ThunderStride, PowerField.Secondary));
            var names = new List<string>();
            foreach (var info in TuningCatalog.All) if (info.Group == "Thunder stride") names.Add(info.Label);
            Assert.AreEqual(12, names.Count, "6 numbers x 2 levels");
            Object.DestroyImmediate(config);
        }
    }
}
