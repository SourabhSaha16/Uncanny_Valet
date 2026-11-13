using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Powers;
using UnityEngine;

namespace UncannyValet.Tests.Powers
{
    public class PowerBarControllerTests
    {
        private sealed class FakeProgress : IRunProgress
        {
            public readonly List<StoryPower> Owned = new List<StoryPower>();
            public bool HasCurrentLot => false;
            public LotId CurrentLot => default;
            public IReadOnlyList<StoryPower> EarnedPowers => Owned;
            public int PowerLevel(StoryPower power) => Owned.Contains(power) ? 1 : 0;
            public bool IsCleared(LotId lot) => false;
            public bool IsWorldCleared(World world) => false;
            public int BestBlessings(LotId lot) => 0;
        }

        private GameObject _go;
        private PowerBarConfig _config;
        private PowerBarController _controller;
        private EventBus _bus;
        private FakeProgress _progress;
        private readonly List<PowerBarChanged> _changes = new List<PowerBarChanged>();

        [SetUp]
        public void SetUp()
        {
            _changes.Clear();
            Services.Reset();
            _bus = new EventBus();
            _progress = new FakeProgress();
            Services.Register<IEventBus>(_bus);
            Services.Register<IRunProgress>(_progress);
            _bus.Subscribe<PowerBarChanged>(e => _changes.Add(e));
            _config = ScriptableObject.CreateInstance<PowerBarConfig>();
            _go = new GameObject("powers");
            _go.SetActive(false);
            _controller = _go.AddComponent<PowerBarController>();
            typeof(PowerBarController).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_controller, _config);
            _go.SetActive(true);
            typeof(PowerBarController).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_controller, null);
            typeof(PowerBarController).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_controller, null);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_config);
            Services.Reset();
        }

        private void Own() => _progress.Owned.Add(StoryPower.FreezeTime);

        [Test]
        public void WithNoPowerOwned_NothingFillsAndNothingSpends()
        {
            Assert.IsFalse(_controller.IsActive);
            _bus.Publish(new PerfectPark(new CarId(1), TicketId.None));
            Assert.AreEqual(0f, _controller.Bar.Value);
            Assert.IsEmpty(_changes);
            Assert.IsFalse(_controller.TrySpend());
        }

        [Test]
        public void PerfectPark_FillsTheBarAndPublishes()
        {
            Own();
            _bus.Publish(new PerfectPark(new CarId(1), TicketId.None));
            Assert.AreEqual(18f, _controller.Bar.Value, 0.0001f);
            Assert.AreEqual(1, _changes.Count);
            Assert.AreEqual(0, _changes[0].OpenBuckets);
        }

        [Test]
        public void PerfectDropOff_FillsTheBar()
        {
            Own();
            _bus.Publish(new PerfectDropOff(new CarId(1), TicketId.None));
            Assert.AreEqual(10f, _controller.Bar.Value, 0.0001f);
        }

        [Test]
        public void TrySpend_NeedsABucket_AndPublishesTheSpentOne()
        {
            Own();
            _bus.Publish(new PerfectPark(new CarId(1), TicketId.None));
            Assert.IsFalse(_controller.TrySpend());
            _bus.Publish(new PerfectPark(new CarId(1), TicketId.None));
            _changes.Clear();
            Assert.IsTrue(_controller.TrySpend());
            Assert.AreEqual(1, _changes[0].SpentBucket);
            Assert.AreEqual(36f - 33.3f, _controller.Bar.Value, 0.001f);
        }

        [Test]
        public void ShiftStarted_EmptiesTheBar()
        {
            Own();
            _bus.Publish(new PerfectPark(new CarId(1), TicketId.None));
            _bus.Publish(new ShiftStarted());
            Assert.AreEqual(0f, _controller.Bar.Value);
        }

        [Test]
        public void TuningChange_ChangesTheFillAmount()
        {
            Own();
            _bus.Publish(new TuningChanged(TuningKey.PowerFillPerfectPark, 50f));
            _bus.Publish(new PerfectPark(new CarId(1), TicketId.None));
            Assert.AreEqual(50f, _controller.Bar.Value, 0.0001f);
        }

        [Test]
        public void Disabled_StopsListening()
        {
            Own();
            typeof(PowerBarController).GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_controller, null);
            _bus.Publish(new PerfectPark(new CarId(1), TicketId.None));
            Assert.AreEqual(0f, _controller.Bar.Value);
        }
    }
}
