using NUnit.Framework;
using UnityEngine;
using UncannyValet.Core.Config;
using UncannyValet.Core.Events;
using UncannyValet.Night;

namespace UncannyValet.Tests.Night
{
    /// <summary>The clock moves by world seconds, so freeze time (scale 0) stops it and a slow scale advances it proportionally.</summary>
    public class ClockWorldTimeTests
    {
        private EventBus _bus;
        private GameObject _go;
        private ShiftClock _clock;
        private ShiftConfig _config;
        private int _ticks;

        [SetUp]
        public void SetUp()
        {
            _ticks = 0;
            _bus = new EventBus();
            _config = ScriptableObject.CreateInstance<ShiftConfig>();
            _go = new GameObject("clock");
            _clock = _go.AddComponent<ShiftClock>();
            _clock.Initialize(_bus, _config);
            _bus.Subscribe<ShiftTick>(_ => _ticks++);
            _clock.StartShift();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void ZeroWorldSeconds_DoNotMoveTheClockOrTickAnyMinute()
        {
            _clock.Advance(0f);
            Assert.AreEqual(0f, _clock.ElapsedMinutes);
            Assert.AreEqual(_clock.ShiftLengthSeconds, _clock.SecondsRemaining);
            Assert.AreEqual(0, _ticks);
        }

        [Test]
        public void SlowWorldSeconds_AdvanceInProportion()
        {
            _clock.Advance(10f * 0.3f);
            var slow = _clock.ElapsedMinutes;
            _clock.StartShift();
            _clock.Advance(10f);
            Assert.AreEqual(slow / 0.3f, _clock.ElapsedMinutes, 0.001f);
            Assert.AreEqual(_clock.ShiftLengthSeconds - 10f, _clock.SecondsRemaining, 0.001f);
        }
    }
}
