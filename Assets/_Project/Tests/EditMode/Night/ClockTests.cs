using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Night;

namespace UncannyValet.Tests.Night
{
    public class ClockTests
    {
        private EventBus _bus;
        private GameObject _go;
        private ShiftClock _clock;
        private ShiftConfig _config;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _config = ScriptableObject.CreateInstance<ShiftConfig>();
            _go = new GameObject("clock");
            _clock = _go.AddComponent<ShiftClock>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_config);
        }

        [TestCase(240f, "4:00")]
        [TestCase(225.4f, "3:46")]
        [TestCase(61f, "1:01")]
        [TestCase(30f, "0:30")]
        [TestCase(0.2f, "0:01")]
        [TestCase(0f, "0:00")]
        [TestCase(-5f, "0:00")]
        public void Format(float seconds, string expected)
            => Assert.AreEqual(expected, ClockFormat.Format(seconds));

        [Test]
        public void Timer_ReportsFinalStretchOnceAndEndOnce()
        {
            var timer = new ShiftTimer(100f, 30f);
            Assert.AreEqual(ShiftTimerStep.None, timer.Advance(60f));
            Assert.AreEqual(ShiftTimerStep.EnteredFinalStretch, timer.Advance(10f));
            Assert.AreEqual(ShiftTimerStep.None, timer.Advance(10f));
            Assert.AreEqual(ShiftTimerStep.Ended, timer.Advance(20f));
            Assert.AreEqual(ShiftTimerStep.None, timer.Advance(5f));
            Assert.IsTrue(timer.IsEnded);
            Assert.AreEqual(0f, timer.SecondsRemaining);
        }

        [Test]
        public void Timer_BigStep_ReportsBothAtOnce()
        {
            var timer = new ShiftTimer(100f, 30f);
            Assert.AreEqual(ShiftTimerStep.EnteredFinalStretch | ShiftTimerStep.Ended, timer.Advance(500f));
        }

        [Test]
        public void Timer_ShorterThanFinalStretch_NeverReportsIt()
        {
            var timer = new ShiftTimer(20f, 30f);
            Assert.AreEqual(ShiftTimerStep.Ended, timer.Advance(25f));
        }

        [Test]
        public void ShiftConfig_DefaultsToFourMinutes()
        {
            Assert.AreEqual(240f, _config.ShiftLengthSeconds);
            Assert.AreEqual(30f, _config.FinalStretchSeconds);
            Assert.AreEqual(150, _config.OneStarScore);
            Assert.AreEqual(300, _config.TwoStarScore);
            Assert.AreEqual(450, _config.ThreeStarScore);
            Assert.AreEqual(15, _config.ComplaintPenalty);
            Assert.AreEqual(4, _config.MaxComboMultiplier);
        }

        [Test]
        public void Clock_CountsDownTicksEachMinuteAndEndsOnce()
        {
            _clock.Initialize(_bus, _config);
            int started = 0, ticks = 0, last30 = 0, ended = 0;
            _bus.Subscribe<ShiftStarted>(_ => started++);
            _bus.Subscribe<ShiftTick>(_ => ticks++);
            _bus.Subscribe<ShiftLast30Seconds>(_ => last30++);
            _bus.Subscribe<ShiftEnded>(_ => ended++);

            _clock.StartShift();
            Assert.AreEqual(240f, _clock.SecondsRemaining);
            _clock.Advance(100f);
            Assert.AreEqual(100, ticks);
            Assert.AreEqual(140f, _clock.SecondsRemaining, 0.001f);
            Assert.IsTrue(_clock.IsRunning);
            Assert.AreEqual(0, last30);
            _clock.Advance(115f);
            Assert.AreEqual(1, last30);
            _clock.Advance(1000f);

            Assert.AreEqual(1, started);
            Assert.AreEqual(1, last30);
            Assert.AreEqual(1, ended);
            Assert.AreEqual(240, ticks);
            Assert.IsFalse(_clock.IsRunning);
            Assert.AreEqual(0f, _clock.SecondsRemaining);
        }

        [Test]
        public void Clock_UsesConfiguredMinutesPerSecondAndCatchesUp()
        {
            var tuning = new TuningStore(_bus);
            tuning.Seed(TuningKey.GameMinutesPerSecond, 1.5f);
            _clock.Initialize(_bus, _config, tuning);
            var ticks = new List<float>();
            _bus.Subscribe<ShiftTick>(e => ticks.Add(e.ElapsedMinutes));

            _clock.StartShift();
            _clock.Advance(4f);

            Assert.AreEqual(6f, _clock.ElapsedMinutes, 0.001f);
            CollectionAssert.AreEqual(new[] { 1f, 2f, 3f, 4f, 5f, 6f }, ticks);
        }

        [Test]
        public void StartShift_PublishesPlanBeforeStarted()
        {
            _clock.Initialize(_bus, _config);
            var order = new List<string>();
            _bus.Subscribe<ShiftPlanChanged>(_ => order.Add("plan"));
            _bus.Subscribe<ShiftStarted>(_ => order.Add("started"));

            _clock.StartShift();

            CollectionAssert.AreEqual(new[] { "plan", "started" }, order);
        }

        [Test]
        public void Clock_RestartsWithFullTime()
        {
            _clock.Initialize(_bus, _config);
            int ended = 0, ticks = 0;
            _bus.Subscribe<ShiftEnded>(_ => ended++);
            _bus.Subscribe<ShiftTick>(_ => ticks++);

            _clock.StartShift();
            _clock.Advance(500f);
            Assert.AreEqual(1, ended);

            _clock.StartShift();
            Assert.AreEqual(0f, _clock.ElapsedMinutes);
            Assert.AreEqual(240f, _clock.SecondsRemaining);
            _clock.Advance(500f);
            Assert.AreEqual(2, ended);
            Assert.AreEqual(480, ticks);
        }

        [Test]
        public void Advance_BeforeStart_DoesNothing()
        {
            _clock.Initialize(_bus, _config);
            var any = 0;
            _bus.Subscribe<ShiftTick>(_ => any++);
            _bus.Subscribe<ShiftEnded>(_ => any++);
            _clock.Advance(1000f);
            Assert.AreEqual(0, any);
        }
    }
}
