using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Economy;
using UncannyValet.Night;

namespace UncannyValet.Tests.Night
{
    public class TuningTests
    {
        private EventBus _bus;
        private TuningStore _tuning;
        private GameObject _go;
        private NightClock _clock;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _tuning = new TuningStore(_bus);
            _go = new GameObject("clock");
            _clock = _go.AddComponent<NightClock>();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_go);

        private void Seed(float speed = 1f, float length = 180f)
        {
            _tuning.Seed(TuningKey.GameMinutesPerSecond, speed);
            _tuning.Seed(TuningKey.NightLengthMinutes, length);
            _tuning.Seed(TuningKey.PlanMaxWaitingCars, 1f);
            _tuning.Seed(TuningKey.PlanActiveSpots, 4f);
            _clock.Initialize(_bus, tuning: _tuning);
        }

        [Test]
        public void Unlocks_NeverGoAway()
        {
            bool charging = false, wash = false;
            var evOn = false;
            for (var night = 1; night <= 7; night++)
            {
                var p = NightPlanDefaults.For(night);
                if (charging) Assert.IsTrue(p.ChargingOpen, "charging night " + night);
                if (wash) Assert.IsTrue(p.WashOpen, "wash night " + night);
                if (evOn) Assert.Greater(p.EvChance, 0f, "EVs night " + night);
                charging |= p.ChargingOpen; wash |= p.WashOpen; evOn |= p.EvChance > 0f;
            }
        }

        [Test]
        public void LateEvSurge_OnNightsThreeToSeven()
        {
            for (var night = 3; night <= 7; night++)
            {
                var p = NightPlanDefaults.For(night);
                Assert.AreEqual(0.75f, p.LateEvChance, 0.0001f);
                Assert.AreEqual(0.6f, p.LateEvStart, 0.0001f);
            }
        }

        [Test]
        public void ClockSpeed_ChangesApplyImmediately()
        {
            Seed(1f);
            _clock.StartNight(1);
            _clock.Advance(10f);
            _tuning.Set(TuningKey.GameMinutesPerSecond, 3f);
            _clock.Advance(10f);
            Assert.AreEqual(40f, _clock.MinuteOfNight, 0.001f);
        }

        [Test]
        public void ClockLength_ChangeMovesTheRunningNightsEnd()
        {
            Seed(1f, 180f);
            var ended = 0;
            _bus.Subscribe<NightEnded>(_ => ended++);
            _clock.StartNight(1);
            _clock.Advance(50f);
            _tuning.Set(TuningKey.NightLengthMinutes, 60f);
            _clock.Advance(20f);
            Assert.AreEqual(1, ended);
            Assert.AreEqual(60, _clock.NightLengthMinutes);
        }

        [Test]
        public void PlanEdit_RepublishesRunningNightAndAppliesToLaterNights()
        {
            Seed();
            var plans = new List<NightPlan>();
            _bus.Subscribe<NightPlanChanged>(e => plans.Add(e.Plan));
            _clock.StartNight(2);
            _tuning.Set(TuningKey.PlanMaxWaitingCars, 2f);
            _tuning.Set(TuningKey.PlanActiveSpots, 6f);

            Assert.AreEqual(3, plans.Count);
            Assert.AreEqual(2, plans[2].MaxWaitingCars);
            Assert.AreEqual(6, plans[2].ActiveSpots);
            Assert.AreEqual(2, plans[2].Night);
            _clock.StartNight(3);
            Assert.AreEqual(2, plans[3].MaxWaitingCars);
        }

        [Test]
        public void PlanReset_ClearsTheOverride()
        {
            Seed();
            var plans = new List<NightPlan>();
            _bus.Subscribe<NightPlanChanged>(e => plans.Add(e.Plan));
            _clock.StartNight(2);
            _tuning.Set(TuningKey.PlanMaxWaitingCars, 2f);
            _tuning.Reset(TuningKey.PlanMaxWaitingCars);

            Assert.AreEqual(NightPlanDefaults.For(2).MaxWaitingCars, plans[plans.Count - 1].MaxWaitingCars);
        }

        [Test]
        public void NonPlanKey_DoesNotRepublish()
        {
            Seed();
            var count = 0;
            _bus.Subscribe<NightPlanChanged>(_ => count++);
            _clock.StartNight(1);
            _tuning.Set(TuningKey.BaseTip, 20f);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void PlanEdit_BetweenNights_DoesNotRepublish()
        {
            Seed();
            var count = 0;
            _bus.Subscribe<NightPlanChanged>(_ => count++);
            _tuning.Set(TuningKey.PlanEvChance, 0.5f);
            Assert.AreEqual(0, count);
            _clock.StartNight(1);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void Tips_ReadLiveValues()
        {
            _tuning.Seed(TuningKey.BaseTip, 10f);
            _tuning.Seed(TuningKey.ChargeBonus, 15f);
            _tuning.Seed(TuningKey.CleanBonus, 15f);
            _tuning.Seed(TuningKey.SpeedWindowMinutes, 10f);
            var calc = new TipCalculator(ScriptableObject.CreateInstance<TipConfig>(), _tuning);
            Assert.AreEqual(12, calc.Calculate(new TipInput(99, 5f, false)));

            _tuning.Set(TuningKey.BaseTip, 20f);
            _tuning.Set(TuningKey.ChargeBonus, 30f);
            _tuning.Set(TuningKey.SpeedWindowMinutes, 2f);
            Assert.AreEqual(20 + 30 - 2, calc.Calculate(new TipInput(99, 6f, false, chargedEv: true)));
        }
    }
}
