using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Powers;
using UnityEngine;

namespace UncannyValet.Tests.Powers
{
    public class PowerStackTests
    {
        private static readonly PowerLevelValues Timed = new PowerLevelValues(10f, 4f);
        private static readonly PowerLevelValues Instant = new PowerLevelValues(5f, 0f);

        private PowerBarConfig _config;
        private PowerSettings _settings;
        private EventBus _bus;
        private PowerStack _stack;
        private FreezeTimeEffect _freeze;
        private TestPower _test;
        private FakeBucket _bucket;
        private readonly List<PowerActivated> _activated = new List<PowerActivated>();
        private readonly List<StoryPower> _ended = new List<StoryPower>();

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<PowerBarConfig>();
            _settings = new PowerSettings(_config);
            _activated.Clear();
            _ended.Clear();
            _bus = new EventBus();
            _bus.Subscribe<PowerActivated>(e => _activated.Add(e));
            _bus.Subscribe<PowerEnded>(e => _ended.Add(e.Power));
            _freeze = new FreezeTimeEffect(_settings);
            _test = new TestPower(StoryPower.ThunderStride);
            _stack = new PowerStack(_bus, new IPower[] { _freeze, _test }, new PowerCooldowns());
            _bucket = new FakeBucket { Open = 3 };
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        private FireResult Fire(StoryPower power, int level, PowerLevelValues values) => _stack.TryFire(power, level, values, _bucket);

        [Test]
        public void FreezeTime_RunsThroughTheFrameworkWithItsBaseValues()
        {
            _settings.TryGetValues(StoryPower.FreezeTime, 1, out var values);
            Assert.AreEqual(6f, values.Duration, 0.0001f, "base freeze lasts 6 seconds");
            Assert.AreEqual(FireResult.Started, Fire(StoryPower.FreezeTime, 1, values));
            Assert.AreEqual(6f, _freeze.Remaining, 0.0001f);
            Assert.AreEqual(StoryPower.FreezeTime, _activated[0].Power);
            Assert.AreEqual(1, _activated[0].Level);
            Assert.AreEqual(1, _bucket.Spent);
            _stack.Tick(6.1f);
            CollectionAssert.AreEqual(new[] { StoryPower.FreezeTime }, _ended);
            Assert.AreEqual(PowerPhase.Cooling, _stack.PhaseOf(StoryPower.FreezeTime, out var left));
            Assert.AreEqual(1f, left, 0.02f);
        }

        [Test]
        public void FreezeTime_ScaleFollowsTheSetting()
        {
            Assert.AreEqual(1f, _freeze.Scale);
            Assert.IsFalse(_freeze.IsSlowed);
            Fire(StoryPower.FreezeTime, 1, new PowerLevelValues(20f, 6f));
            Assert.AreEqual(0f, _freeze.Scale);
            Assert.IsTrue(_freeze.IsSlowed);
            _settings.Apply(TuningKey.PowerFreezeTimeScale, 0.3f);
            Assert.AreEqual(0.3f, _freeze.Scale, 0.0001f);
            _stack.Tick(6.1f);
            Assert.AreEqual(1f, _freeze.Scale);
        }

        [Test]
        public void ANotOwnedPowerOrOneWithoutABehaviourNeverStartsAndSpendsNothing()
        {
            Assert.AreEqual(FireResult.NotOwned, Fire(StoryPower.ThunderStride, 0, Timed));
            Assert.AreEqual(FireResult.NoPower, Fire(StoryPower.MjolnirRecall, 1, Timed));
            Assert.AreEqual(0, _bucket.Spent);
            Assert.IsEmpty(_activated);
        }

        [Test]
        public void WithNoOpenBucket_NothingStarts()
        {
            _bucket.Open = 0;
            Assert.AreEqual(FireResult.NoBucket, Fire(StoryPower.ThunderStride, 1, Timed));
            Assert.AreEqual(0, _test.Activations);
            Assert.IsTrue(_stack.Cooldowns.IsReady(StoryPower.ThunderStride));
        }

        [Test]
        public void WhenThePowerCannotActivate_NoBucketIsSpentAndNoCooldownStarts()
        {
            _test.Allowed = false;
            Assert.AreEqual(FireResult.CannotActivate, Fire(StoryPower.ThunderStride, 1, Instant));
            Assert.AreEqual(0, _bucket.Spent);
            Assert.AreEqual(3, _bucket.Open);
            Assert.IsTrue(_stack.Cooldowns.IsReady(StoryPower.ThunderStride));
            Assert.IsEmpty(_activated);
        }

        [Test]
        public void AnInstantPowerStartsItsCooldownAtOnce_AndWaitsForIt()
        {
            Assert.AreEqual(FireResult.Started, Fire(StoryPower.ThunderStride, 1, Instant));
            Assert.AreEqual(PowerPhase.Cooling, _stack.PhaseOf(StoryPower.ThunderStride, out _));
            Assert.AreEqual(FireResult.NotReady, Fire(StoryPower.ThunderStride, 1, Instant));
            Assert.AreEqual(1, _bucket.Spent, "a refused press costs no bucket");
            _stack.Tick(4.9f);
            Assert.AreEqual(FireResult.NotReady, Fire(StoryPower.ThunderStride, 1, Instant));
            _stack.Tick(0.2f);
            Assert.AreEqual(PowerPhase.Ready, _stack.PhaseOf(StoryPower.ThunderStride, out _));
            Assert.AreEqual(FireResult.Started, Fire(StoryPower.ThunderStride, 1, Instant));
            Assert.IsEmpty(_ended, "an instant power never publishes PowerEnded");
        }

        [Test]
        public void ATimedPowersCooldownStartsOnlyAfterItsUseEnds()
        {
            Fire(StoryPower.ThunderStride, 1, Timed);
            Assert.AreEqual(PowerPhase.Active, _stack.PhaseOf(StoryPower.ThunderStride, out var active));
            Assert.AreEqual(1f, active, 0.0001f);
            _stack.Tick(3f);
            Assert.AreEqual(PowerPhase.Active, _stack.PhaseOf(StoryPower.ThunderStride, out active));
            Assert.AreEqual(0.25f, active, 0.0001f);
            Assert.IsTrue(_stack.Cooldowns.IsReady(StoryPower.ThunderStride), "no cooldown clock while it runs");
            Assert.AreEqual(FireResult.NotReady, Fire(StoryPower.ThunderStride, 1, Timed), "a running power is not fired again");
            _stack.Tick(1.5f);
            CollectionAssert.AreEqual(new[] { StoryPower.ThunderStride }, _ended);
            Assert.AreEqual(PowerPhase.Cooling, _stack.PhaseOf(StoryPower.ThunderStride, out var cooling));
            Assert.AreEqual(1f, cooling, 0.0001f, "the full cooldown runs after the use");
            _stack.Tick(5f);
            _stack.PhaseOf(StoryPower.ThunderStride, out cooling);
            Assert.AreEqual(0.5f, cooling, 0.0001f);
        }

        [Test]
        public void CooldownsCountRealSecondsWhileTheWorldIsFrozen()
        {
            _settings.TryGetValues(StoryPower.FreezeTime, 1, out var values);
            Fire(StoryPower.ThunderStride, 1, Instant);
            Fire(StoryPower.FreezeTime, 1, values);
            Assert.AreEqual(0f, _freeze.Scale);
            _stack.Tick(5.1f);
            Assert.IsTrue(_stack.Cooldowns.IsReady(StoryPower.ThunderStride), "the cooldown ran during the freeze");
        }

        [Test]
        public void ANewLotMakesEveryCooldownReadyAndStopsRunningPowers()
        {
            Fire(StoryPower.ThunderStride, 1, Instant);
            Fire(StoryPower.FreezeTime, 1, new PowerLevelValues(20f, 6f));
            _stack.EndAll();
            Assert.IsFalse(_freeze.IsActive);
            Assert.IsTrue(_stack.Cooldowns.IsReady(StoryPower.ThunderStride));
            Assert.IsTrue(_stack.Cooldowns.IsReady(StoryPower.FreezeTime), "no cooldown carries over");
            CollectionAssert.AreEqual(new[] { StoryPower.FreezeTime }, _ended);
        }

        [Test]
        public void TheLevelAndItsValuesReachThePower()
        {
            var level2 = new PowerLevelValues(7f, 0f, 9f, 3f, 2f);
            Fire(StoryPower.ThunderStride, 2, level2);
            Assert.AreEqual(2, _activated[0].Level);
            Assert.AreEqual(9f, _test.LastValues.Distance);
            Assert.AreEqual(7f, _stack.Cooldowns.Remaining(StoryPower.ThunderStride), 0.0001f);
        }

        [Test]
        public void TuningKeys_CoverEveryPowerRowAndApply()
        {
            foreach (var info in PowerRows.Rows)
            {
                Assert.IsTrue(info.Advanced, info.Key.ToString());
                Assert.IsTrue(_settings.Apply(info.Key, info.Min), info.Key.ToString());
            }
        }
    }
}
