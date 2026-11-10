using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class LiveTuningTests
    {
        private sealed class FakeClock : IGameClock
        {
            public float SecondsRemaining => 0f;
            public float ShiftLengthSeconds => 0f;
            public float ElapsedMinutes { get; set; }
            public bool IsRunning => true;
        }

        private sealed class FakeTips : ITipCalculator
        {
            public int Calculate(in TipInput input) => 1;
            public TipBreakdown Explain(in TipInput input) => new TipBreakdown(1, 0, 0, 0, 1, TipZeroReason.None);
        }

        private sealed class FakeTuning : ITuning
        {
            public readonly Dictionary<TuningKey, float> Values = new Dictionary<TuningKey, float>();
            public float Get(TuningKey key) => Values[key];
            public bool TryGet(TuningKey key, out float value) => Values.TryGetValue(key, out value);
            public void Set(TuningKey key, float value) { Values[key] = value; }
            public void Seed(TuningKey key, float value) { Values[key] = value; }
            public void Reset(TuningKey key) { }
        }

        private sealed class Scripted : IPatronRandom
        {
            public float Next;
            public float Value() => Next;
            public float Range(float min, float max) => max;
        }

        private PatronConfig _config;
        private FakeTuning _tuning;
        private PatronTuning _live;
        private EventBus _bus;
        private FakeClock _clock;

        [SetUp]
        public void SetUp()
        {
            _config = UnityEngine.ScriptableObject.CreateInstance<PatronConfig>();
            _tuning = new FakeTuning();
            _live = new PatronTuning(_tuning, _config);
            _bus = new EventBus(); _clock = new FakeClock();
        }

        [Test]
        public void EvChanceCurve_FlatBeforeStart_RampsToLate()
        {
            Assert.AreEqual(0.1f, EvChanceCurve.Evaluate(0.1f, 0.5f, 0.6f, 0.3f), 1e-4f);
            Assert.AreEqual(0.3f, EvChanceCurve.Evaluate(0.1f, 0.5f, 0.6f, 0.8f), 1e-4f);
            Assert.AreEqual(0.5f, EvChanceCurve.Evaluate(0.1f, 0.5f, 0.6f, 1.2f), 1e-4f);
            Assert.AreEqual(0.5f, EvChanceCurve.Evaluate(0.1f, 0.5f, 1f, 1f), 1e-4f);
        }

        [Test]
        public void Roller_UsesLiveGapStayAndLateEvChance_AndSurvivesPlanChange()
        {
            _tuning.Seed(TuningKey.ArrivalGapMaxMinutes, 12f);
            _tuning.Seed(TuningKey.StayMaxMinutes, 30f);
            var plan = new PatronPlan(3);
            var roller = new PatronRequestRoller(_config, _live, plan, new Scripted { Next = 0.4f });
            plan.Apply(new ShiftPlan(3, 2f, 0.1f, 0f, true, true, 0, 0.5f, 0.5f));
            Assert.AreEqual(24f, roller.NextGap(), 1e-4f);
            Assert.IsFalse(roller.Roll(0.1f).IsEv);
            Assert.IsTrue(roller.Roll(1f).IsEv);
            Assert.AreEqual(30f, roller.Roll(0.1f).StayMinutes);
            plan.Apply(new ShiftPlan(3, 1f, 0.9f, 0f, true, true));
            Assert.IsTrue(roller.Roll(0.1f).IsEv);
            Assert.AreEqual(12f, roller.NextGap(), 1e-4f);
        }

        [Test]
        public void PickupWindowChange_ReachesRunningTimer()
        {
            var registry = new TicketRegistry(_bus, _clock, new FakeTips(), _live);
            var expired = 0;
            _bus.Subscribe<TicketTimerExpired>(e => expired++);
            var t = registry.Create(new CarId(1), new PatronRequest(10f, 20, false, 60f));
            _bus.Publish(new CarParked(t.CarId, 1));
            _clock.ElapsedMinutes = 20f; _bus.Publish(new ShiftTick(20f));
            _clock.ElapsedMinutes = 40f; _bus.Publish(new ShiftTick(40f));
            Assert.AreEqual(0, expired);
            _tuning.Set(TuningKey.PickupWindowMinutes, 15f);
            _bus.Publish(new TuningChanged(TuningKey.PickupWindowMinutes, 15f));
            _clock.ElapsedMinutes = 41f; _bus.Publish(new ShiftTick(41f));
            Assert.AreEqual(1, expired);
        }

        [Test]
        public void ChargeMinutesChange_AppliesToLaterTicketsOnly()
        {
            var registry = new TicketRegistry(_bus, _clock, new FakeTips(), _live);
            _tuning.Seed(TuningKey.ChargeMinutes, 20f);
            var first = registry.Create(new CarId(1), new PatronRequest(10f, 20, false, 60f, true));
            _tuning.Set(TuningKey.ChargeMinutes, 10f);
            _bus.Publish(new TuningChanged(TuningKey.ChargeMinutes, 10f));
            var second = registry.Create(new CarId(2), new PatronRequest(10f, 20, false, 60f, true));
            first.Charge.Advance(true);
            second.Charge.Advance(true);
            Assert.AreEqual(first.Charge.Charge01 + 0.05f, second.Charge.Charge01, 1e-4f);
        }
    }
}
