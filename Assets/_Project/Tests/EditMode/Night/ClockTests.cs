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
            clock.Initialize(bus, 0, 1f);
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
        public void Clock_AdvancesConfiguredMinutesPerSecondAndCatchesUp()
        {
            var bus = new EventBus();
            var go = new GameObject("clock");
            var clock = go.AddComponent<NightClock>();
            clock.Initialize(bus, 180, 1.5f);
            var ticks = new System.Collections.Generic.List<float>();
            bus.Subscribe<NightTick>(e => ticks.Add(e.MinuteOfNight));

            clock.StartNight(1);
            clock.Advance(4f);

            Assert.AreEqual(6f, clock.MinuteOfNight, 0.001f);
            CollectionAssert.AreEqual(new[] { 1f, 2f, 3f, 4f, 5f, 6f }, ticks);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void StartNight_PublishesPlanBeforeStarted()
        {
            var bus = new EventBus();
            var go = new GameObject("clock");
            var clock = go.AddComponent<NightClock>();
            clock.Initialize(bus);
            var order = new System.Collections.Generic.List<string>();
            var planNight = 0;
            bus.Subscribe<NightPlanChanged>(e => { order.Add("plan"); planNight = e.Plan.Night; });
            bus.Subscribe<NightStarted>(_ => order.Add("started"));

            clock.StartNight(3);

            CollectionAssert.AreEqual(new[] { "plan", "started" }, order);
            Assert.AreEqual(3, planNight);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void PlanDefaults_MatchDesignTable()
        {
            var n1 = NightPlanDefaults.For(1);
            Assert.AreEqual(1, n1.MaxWaitingCars);
            Assert.AreEqual(2.0f, n1.ArrivalGapScale);
            var n3 = NightPlanDefaults.For(3);
            Assert.IsTrue(n3.ChargingOpen);
            Assert.IsFalse(n3.WashOpen);
            Assert.AreEqual(0.35f, n3.EvChance);
            Assert.AreEqual(0.75f, n3.LateEvChance);
            Assert.AreEqual(0.6f, n3.LateEvStart);
            var n4 = NightPlanDefaults.For(4);
            Assert.IsTrue(n4.WashOpen);
            Assert.IsTrue(n4.ChargingOpen, "unlocks never go away");
            Assert.AreEqual(0.3f, n4.EvChance);
            Assert.AreEqual(0.4f, n4.DirtyChance);
            Assert.AreEqual(NightPlanDefaults.For(7).NewTonight, NightPlanDefaults.For(5).NewTonight);
        }

        [Test]
        public void PlanDefaults_MatchPacingTable()
        {
            var waiting = new[] { 1, 3, 3, 3, 3, 3, 3 };
            var gap = new[] { 2.0f, 1.5f, 1.3f, 1.2f, 1.1f, 1.0f, 0.9f };
            var spots = new[] { 4, 5, 6, 6, 7, 7, 8 };
            var previousSpots = 0;
            for (var night = 1; night <= 7; night++)
            {
                var plan = NightPlanDefaults.For(night);
                Assert.AreEqual(waiting[night - 1], plan.MaxWaitingCars, "waiting night " + night);
                Assert.AreEqual(gap[night - 1], plan.ArrivalGapScale, 0.0001f, "gap night " + night);
                Assert.AreEqual(spots[night - 1], plan.ActiveSpots, "spots night " + night);
                Assert.GreaterOrEqual(plan.ActiveSpots, previousSpots, "spots never shrink");
                previousSpots = plan.ActiveSpots;
            }
        }

        [Test]
        public void PlanDefaults_ClampToLastEntry()
        {
            Assert.AreEqual(7, NightPlanDefaults.For(12).Night);
            Assert.AreEqual(1, NightPlanDefaults.For(0).Night);
        }

        [Test]
        public void PlanConfig_EmptyListFallsBackToDefaults()
        {
            var config = ScriptableObject.CreateInstance<NightPlanConfig>();
            Assert.AreEqual(7, config.For(9).Night);
            Assert.AreEqual(2, config.For(2).Night);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Clock_EndsOnceStopsTickingAndRestarts()
        {
            var bus = new EventBus();
            var go = new GameObject("clock");
            var clock = go.AddComponent<NightClock>();
            clock.Initialize(bus, 10, 1f);
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
