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
    /// <summary>The mud modifier asset: its patch list reaches the lot's world spec, its numbers seed the F2 rows, and the Muddy arrival share comes from the lot.</summary>
    public sealed class MudPatchModifierTests
    {
        private RunFixtures _fx;
        private LotDefinition _lot;
        private MudPatchModifier _mud;

        [SetUp]
        public void SetUp()
        {
            _fx = new RunFixtures();
            _lot = _fx.Lot(World.Vanaheim, LotKind.Crazy);
            _mud = ScriptableObject.CreateInstance<MudPatchModifier>();
            var so = new SerializedObject(_mud);
            var list = so.FindProperty("_patches");
            list.arraySize = 2;
            Patch(list, 0, new Vector2(-3f, 4f), 3f);
            Patch(list, 1, new Vector2(5f, -2f), 2.5f);
            so.FindProperty("_topSpeedScale").floatValue = 0.5f;
            so.ApplyModifiedPropertiesWithoutUndo();
            SetModifiers(_lot, _mud);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_mud);
            _fx.Dispose();
        }

        private static void Patch(SerializedProperty list, int index, Vector2 center, float size)
        {
            var entry = list.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("_center").vector2Value = center;
            entry.FindPropertyRelative("_size").floatValue = size;
        }

        private static void SetModifiers(LotDefinition lot, params LotModifier[] modifiers)
        {
            var so = new SerializedObject(lot);
            var list = so.FindProperty("_modifiers");
            list.arraySize = modifiers.Length;
            for (var i = 0; i < modifiers.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = modifiers[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [Test]
        public void WorldSpec_ListsThePatchesOfTheLotsMudModifier()
        {
            var spec = _lot.BuildWorldSpec();
            Assert.AreEqual(2, spec.MudPatches.Count);
            Assert.AreEqual(new Vector2(-3f, 4f), spec.MudPatches[0].Center);
            Assert.AreEqual(2.5f, spec.MudPatches[1].Size);
        }

        [Test]
        public void WorldSpec_OfALotWithoutMud_IsEmpty()
        {
            SetModifiers(_lot);
            Assert.AreEqual(0, _lot.BuildWorldSpec().MudPatches.Count);
        }

        [Test]
        public void SeedPacing_SeedsTheModifiersMudNumbersOverTheDefaults()
        {
            var tuning = new TuningStore(new EventBus());
            MudRows.SeedDefaults(tuning);
            Assert.AreEqual(MudRows.DefaultTopSpeedScale, tuning.Get(TuningKey.MudTopSpeedScale));
            _lot.SeedPacing(tuning, 1f);
            Assert.AreEqual(0.5f, tuning.Get(TuningKey.MudTopSpeedScale));
            Assert.AreEqual(MudRows.DefaultGripLossKept, tuning.Get(TuningKey.MudGripLossKept));
        }

        [Test]
        public void TheSeederAlwaysSeedsTheMudRows_EvenForALotWithoutMud()
        {
            SetModifiers(_lot);
            var shift = ScriptableObject.CreateInstance<ShiftConfig>();
            var patrons = ScriptableObject.CreateInstance<PatronConfig>();
            var blessings = ScriptableObject.CreateInstance<BlessingConfig>();
            try
            {
                var tuning = new TuningStore(new EventBus());
                TuningSeeder.Seed(tuning, shift, patrons, blessings, 6, lot: _lot, modifiers: _lot.BuildModifierCatalog());
                Assert.AreEqual(MudRows.DefaultTopSpeedScale, tuning.Get(TuningKey.MudTopSpeedScale));
                Assert.AreEqual(MudRows.DefaultReleaseSeconds, tuning.Get(TuningKey.MudReleaseSeconds));
            }
            finally
            {
                Object.DestroyImmediate(shift);
                Object.DestroyImmediate(patrons);
                Object.DestroyImmediate(blessings);
            }
        }

        [TestCase(0.3f)]
        [TestCase(0.6f)]
        [TestCase(0f)]
        public void MuddyArrivalShare_FollowsTheLotsChance(float share)
        {
            var catalog = ModifierTestKit.Catalog();
            catalog.TryGet(ModifierId.Muddy, out var muddy);
            var so = new SerializedObject(_lot);
            var chances = so.FindProperty("_modifierChances");
            chances.arraySize = 1;
            chances.GetArrayElementAtIndex(0).FindPropertyRelative("_modifier").objectReferenceValue = muddy;
            chances.GetArrayElementAtIndex(0).FindPropertyRelative("_chance").floatValue = share;
            so.ApplyModifiedPropertiesWithoutUndo();
            var tuning = new TuningStore(new EventBus());
            _lot.SeedPacing(tuning, 1f);
            Assert.AreEqual(share, tuning.Get(muddy.ChanceKey), 1e-5f);
            Assert.IsTrue(_lot.TryValidate(out var problem), problem);
        }

        [Test]
        public void TheDefaultValuesMatchTheOwnersDecisions()
        {
            Assert.AreEqual(0.6f, _mud.AccelerationScale);
            Assert.AreEqual(0.4f, _mud.GripLossKept);
            Assert.AreEqual(0.3f, _mud.ReleaseSeconds);
        }
    }
}
