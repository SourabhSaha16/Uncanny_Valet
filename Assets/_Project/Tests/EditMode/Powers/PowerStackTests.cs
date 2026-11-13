using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Powers;
using UnityEngine;

namespace UncannyValet.Tests.Powers
{
    public class PowerStackTests
    {
        private PowerBarConfig _config;
        private PowerSettings _settings;
        private EventBus _bus;
        private PowerStack _stack;
        private FreezeTimeEffect _freeze;
        private readonly List<PowerKind> _activated = new List<PowerKind>();
        private readonly List<PowerKind> _ended = new List<PowerKind>();

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<PowerBarConfig>();
            _settings = new PowerSettings(_config);
            _activated.Clear();
            _ended.Clear();
            _bus = new EventBus();
            _bus.Subscribe<PowerActivated>(e => _activated.Add(e.Power));
            _bus.Subscribe<PowerEnded>(e => _ended.Add(e.Power));
            _freeze = new FreezeTimeEffect(_settings);
            _stack = new PowerStack(_bus, new IPowerEffect[] { _freeze });
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void Fire_PublishesActivatedAndStartsTheTimer()
        {
            Assert.IsTrue(_stack.Fire(PowerKind.FreezeTime));
            CollectionAssert.AreEqual(new[] { PowerKind.FreezeTime }, _activated);
            Assert.AreEqual(6f, _freeze.Remaining, 0.0001f);
        }

        [Test]
        public void FiringAgain_RefreshesTheTimerWithoutDoubling()
        {
            _stack.Fire(PowerKind.FreezeTime);
            _stack.Tick(3f);
            _stack.Fire(PowerKind.FreezeTime);
            Assert.AreEqual(6f, _freeze.Remaining, 0.0001f);
            _stack.Tick(5.9f);
            Assert.IsEmpty(_ended);
        }

        [Test]
        public void FreezeTime_ScaleFollowsTheSetting()
        {
            Assert.AreEqual(1f, _freeze.Scale);
            Assert.IsFalse(_freeze.IsSlowed);
            _stack.Fire(PowerKind.FreezeTime);
            Assert.AreEqual(0f, _freeze.Scale);
            Assert.IsTrue(_freeze.IsSlowed);
            _settings.Apply(TuningKey.PowerFreezeTimeScale, 0.3f);
            Assert.AreEqual(0.3f, _freeze.Scale, 0.0001f);
            _stack.Tick(6.1f);
            Assert.AreEqual(1f, _freeze.Scale);
            CollectionAssert.AreEqual(new[] { PowerKind.FreezeTime }, _ended);
        }

        [Test]
        public void EndAll_StopsEveryRunningPower()
        {
            _stack.Fire(PowerKind.FreezeTime);
            _stack.EndAll();
            Assert.AreEqual(1, _ended.Count);
            Assert.IsFalse(_freeze.IsActive);
        }

        [Test]
        public void TuningKeys_CoverEveryPowerRowAndApply()
        {
            foreach (var info in UncannyValet.Core.Contracts.PowerRows.Rows)
            {
                Assert.IsTrue(info.Advanced, info.Key.ToString());
                Assert.IsTrue(_settings.Apply(info.Key, info.Min), info.Key.ToString());
            }
        }
    }
}
