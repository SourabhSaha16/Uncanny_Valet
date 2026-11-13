using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
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
            public TipBreakdown Explain(in TipInput input) => new TipBreakdown(1, 0, 0, 1, TipZeroReason.None);
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
        public void LateChanceCurve_FlatBeforeStart_RampsToLate()
        {
            Assert.AreEqual(0.1f, LateChanceCurve.Evaluate(0.1f, 0.5f, 0.6f, 0.3f), 1e-4f);
            Assert.AreEqual(0.3f, LateChanceCurve.Evaluate(0.1f, 0.5f, 0.6f, 0.8f), 1e-4f);
            Assert.AreEqual(0.5f, LateChanceCurve.Evaluate(0.1f, 0.5f, 0.6f, 1.2f), 1e-4f);
            Assert.AreEqual(0.5f, LateChanceCurve.Evaluate(0.1f, 0.5f, 1f, 1f), 1e-4f);
            Assert.AreEqual(0.1f, LateChanceCurve.Evaluate(0.1f, PatronPlan.NoLateChance, 0.6f, 1f), 1e-4f, "no plan yet: no ramp");
        }

        [Test]
        public void Roller_UsesLiveGapStayAndLateEvChance_AndSurvivesPlanChange()
        {
            _tuning.Seed(TuningKey.ArrivalGapMaxMinutes, 12f);
            _tuning.Seed(TuningKey.StayMaxMinutes, 30f);
            _tuning.Seed(TuningKey.ModifierChanceNeedsCharge, 0.1f);
            _tuning.Seed(TuningKey.ModifierChanceMuddy, 0f);
            var plan = new PatronPlan(3);
            var roller = new PatronRequestRoller(_config, _live, plan, new Scripted { Next = 0.4f }, ModifierTestKit.Catalog());
            plan.Apply(new ShiftPlan(3, 2f, true, true, 0, 0.5f, 0.5f));
            Assert.AreEqual(24f, roller.NextGap(), 1e-4f);
            Assert.IsFalse(roller.Roll(0.1f).Modifiers.Contains(ModifierId.NeedsCharge));
            Assert.IsTrue(roller.Roll(1f).Modifiers.Contains(ModifierId.NeedsCharge));
            Assert.AreEqual(30f, roller.Roll(0.1f).StayMinutes);
            _tuning.Set(TuningKey.ModifierChanceNeedsCharge, 0.9f);
            plan.Apply(new ShiftPlan(3, 1f, true, true, 0, 0.9f));
            Assert.IsTrue(roller.Roll(0.1f).Modifiers.Contains(ModifierId.NeedsCharge));
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
        public void ClearMinutesChange_AppliesToLaterTicketsOnly()
        {
            var registry = new TicketRegistry(_bus, _clock, new FakeTips(), _live, null, ModifierTestKit.Catalog());
            _tuning.Seed(TuningKey.ChargeMinutes, 20f);
            var charge = ModifierSet.Of(ModifierId.NeedsCharge);
            var first = registry.Create(new CarId(1), new PatronRequest(10f, 20, false, 60f, charge));
            _tuning.Set(TuningKey.ChargeMinutes, 10f);
            _bus.Publish(new TuningChanged(TuningKey.ChargeMinutes, 10f));
            var second = registry.Create(new CarId(2), new PatronRequest(10f, 20, false, 60f, charge));
            first.Modifiers[0].Advance(true);
            second.Modifiers[0].Advance(true);
            Assert.AreEqual(first.Modifiers[0].Progress01 + 0.05f, second.Modifiers[0].Progress01, 1e-4f);
        }

        [Test]
        public void Defaults_AreTwentyFiveMinutesAndImpatientWaitSixtyPercent()
        {
            Assert.AreEqual(25f, PatronRequest.DefaultPickupWindowMinutes);
            Assert.AreEqual(25f, _config.PickupWindowMinutes);
            Assert.AreEqual(25f, _live.WindowFor(false), 1e-4f);
            Assert.AreEqual(15f, _live.WindowFor(true), 1e-4f);
        }

        [Test]
        public void Roller_GivesImpatientPatronsTheShorterLiveWindow()
        {
            var roller = new PatronRequestRoller(_config, _live, new PatronPlan(3), new Scripted { Next = 0.1f }, ModifierTestKit.Catalog());
            Assert.IsTrue(roller.Roll(0.1f).Impatient);
            Assert.AreEqual(15f, roller.Roll(0.1f).PickupWindowMinutes, 1e-4f);
            _tuning.Set(TuningKey.ImpatientWindowScale, 0.5f);
            _tuning.Set(TuningKey.PickupWindowMinutes, 30f);
            Assert.AreEqual(15f, roller.Roll(0.1f).PickupWindowMinutes, 1e-4f);
            var calm = new PatronRequestRoller(_config, _live, new PatronPlan(3), new Scripted { Next = 0.9f }, ModifierTestKit.Catalog());
            Assert.AreEqual(30f, calm.Roll(0.1f).PickupWindowMinutes, 1e-4f);
        }

        [Test]
        public void ImpatientScaleChange_ReachesWaitingImpatientTicketsOnly()
        {
            var registry = new TicketRegistry(_bus, _clock, new FakeTips(), _live);
            var impatient = registry.Create(new CarId(1), new PatronRequest(10f, 20, true, 15f));
            var calm = registry.Create(new CarId(2), new PatronRequest(10f, 20, false, 25f));
            _tuning.Set(TuningKey.ImpatientWindowScale, 0.3f);
            _bus.Publish(new TuningChanged(TuningKey.ImpatientWindowScale, 0.3f));
            impatient.PickupTimer.Start(0f);
            calm.PickupTimer.Start(0f);
            impatient.PickupTimer.Tick(13.6f);
            calm.PickupTimer.Tick(13.6f);
            Assert.IsTrue(impatient.PickupTimer.IsExpired, "13.5 = 0.3 x 25");
            Assert.IsFalse(calm.PickupTimer.IsExpired);
        }

        [Test]
        public void ElapsedFraction_ScalesWithEachPatronsOwnWindow()
        {
            var shortTimer = new TicketTimer(_live.WindowFor(true));
            var longTimer = new TicketTimer(_live.WindowFor(false));
            shortTimer.Start(0f); longTimer.Start(0f);
            shortTimer.Tick(9f); longTimer.Tick(15f);
            Assert.AreEqual(0.6f, shortTimer.Elapsed01, 1e-4f);
            Assert.AreEqual(0.6f, longTimer.Elapsed01, 1e-4f);
        }
    }
}
