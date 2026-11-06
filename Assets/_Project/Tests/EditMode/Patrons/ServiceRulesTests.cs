using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class ServiceRulesTests
    {
        private sealed class ScriptedRandom : IPatronRandom
        {
            public float Next;
            public float Value() => Next;
            public float Range(float min, float max) => min;
        }

        [Test]
        public void Charge_OnlyAdvancesInBay_AndStopsAtFull()
        {
            var rule = new ChargeRule(0.2f, 20f);
            Assert.IsFalse(rule.Advance(false));
            Assert.AreEqual(0.2f, rule.Charge01, 1e-4f);
            Assert.IsTrue(rule.Advance(true));
            Assert.AreEqual(0.25f, rule.Charge01, 1e-4f);
            for (var i = 0; i < 40; i++) rule.Advance(true);
            Assert.IsTrue(rule.IsFull);
            Assert.AreEqual(1f, rule.Charge01);
            Assert.IsFalse(rule.Advance(true));
        }

        [Test]
        public void Wash_KeepsProgressWhenCarLeavesEarly()
        {
            var rule = new WashRule(15f);
            for (var i = 0; i < 5; i++) rule.Advance(true);
            rule.Advance(false);
            Assert.AreEqual(5f / 15f, rule.Clean01, 1e-4f);
            for (var i = 0; i < 10; i++) rule.Advance(true);
            Assert.IsTrue(rule.IsClean);
        }

        [Test]
        public void Plan_AppliesCapAndGapScale_DefaultsBeforeAnyPlan()
        {
            var plan = new PatronPlan(3);
            Assert.AreEqual(3, plan.MaxWaitingCars);
            plan.Apply(new NightPlan(1, 1, 1.5f, 0.3f, 0.2f, true, false, ""));
            Assert.AreEqual(1, plan.MaxWaitingCars);
            Assert.AreEqual(1.5f, plan.ArrivalGapScale);
            Assert.AreEqual(-1, ArrivalGate.FirstFreeSlot(1, plan.MaxWaitingCars, new[] { false, false, false }));
            Assert.AreEqual(0, ArrivalGate.FirstFreeSlot(0, plan.MaxWaitingCars, new[] { false, false, false }));
        }

        [TestCase(0.1f, true, false)]
        [TestCase(0.4f, false, true)]
        [TestCase(0.9f, false, false)]
        public void Plan_RollsTraits_NeverBoth(float roll, bool ev, bool dirty)
        {
            var plan = new PatronPlan(3);
            plan.Apply(new NightPlan(2, 2, 1f, 0.3f, 0.3f, true, true, ""));
            plan.RollTraits(new ScriptedRandom { Next = roll }, 0f, out var isEv, out var isDirty);
            Assert.AreEqual(ev, isEv);
            Assert.AreEqual(dirty, isDirty);
        }
    }
}
