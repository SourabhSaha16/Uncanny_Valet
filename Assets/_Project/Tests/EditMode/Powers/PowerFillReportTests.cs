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
    public class PowerFillReportTests
    {
        private PowerBarConfig _config;

        [SetUp]
        public void SetUp() => _config = ScriptableObject.CreateInstance<PowerBarConfig>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        private DriftFillTracker NewTracker() => new DriftFillTracker(new PowerSettings(_config));

        [Test]
        public void Rule_SkipsUnderHalfAndKeepsHalfAndMore()
        {
            Assert.IsFalse(PowerFillRule.IsWorthShowing(0.49f));
            Assert.IsFalse(PowerFillRule.IsWorthShowing(0f));
            Assert.IsTrue(PowerFillRule.IsWorthShowing(0.5f));
            Assert.IsTrue(PowerFillRule.IsWorthShowing(8f));
        }

        [Test]
        public void End_ReturnsZeroForUnknownCar()
        {
            Assert.AreEqual(0f, NewTracker().End(new CarId(7)));
        }

        [Test]
        public void End_ReturnsEverythingTheBarTook()
        {
            var tracker = NewTracker();
            var car = new CarId(1);
            tracker.Begin(car);
            var total = 0f;
            for (var i = 0; i < 3; i++)
            {
                var offered = tracker.Tick(0.1f);
                tracker.Credit(offered, offered);
                total += offered;
            }
            Assert.Greater(total, 0f);
            Assert.AreEqual(total, tracker.End(car), 0.001f);
        }

        [Test]
        public void Credit_SharesWhatTheBarTookAcrossSlides()
        {
            var tracker = NewTracker();
            tracker.Begin(new CarId(1));
            tracker.Begin(new CarId(2));
            var offered = tracker.Tick(0.1f);
            tracker.Credit(offered * 0.5f, offered);
            Assert.AreEqual(offered * 0.25f, tracker.End(new CarId(1)), 0.001f);
            Assert.AreEqual(offered * 0.25f, tracker.End(new CarId(2)), 0.001f);
        }

        [Test]
        public void Clear_DropsTheTotalSilently()
        {
            var tracker = NewTracker();
            tracker.Begin(new CarId(1));
            var offered = tracker.Tick(0.1f);
            tracker.Credit(offered, offered);
            tracker.Clear();
            Assert.AreEqual(0f, tracker.End(new CarId(1)));
        }

        private sealed class OwnsPower : IRunProgress
        {
            public bool HasCurrentLot => false;
            public LotId CurrentLot => default;
            public IReadOnlyList<StoryPower> EarnedPowers { get; } = new List<StoryPower> { StoryPower.FreezeTime };
            public int PowerLevel(StoryPower power) => 1;
            public int FlashedLevel(StoryPower power) => 0;
            public bool IsCleared(LotId lot) => false;
            public bool IsWorldCleared(World world) => false;
            public int BestBlessings(LotId lot) => 0;
        }

        [Test]
        public void Controller_PublishesPowerFilledWithTheClampedAmountAndSource()
        {
            Services.Reset();
            var bus = new EventBus();
            Services.Register<IEventBus>(bus);
            Services.Register<IRunProgress>(new OwnsPower());
            var filled = new List<PowerFilled>();
            bus.Subscribe<PowerFilled>(e => filled.Add(e));
            var go = new GameObject("powers");
            go.SetActive(false);
            var controller = go.AddComponent<PowerBarController>();
            typeof(PowerBarController).GetField("_config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(controller, _config);
            go.SetActive(true);
            foreach (var m in new[] { "Awake", "Start" })
                typeof(PowerBarController).GetMethod(m, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(controller, null);

            bus.Publish(new TuningChanged(TuningKey.PowerFillPerfectPark, 95f));
            bus.Publish(new PerfectPark(new CarId(1), TicketId.None));
            bus.Publish(new PerfectPark(new CarId(1), TicketId.None));
            bus.Publish(new PerfectPark(new CarId(1), TicketId.None));

            Assert.AreEqual(2, filled.Count);
            Assert.AreEqual(95f, filled[0].Amount, 0.001f);
            Assert.AreEqual(PowerFillSource.PerfectPark, filled[0].Source);
            Assert.AreEqual(5f, filled[1].Amount, 0.001f);
            Object.DestroyImmediate(go);
            Services.Reset();
        }
    }
}
