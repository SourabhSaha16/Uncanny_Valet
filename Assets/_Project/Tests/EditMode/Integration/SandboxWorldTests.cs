using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Bootstrap.Sandbox;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    /// <summary>The sandbox's pure rules: choosing a world, applying a world's driving numbers live, and the boundary ring. Refs #180.</summary>
    public class SandboxWorldTests
    {
        private const float GameMinutesPerSecond = 1f;
        private const float SlowMud = 0.3f;
        private const float DefaultMud = MudRows.DefaultTopSpeedScale;
        private const string WorldsPath = "Assets/_Project/Data/Sandbox/SandboxWorldsConfig.asset";

        private readonly List<Object> _made = new List<Object>();
        private EventBus _bus;
        private TuningStore _store;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _store = new TuningStore(_bus);
            MudRows.SeedDefaults(_store);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var made in _made) Object.DestroyImmediate(made);
            _made.Clear();
        }

        [Test]
        public void Selection_StartsOnTheFirstWorld_AndKeyNumbersAreOneBased()
        {
            var selection = new SandboxWorldSelection(3);
            Assert.AreEqual(0, selection.Current);
            Assert.IsTrue(selection.TrySelectNumber(3));
            Assert.AreEqual(2, selection.Current);
        }

        [Test]
        public void Selection_RefusesUnknownAndCurrentWorlds()
        {
            var selection = new SandboxWorldSelection(2);
            Assert.IsFalse(selection.TrySelect(0), "already shown");
            Assert.IsFalse(selection.TrySelect(2), "past the end");
            Assert.IsFalse(selection.TrySelect(-1));
            Assert.IsFalse(selection.TrySelectNumber(0), "keys start at 1");
            Assert.AreEqual(0, selection.Current);
        }

        [Test]
        public void Selection_WithNoWorlds_SelectsNothing()
        {
            var selection = new SandboxWorldSelection(0);
            Assert.IsFalse(selection.TrySelect(0));
            Assert.AreEqual(0, selection.Current);
        }

        [Test]
        public void ApplyingAWorldWithMud_WritesItsMudNumbers_AndPublishesThem()
        {
            var changed = new List<TuningKey>();
            _bus.Subscribe<TuningChanged>(e => changed.Add(e.Key));
            SandboxWorldTuning.Apply(_store, LotWithMud(SlowMud), GameMinutesPerSecond);
            Assert.AreEqual(SlowMud, _store.Get(TuningKey.MudTopSpeedScale), 1e-5f);
            Assert.Contains(TuningKey.MudTopSpeedScale, changed, "a live car reads the key on the next tick");
        }

        [Test]
        public void ApplyingAWorldWithoutMud_RestoresTheMudDefaults()
        {
            SandboxWorldTuning.Apply(_store, LotWithMud(SlowMud), GameMinutesPerSecond);
            SandboxWorldTuning.Apply(_store, MakeLot(), GameMinutesPerSecond);
            Assert.AreEqual(DefaultMud, _store.Get(TuningKey.MudTopSpeedScale), 1e-5f);
        }

        [Test]
        public void SetThroughTuning_SeedWritesLiveValue_ButKeepsTheStoresSeedForReset()
        {
            _store.Seed(TuningKey.MudTopSpeedScale, DefaultMud);
            new SetThroughTuning(_store).Seed(TuningKey.MudTopSpeedScale, SlowMud);
            Assert.AreEqual(SlowMud, _store.Get(TuningKey.MudTopSpeedScale), 1e-5f);
            _store.Reset(TuningKey.MudTopSpeedScale);
            Assert.AreEqual(DefaultMud, _store.Get(TuningKey.MudTopSpeedScale), 1e-5f);
        }

        [Test]
        public void Boundary_CoversTheFourSidesAndCorners_WithoutOverlappingTheArea()
        {
            var area = new Rect(-10f, -5f, 20f, 12f);
            var segments = SandboxBoundary.Segments(area, 2f);
            Assert.AreEqual(4, segments.Length);
            foreach (var segment in segments) Assert.IsFalse(segment.Overlaps(area), "a segment would block the lot itself");
            var union = Rect.MinMaxRect(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
            foreach (var s in segments) union = Rect.MinMaxRect(Mathf.Min(union.xMin, s.xMin), Mathf.Min(union.yMin, s.yMin), Mathf.Max(union.xMax, s.xMax), Mathf.Max(union.yMax, s.yMax));
            Assert.AreEqual(new Rect(-12f, -7f, 24f, 16f), union);
        }

        [Test]
        public void TheShippedWorldList_HasMidgardAndVanaheim_AndEveryLotValidates()
        {
            var config = AssetDatabase.LoadAssetAtPath<SandboxWorldsConfig>(WorldsPath);
            Assert.IsNotNull(config, "Run Uncanny Valet/Setup/Build Everything to create " + WorldsPath);
            var worlds = new HashSet<World>();
            foreach (var lot in config.Worlds)
            {
                Assert.IsNotNull(lot);
                Assert.IsTrue(lot.TryValidate(out var problem), problem);
                worlds.Add(lot.Id.World);
            }
            CollectionAssert.IsSupersetOf(worlds, new[] { World.Midgard, World.Vanaheim });
        }

        private LotDefinition MakeLot()
        {
            var lot = ScriptableObject.CreateInstance<LotDefinition>();
            _made.Add(lot);
            return lot;
        }

        private LotDefinition LotWithMud(float topSpeedScale)
        {
            var mud = ScriptableObject.CreateInstance<MudPatchModifier>();
            _made.Add(mud);
            var mudSo = new SerializedObject(mud);
            mudSo.FindProperty("_topSpeedScale").floatValue = topSpeedScale;
            mudSo.ApplyModifiedPropertiesWithoutUndo();
            var lot = MakeLot();
            var so = new SerializedObject(lot);
            var list = so.FindProperty("_modifiers");
            list.arraySize = 1;
            list.GetArrayElementAtIndex(0).objectReferenceValue = mud;
            so.ApplyModifiedPropertiesWithoutUndo();
            return lot;
        }
    }
}
