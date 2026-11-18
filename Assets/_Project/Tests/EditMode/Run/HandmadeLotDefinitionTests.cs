using NUnit.Framework;
using UncannyValet.Core.Ids;
using UncannyValet.EditorTools;
using UncannyValet.EditorTools.HandmadeLots;
using UncannyValet.Lot.Handmade;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.Tests.Run
{
    /// <summary>The Handmade layout kind of a lot definition (#194): the prefab rule, the requirements the scene builders write into the lot, and the DEV hand-made run assets.</summary>
    public sealed class HandmadeLotDefinitionTests
    {
        private RunFixtures _fx;
        private LotDefinition _lot;
        private GameObject _prefab;

        [SetUp]
        public void SetUp()
        {
            _fx = new RunFixtures();
            _lot = _fx.Lot(World.Midgard, LotKind.Training, "Handmade", "Hand-made");
            _prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HandmadeTestLot.PrefabPath);
        }

        [TearDown] public void TearDown() => _fx.Dispose();

        [Test]
        public void LayoutKinds_KeepTheirStoredNumbers()
        {
            Assert.AreEqual(0, (int)LotLayoutKind.None);
            Assert.AreEqual(1, (int)LotLayoutKind.Diner);
            Assert.AreEqual(2, (int)LotLayoutKind.Handmade);
        }

        [Test]
        public void HandmadeLayout_NeedsItsPrefab()
        {
            RunFixtures.Set(_lot, "_layout", (float)LotLayoutKind.Handmade);
            Assert.IsFalse(_lot.TryValidate(out var problem));
            StringAssert.Contains("lot prefab", problem);
            RunFixtures.SetObject(_lot, "_handmadePrefab", _prefab);
            Assert.IsTrue(_lot.TryValidate(out problem), problem);
            Assert.AreSame(_prefab, _lot.HandmadePrefab);
        }

        [Test]
        public void GeneratedLayout_MustNotCarryAPrefab()
        {
            RunFixtures.SetObject(_lot, "_handmadePrefab", _prefab);
            Assert.IsFalse(_lot.TryValidate(out var problem));
            StringAssert.Contains("not Handmade", problem);
        }

        [Test]
        public void Requirements_FollowTheDefinition()
        {
            var go = new GameObject("lot", typeof(HandmadeLot));
            try
            {
                RunFixtures.Set(_lot, "_queueSlots", 3f);
                RunFixtures.Set(_lot, "_ambientPedestrians", 4f);
                var so = new SerializedObject(_lot);
                so.FindProperty("_ambientTraffic").boolValue = false;
                var chances = so.FindProperty("_modifierChances");
                chances.arraySize = 1;
                chances.GetArrayElementAtIndex(0).FindPropertyRelative("_modifier").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<UncannyValet.Core.Modifiers.ServiceModifierDefinition>(ModifierAssetSetup.NeedsChargePath);
                chances.GetArrayElementAtIndex(0).FindPropertyRelative("_chance").floatValue = 0.3f;
                so.ApplyModifiedPropertiesWithoutUndo();

                HandmadeLotInstance.Apply(go.GetComponent<HandmadeLot>(), _lot);

                var req = go.GetComponent<HandmadeLot>().Requirements;
                Assert.AreEqual(3, req.MinQueueSlots);
                Assert.AreEqual(3, req.MaxQueueSlots);
                Assert.AreEqual(4, req.AmbientPedestrians);
                Assert.IsFalse(req.AmbientTraffic, "definition says " + _lot.AmbientTraffic);
                Assert.IsTrue(req.NeedsCharging);
                Assert.IsFalse(req.NeedsWash);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void DevLot_IsAValidHandmadeCopyOfMidgardTraining_WithAMatchingPrefab()
        {
            var dev = AssetDatabase.LoadAssetAtPath<LotDefinition>(RunAssetSetup.MidgardHandmadePath);
            Assert.IsNotNull(dev, "run Uncanny Valet/Setup/Build Everything");
            Assert.IsTrue(dev.TryValidate(out var problem), problem);
            Assert.AreEqual(LotLayoutKind.Handmade, dev.Layout);
            Assert.AreEqual(new LotId(World.Midgard, LotKind.Training), dev.Id);
            Assert.AreEqual(RunAssetSetup.MidgardHandmadeScene, dev.SceneName);
            var contents = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(dev.HandmadePrefab));
            try
            {
                var data = HandmadeLotReader.Read(contents.GetComponent<HandmadeLot>());
                Assert.AreEqual(dev.QueueSlots, data.Slots.Count, "the prefab's queue slots match the definition");
                Assert.AreEqual(0, data.Bays.FindAll(b => b.Kind != BayKind.Regular).Count, "no EV or wash bay");
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            Assert.AreEqual(0, dev.ModifierChances.Count, "no service modifiers: the test lot has no EV or wash bay");
        }

        [Test]
        public void DevRun_IsTheRealRunWithTheHandmadeLotInPlaceOfTheFirst()
        {
            var real = AssetDatabase.LoadAssetAtPath<RunDefinition>(RunAssetSetup.RunPath);
            var dev = AssetDatabase.LoadAssetAtPath<RunDefinition>(RunAssetSetup.DevHandmadeRunPath);
            Assert.IsNotNull(dev, "run Uncanny Valet/Setup/Build Everything");
            Assert.IsTrue(dev.TryValidate(out var problem), problem);
            Assert.AreEqual(real.Lots.Count, dev.Lots.Count);
            Assert.AreEqual(LotLayoutKind.Handmade, dev.Lots[0].Layout);
            Assert.AreEqual(real.Lots[0].Id, dev.Lots[0].Id);
            for (var i = 1; i < real.Lots.Count; i++) Assert.AreSame(real.Lots[i], dev.Lots[i], "lot " + i);
            foreach (var lot in real.Lots) Assert.AreNotEqual(LotLayoutKind.Handmade, lot.Layout, "the real run stays generated");
        }
    }
}
