using NUnit.Framework;
using UnityEngine;
using UncannyValet.Core.Events;
using UncannyValet.Night;

namespace UncannyValet.Tests.Night
{
    public class ClockTests
    {
        [TestCase(0f, "22:00")]
        [TestCase(59f, "22:59")]
        [TestCase(60f, "23:00")]
        [TestCase(105f, "23:45")]
        [TestCase(119.9f, "23:59")]
        [TestCase(120f, "00:00")]
        [TestCase(125f, "00:05")]
        [TestCase(180f, "01:00")]
        [TestCase(-5f, "22:00")]
        public void Format(float minute, string expected)
            => Assert.AreEqual(expected, ClockFormat.Format(minute));

        [Test]
        public void Format_CustomStartHour() => Assert.AreEqual("09:30", ClockFormat.Format(30f, 9));

        [Test]
        public void Clock_TicksEachMinuteAndEnds()
        {
            var bus = new EventBus();
            var go = new GameObject("clock");
            var clock = go.AddComponent<NightClock>();
            clock.Initialize(bus);
            int started = 0, ticks = 0, ended = 0;
            bus.Subscribe<NightStarted>(_ => started++);
            bus.Subscribe<NightTick>(_ => ticks++);
            bus.Subscribe<NightEnded>(_ => ended++);

            clock.StartNight(1);
            clock.Advance(100f);
            Assert.AreEqual(100, ticks);
            Assert.IsTrue(clock.IsRunning);
            clock.Advance(1000f);

            Assert.AreEqual(1, started);
            Assert.AreEqual(180, ticks);
            Assert.AreEqual(1, ended);
            Assert.IsFalse(clock.IsRunning);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Clock_EndsOnceStopsTickingAndRestarts()
        {
            var bus = new EventBus();
            var go = new GameObject("clock");
            var clock = go.AddComponent<NightClock>();
            clock.Initialize(bus, 10);
            int ticks = 0, ended = 0, started = 0;
            bus.Subscribe<NightStarted>(_ => started++);
            bus.Subscribe<NightTick>(_ => ticks++);
            bus.Subscribe<NightEnded>(_ => ended++);

            clock.StartNight(1);
            clock.Advance(50f);
            clock.Advance(50f);
            Assert.AreEqual(1, ended);
            Assert.AreEqual(10, ticks);

            clock.StartNight(2);
            Assert.AreEqual(0f, clock.MinuteOfNight);
            clock.Advance(50f);
            Assert.AreEqual(2, started);
            Assert.AreEqual(2, ended);
            Assert.AreEqual(20, ticks);
            Object.DestroyImmediate(go);
        }
    }
}
