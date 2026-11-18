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
        private ThunderStridePower _power;
        private PowerStack _stack;
        private FakeBucket _bucket;

        [SetUp]
        public void SetUp()
        {
            _power = new ThunderStridePower();
            _bucket = new FakeBucket { Open = 3 };
            _stack = new PowerStack(new EventBus(), new IPower[] { _power }, new PowerCooldowns());
        }

        private static PowerLevelValues Values(int level) => PowerDefaults.Values(StoryPower.ThunderStride, level);

        [TestCase(1, 5f, 5f)]
        [TestCase(2, 6f, 7f)]
        public void TheShippedNumbersFollowTheSpec(int level, float walkSpeed, float boost)
        {
            var v = Values(level);
            Assert.AreEqual(walkSpeed, v.Secondary, 0.0001f, "on-foot walking speed x");
            Assert.AreEqual(0f, v.Distance, "the old dash distance is unused");
            Assert.AreEqual(4f, v.Cooldown);
            Assert.AreEqual(boost, v.Duration, 0.0001f);
            Assert.AreEqual(1.8f, v.PushStrength, "car top speed x1.8");
            Assert.AreEqual(10f, v.Width, "car acceleration x10");
        }

        [Test]
        public void ItCanAlwaysStart()
        {
            Assert.IsTrue(_power.CanActivate());
        }

        [Test]
        public void ItLastsTheLevelsBoostTime()
        {
            _stack.TryFire(StoryPower.ThunderStride, 1, Values(1), _bucket);
            _stack.Tick(4.9f);
            Assert.IsTrue(_power.IsActive);
            _stack.Tick(0.2f);
            Assert.IsFalse(_power.IsActive);
        }

        [Test]
        public void OneUseSpendsOneBucket_AndTheCooldownStartsOnlyAfterTheBoostEnds()
        {
            Assert.AreEqual(FireResult.Started, _stack.TryFire(StoryPower.ThunderStride, 1, Values(1), _bucket));
            Assert.AreEqual(1, _bucket.Spent);
            Assert.IsTrue(_power.IsActive);
            Assert.AreEqual(PowerPhase.Active, _stack.PhaseOf(StoryPower.ThunderStride, out _));
            _stack.Tick(4.9f);
            Assert.IsTrue(_stack.Cooldowns.IsReady(StoryPower.ThunderStride), "no cooldown while the boost runs");
            _stack.Tick(0.2f);
            Assert.IsFalse(_power.IsActive);
            Assert.AreEqual(PowerPhase.Cooling, _stack.PhaseOf(StoryPower.ThunderStride, out var left));
            Assert.AreEqual(1f, left, 0.05f, "the cooldown has barely started");
            Assert.AreEqual(FireResult.NotReady, _stack.TryFire(StoryPower.ThunderStride, 1, Values(1), _bucket));
            _stack.Tick(3.9f);
            Assert.AreEqual(PowerPhase.Cooling, _stack.PhaseOf(StoryPower.ThunderStride, out _), "4 s after the use ended");
            _stack.Tick(0.3f);
            Assert.AreEqual(FireResult.Started, _stack.TryFire(StoryPower.ThunderStride, 1, Values(1), _bucket));
            Assert.AreEqual(2, _bucket.Spent);
        }

        [Test]
        public void AFireWhileTheLongBoostRunsIsRefusedSoItNeverStacksOrRestarts()
        {
            _stack.TryFire(StoryPower.ThunderStride, 1, Values(1), _bucket);
            _stack.Tick(3f);
            Assert.AreEqual(FireResult.NotReady, _stack.TryFire(StoryPower.ThunderStride, 1, Values(1), _bucket));
            Assert.AreEqual(1, _bucket.Spent, "no bucket spent on the refused fire");
            _stack.Tick(2.1f);
            Assert.IsFalse(_power.IsActive, "it still ends 5 s after the first use");
        }

        [Test]
        public void TheLevel2UseLastsTheLongerBoostThenTheCooldownStarts()
        {
            _stack.TryFire(StoryPower.ThunderStride, 2, Values(2), _bucket);
            _stack.Tick(6.9f);
            Assert.IsTrue(_power.IsActive);
            _stack.Tick(0.2f);
            Assert.IsFalse(_power.IsActive);
            Assert.AreEqual(PowerPhase.Cooling, _stack.PhaseOf(StoryPower.ThunderStride, out _));
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
            Assert.AreEqual("walking speed (times)", PowerLevelRows.FieldName(StoryPower.ThunderStride, PowerField.Secondary));
            var names = new List<string>();
            foreach (var info in TuningCatalog.All) if (info.Group == "Thunder stride") names.Add(info.Label);
            Assert.AreEqual(10, names.Count, "5 shown numbers x 2 levels (distance is unused)");
            Object.DestroyImmediate(config);
        }
    }
}
