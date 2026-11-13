using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UncannyValet.Core.Config;
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
        private ShiftClock _clock;
        private ShiftConfig _config;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _tuning = new TuningStore(_bus);
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

        private void Seed(float speed = 1f, float length = 240f)
        {
            _tuning.Seed(TuningKey.GameMinutesPerSecond, speed);
            _tuning.Seed(TuningKey.ShiftLengthSeconds, length);
            _tuning.Seed(TuningKey.PlanMaxWaitingCars, 1f);
            _tuning.Seed(TuningKey.PlanActiveSpots, 4f);
            _clock.Initialize(_bus, _config, _tuning);
        }

        [Test]
        public void ClockSpeed_ChangesApplyImmediately()
        {
            Seed(1f);
            _clock.StartShift();
            _clock.Advance(10f);
            _tuning.Set(TuningKey.GameMinutesPerSecond, 3f);
            _clock.Advance(10f);
            Assert.AreEqual(40f, _clock.ElapsedMinutes, 0.001f);
        }

        [Test]
        public void ShiftLength_ComesFromTuningAndAppliesFromTheNextShift()
        {
            Seed(1f, 100f);
            var ended = 0;
            _bus.Subscribe<ShiftEnded>(_ => ended++);
            _clock.StartShift();
            Assert.AreEqual(100f, _clock.ShiftLengthSeconds);
            _tuning.Set(TuningKey.ShiftLengthSeconds, 60f);
            Assert.AreEqual(100f, _clock.ShiftLengthSeconds, "the running shift keeps its length");
            _clock.Advance(70f);
            Assert.AreEqual(0, ended);

            _clock.Advance(40f);
            Assert.AreEqual(1, ended);
            _clock.StartShift();
            Assert.AreEqual(60f, _clock.SecondsRemaining);
        }

        [Test]
        public void PlanEdit_RepublishesRunningShiftAndAppliesToLaterShifts()
        {
            Seed();
            var plans = new List<ShiftPlan>();
            _bus.Subscribe<ShiftPlanChanged>(e => plans.Add(e.Plan));
            _clock.StartShift();
            _tuning.Set(TuningKey.PlanMaxWaitingCars, 2f);
            _tuning.Set(TuningKey.PlanActiveSpots, 6f);

            Assert.AreEqual(3, plans.Count);
            Assert.AreEqual(2, plans[2].MaxWaitingCars);
            Assert.AreEqual(6, plans[2].ActiveSpots);
            _clock.Advance(1000f);
            _clock.StartShift();
            Assert.AreEqual(2, plans[3].MaxWaitingCars);
        }

        [Test]
        public void PlanReset_ClearsTheOverride()
        {
            Seed();
            var plans = new List<ShiftPlan>();
            _bus.Subscribe<ShiftPlanChanged>(e => plans.Add(e.Plan));
            _clock.StartShift();
            _tuning.Set(TuningKey.PlanMaxWaitingCars, 2f);
            _tuning.Reset(TuningKey.PlanMaxWaitingCars);

            Assert.AreEqual(_config.Plan.MaxWaitingCars, plans[plans.Count - 1].MaxWaitingCars);
        }

        [Test]
        public void NonPlanKey_DoesNotRepublish()
        {
            Seed();
            var count = 0;
            _bus.Subscribe<ShiftPlanChanged>(_ => count++);
            _clock.StartShift();
            _tuning.Set(TuningKey.BaseTip, 20f);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void PlanEdit_BeforeTheShift_DoesNotRepublish()
        {
            Seed();
            var count = 0;
            _bus.Subscribe<ShiftPlanChanged>(_ => count++);
            _tuning.Set(TuningKey.PlanLateEvChance, 0.7f);
            Assert.AreEqual(0, count);
            _clock.StartShift();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void Tips_ReadLiveValues()
        {
            _tuning.Seed(TuningKey.BaseTip, 10f);
            _tuning.Seed(TuningKey.ChargeBonus, 15f);
            _tuning.Seed(TuningKey.CleanBonus, 15f);
            _tuning.Seed(TuningKey.SpeedWindowMinutes, 10f);
            var calc = new TipCalculator(ScriptableObject.CreateInstance<TipConfig>(), _tuning, ModifierTestKit.Catalog());
            Assert.AreEqual(12, calc.Calculate(new TipInput(99, 5f, false)));

            _tuning.Set(TuningKey.BaseTip, 20f);
            _tuning.Set(TuningKey.ChargeBonus, 30f);
            _tuning.Set(TuningKey.SpeedWindowMinutes, 2f);
            Assert.AreEqual(20 + 30 - 2, calc.Calculate(new TipInput(99, 6f, false, clearedModifiers: UncannyValet.Core.Modifiers.ModifierSet.Of(UncannyValet.Core.Modifiers.ModifierId.NeedsCharge))));
        }
    }
}
