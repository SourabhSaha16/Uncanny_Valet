using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UncannyValet.Economy;
using UncannyValet.Patrons;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.Tests.Run
{
    /// <summary>The lot's list of (modifier, chance): seeded before the dials, validated, and the source of the lot's modifier catalog.</summary>
    public sealed class LotModifierChanceTests
    {
        private RunFixtures _fx;
        private LotDefinition _lot;
        private ModifierCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _fx = new RunFixtures();
            _lot = _fx.Lot(World.Midgard, LotKind.Training);
            _catalog = ModifierTestKit.Catalog();
        }

        [TearDown]
        public void TearDown() => _fx.Dispose();

        private void Chances(params (ModifierId id, float chance)[] entries)
        {
            var so = new SerializedObject(_lot);
            var list = so.FindProperty("_modifierChances");
            list.arraySize = entries.Length;
            for (var i = 0; i < entries.Length; i++)
            {
                _catalog.TryGet(entries[i].id, out var def);
                list.GetArrayElementAtIndex(i).FindPropertyRelative("_modifier").objectReferenceValue = def;
                list.GetArrayElementAtIndex(i).FindPropertyRelative("_chance").floatValue = entries[i].chance;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [Test]
        public void SeedPacing_SeedsEachModifiersChanceKey()
        {
            Chances((ModifierId.NeedsCharge, 0.25f), (ModifierId.Muddy, 0.4f));
            var tuning = new TuningStore(new EventBus());
            _lot.SeedPacing(tuning, 1f);
            Assert.AreEqual(0.25f, tuning.Get(TuningKey.ModifierChanceNeedsCharge), 1e-5f);
            Assert.AreEqual(0.4f, tuning.Get(TuningKey.ModifierChanceMuddy), 1e-5f);
        }

        [Test]
        public void TheLotWins_ThenTheSharedDefaultsApplyWithoutALot()
        {
            var shift = ScriptableObject.CreateInstance<ShiftConfig>();
            var patrons = ScriptableObject.CreateInstance<PatronConfig>();
            var blessings = ScriptableObject.CreateInstance<BlessingConfig>();
            try
            {
                Chances((ModifierId.NeedsCharge, 0.1f), (ModifierId.Muddy, 0.2f));
                var withLot = new TuningStore(new EventBus());
                TuningSeeder.Seed(withLot, shift, patrons, blessings, 6, lot: _lot, modifiers: _lot.BuildModifierCatalog());
                Assert.AreEqual(0.1f, withLot.Get(TuningKey.ModifierChanceNeedsCharge), 1e-5f);
                Assert.AreEqual(0.2f, withLot.Get(TuningKey.ModifierChanceMuddy), 1e-5f);
                var bare = new TuningStore(new EventBus());
                TuningSeeder.Seed(bare, shift, patrons, blessings, 6);
                Assert.AreEqual(ModifierDefaults.NeedsChargeChance, bare.Get(TuningKey.ModifierChanceNeedsCharge), 1e-5f);
                Assert.AreEqual(ModifierDefaults.MuddyChance, bare.Get(TuningKey.ModifierChanceMuddy), 1e-5f);
            }
            finally
            {
                Object.DestroyImmediate(shift);
                Object.DestroyImmediate(patrons);
                Object.DestroyImmediate(blessings);
            }
        }

        [Test]
        public void TheSeederPutsEachModifiersClearTimeAndBonusIntoItsKeys()
        {
            var shift = ScriptableObject.CreateInstance<ShiftConfig>();
            var patrons = ScriptableObject.CreateInstance<PatronConfig>();
            var blessings = ScriptableObject.CreateInstance<BlessingConfig>();
            try
            {
                var tuning = new TuningStore(new EventBus());
                TuningSeeder.Seed(tuning, shift, patrons, blessings, 6, modifiers: _catalog);
                Assert.AreEqual(15f, tuning.Get(TuningKey.WashMinutes));
                Assert.AreEqual(20f, tuning.Get(TuningKey.ChargeMinutes));
                Assert.AreEqual(15f, tuning.Get(TuningKey.CleanBonus));
                Assert.AreEqual(15f, tuning.Get(TuningKey.ChargeBonus));
            }
            finally
            {
                Object.DestroyImmediate(shift);
                Object.DestroyImmediate(patrons);
                Object.DestroyImmediate(blessings);
            }
        }

        [Test]
        public void ValidationRejectsADuplicate_AnEmptyEntry_AndChancesAboveOne()
        {
            Chances((ModifierId.Muddy, 0.3f), (ModifierId.Muddy, 0.2f));
            Assert.IsFalse(_lot.TryValidate(out var problem));
            StringAssert.Contains("listed twice", problem);
            Chances((ModifierId.Muddy, 0.6f), (ModifierId.NeedsCharge, 0.6f));
            Assert.IsFalse(_lot.TryValidate(out problem));
            StringAssert.Contains("more than 1", problem);
            var so = new SerializedObject(_lot);
            so.FindProperty("_modifierChances").arraySize = 1;
            so.FindProperty("_modifierChances").GetArrayElementAtIndex(0).FindPropertyRelative("_modifier").objectReferenceValue = null;
            so.ApplyModifiedPropertiesWithoutUndo();
            Assert.IsFalse(_lot.TryValidate(out problem));
            StringAssert.Contains("no modifier", problem);
        }

        [Test]
        public void TheLotsCatalogFollowsItsListOrder()
        {
            Chances((ModifierId.NeedsCharge, 0.3f), (ModifierId.Muddy, 0.3f));
            var catalog = _lot.BuildModifierCatalog();
            Assert.AreEqual(2, catalog.All.Count);
            Assert.AreEqual(ModifierId.NeedsCharge, catalog.All[0].Id);
            Assert.AreEqual(ModifierId.Muddy, catalog.All[1].Id);
        }

        [TestCase(RunAssetSetupPaths.Training)]
        [TestCase(RunAssetSetupPaths.Crazy)]
        public void TheShippedMidgardLots_ListBothModifiers_WithChanceZero_NoServicesInMidgard(string path)
        {
            var lot = AssetDatabase.LoadAssetAtPath<LotDefinition>(path);
            Assert.IsNotNull(lot, path);
            Assert.AreEqual(2, lot.ModifierChances.Count);
            Assert.AreEqual(ModifierId.NeedsCharge, lot.ModifierChances[0].Modifier.Id);
            Assert.AreEqual(0f, lot.ModifierChances[0].Chance, 1e-5f);
            Assert.AreEqual(ModifierId.Muddy, lot.ModifierChances[1].Modifier.Id);
            Assert.AreEqual(0f, lot.ModifierChances[1].Chance, 1e-5f);
            Assert.IsTrue(lot.TryValidate(out var problem), problem);
        }

        private static class RunAssetSetupPaths
        {
            public const string Training = "Assets/_Project/Data/Run/MidgardTrainingLot.asset";
            public const string Crazy = "Assets/_Project/Data/Run/MidgardCrazyLot.asset";
        }
    }
}
