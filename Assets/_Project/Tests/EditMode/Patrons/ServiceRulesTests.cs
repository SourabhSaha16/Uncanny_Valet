using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;
using UncannyValet.Patrons;
using UnityEngine;

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

        private PatronConfig _config;
        private TuningStore _tuning;
        private ModifierCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<PatronConfig>();
            _tuning = new TuningStore(new EventBus());
            _tuning.Seed(TuningKey.ModifierChanceNeedsCharge, 0.3f);
            _tuning.Seed(TuningKey.ModifierChanceMuddy, 0.3f);
            _catalog = ModifierTestKit.Catalog();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        private ModifierSet Roll(float roll, float progress, PatronPlan plan)
            => new ModifierRoller(_catalog, new PatronTuning(_tuning, _config), plan).Roll(new ScriptedRandom { Next = roll }, progress);

        [Test]
        public void NeedsCharge_OnlyAdvancesInStation_AndStopsAtFull()
        {
            var rule = new ModifierInstance(ModifierId.NeedsCharge, StationKind.Charging, 0.2f, 20f);
            Assert.IsFalse(rule.Advance(false));
            Assert.AreEqual(0.2f, rule.Progress01, 1e-4f);
            Assert.IsTrue(rule.Advance(true));
            Assert.AreEqual(0.25f, rule.Progress01, 1e-4f);
            for (var i = 0; i < 40; i++) rule.Advance(true);
            Assert.IsTrue(rule.IsCleared);
            Assert.AreEqual(1f, rule.Progress01);
            Assert.IsFalse(rule.Advance(true));
        }

        [Test]
        public void Muddy_KeepsProgressWhenCarLeavesEarly()
        {
            var rule = new ModifierInstance(ModifierId.Muddy, StationKind.Wash, 0f, 15f);
            for (var i = 0; i < 5; i++) rule.Advance(true);
            rule.Advance(false);
            Assert.AreEqual(5f / 15f, rule.Progress01, 1e-4f);
            for (var i = 0; i < 10; i++) rule.Advance(true);
            Assert.IsTrue(rule.IsCleared);
        }

        [Test]
        public void Plan_AppliesCapAndGapScale_DefaultsBeforeAnyPlan()
        {
            var plan = new PatronPlan(3);
            Assert.AreEqual(3, plan.MaxWaitingCars);
            plan.Apply(new ShiftPlan(1, 1.5f, true, false));
            Assert.AreEqual(1, plan.MaxWaitingCars);
            Assert.AreEqual(1.5f, plan.ArrivalGapScale);
            Assert.AreEqual(-1, ArrivalGate.FirstFreeSlot(1, plan.MaxWaitingCars, new[] { false, false, false }));
            Assert.AreEqual(0, ArrivalGate.FirstFreeSlot(0, plan.MaxWaitingCars, new[] { false, false, false }));
        }

        [TestCase(0.1f, ModifierId.NeedsCharge)]
        [TestCase(0.4f, ModifierId.Muddy)]
        [TestCase(0.9f, ModifierId.None)]
        public void Roll_GivesAtMostOneModifier_ByChanceInRollOrder(float roll, ModifierId expected)
        {
            var plan = new PatronPlan(3);
            plan.Apply(new ShiftPlan(2, 1f, true, true, lateEvChance: 0.3f));
            var set = Roll(roll, 0f, plan);
            Assert.AreEqual(expected == ModifierId.None ? 0 : 1, set.Count);
            Assert.IsTrue(expected == ModifierId.None ? set.IsEmpty : set.Contains(expected));
        }

        [Test]
        public void Roll_LateRampRaisesOnlyTheLateModifier()
        {
            var plan = new PatronPlan(3);
            plan.Apply(new ShiftPlan(2, 1f, true, true, lateEvChance: 0.5f, lateEvStart: 0.6f));
            Assert.IsTrue(Roll(0.45f, 0f, plan).Contains(ModifierId.Muddy), "early in the shift 0.45 is in the Muddy band");
            var late = Roll(0.45f, 1f, plan);
            Assert.IsTrue(late.Contains(ModifierId.NeedsCharge), "at the end the charge band reaches 0.5");
            Assert.IsFalse(late.Contains(ModifierId.Muddy));
        }

        [Test]
        public void Roll_BeforeAnyPlan_UsesTheLotChancesWithoutARamp()
        {
            var plan = new PatronPlan(3);
            Assert.IsTrue(Roll(0.1f, 1f, plan).Contains(ModifierId.NeedsCharge));
            Assert.IsTrue(Roll(0.4f, 1f, plan).Contains(ModifierId.Muddy));
        }

        [Test]
        public void Roll_ZeroChancesGiveNoModifier()
        {
            _tuning.Set(TuningKey.ModifierChanceNeedsCharge, 0f);
            _tuning.Set(TuningKey.ModifierChanceMuddy, 0f);
            Assert.IsTrue(Roll(0f, 0f, new PatronPlan(3)).IsEmpty);
        }
    }
}
