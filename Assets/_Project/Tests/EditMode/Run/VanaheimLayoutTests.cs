using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.Lot;
using UncannyValet.Run;
using UncannyValet.UI.Screens;
using UnityEditor;
using UnityEngine;
using static UncannyValet.Tests.Run.VanaheimAssets;

namespace UncannyValet.Tests.Run
{
    /// <summary>The shipped Vanaheim lots (task 107): where they sit in the run, the bays, the wash bays, the widened lot and the mud patches.</summary>
    public sealed class VanaheimLayoutTests
    {
        private const string TrainingScene = "Assets/_Project/Scenes/Lot_VanaheimTraining.unity";
        private const string CrazyScene = "Assets/_Project/Scenes/Lot_VanaheimCrazy.unity";
        private const float MaxWashBayGap = 1f;

        [Test]
        public void TheRun_ListsTheVanaheimLotsAfterMidgardsTwo_AndValidates()
        {
            var run = AssetDatabase.LoadAssetAtPath<RunDefinition>(RunPath);
            Assert.IsTrue(run.TryValidate(out var problem), problem);
            Assert.AreEqual(new LotId(World.Midgard, LotKind.Training), run.Lots[0].Id);
            Assert.AreEqual(new LotId(World.Midgard, LotKind.Crazy), run.Lots[1].Id);
            Assert.AreEqual(new LotId(World.Vanaheim, LotKind.Training), run.Lots[2].Id);
            Assert.AreEqual(new LotId(World.Vanaheim, LotKind.Crazy), run.Lots[3].Id);
            Assert.AreEqual("Freyr's Harvest Barn", run.Lots[2].DisplayName);
            Assert.AreEqual("Storm-Soaked Harvest Feast", run.Lots[3].DisplayName);
        }

        [Test]
        public void ThePicker_ListsBothLots_OnDigitKeys3And4()
        {
            var picker = new TitlePicker(new FakePicker(LotChoices.From(AssetDatabase.LoadAssetAtPath<RunDefinition>(RunPath))));
            Assert.AreEqual(4, picker.Count);
            Assert.AreEqual("3 FREYR'S HARVEST BARN (TRAINING)", picker.Line(2));
            Assert.AreEqual("4 STORM-SOAKED HARVEST FEAST (CRAZY)", picker.Line(3));
        }

        [Test]
        public void BothScenes_AreInTheBuildSettings()
        {
            var paths = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes) if (scene.enabled) paths.Add(scene.path);
            CollectionAssert.Contains(paths, TrainingScene);
            CollectionAssert.Contains(paths, CrazyScene);
            Assert.AreEqual(System.IO.Path.GetFileNameWithoutExtension(TrainingScene), LotAt(TrainingPath).SceneName);
            Assert.AreEqual(System.IO.Path.GetFileNameWithoutExtension(CrazyScene), LotAt(CrazyPath).SceneName);
        }

        [TestCase(TrainingPath, 1)]
        [TestCase(CrazyPath, 3)]
        public void BothLots_AreValid_WithEightBays_NoEv_AndTheirWashBays(string path, int washBays)
        {
            var lot = LotAt(path);
            Assert.IsTrue(lot.TryValidate(out var problem), problem);
            var g = Geometry(lot);
            Assert.AreEqual(8, g.Spots.Length);
            Assert.AreEqual(0, g.ChargingSpots.Length, "no EV bay");
            Assert.IsFalse(g.HasWash, "no wash slot in the bay grid");
            Assert.AreEqual(washBays, g.WashBays.Length);
            Assert.AreEqual(washBays, g.WashStations.Length);
            Assert.AreEqual(washBays, lot.LayoutOptions.WashTroughs);
        }

        [Test]
        public void BothLots_KeepTodaysWidth()
        {
            var plain = new LotGeometry(Config(), 4, new LotLayoutOptions(true, 0f));
            Assert.AreEqual(0f, LotAt(TrainingPath).LayoutOptions.ExtensionWidth);
            Assert.AreEqual(0f, LotAt(CrazyPath).LayoutOptions.ExtensionWidth);
            Assert.AreEqual(plain.Bounds.xMax, Geometry(LotAt(CrazyPath)).Bounds.xMax, 0.001f);
        }

        [TestCase(TrainingPath)]
        [TestCase(CrazyPath)]
        public void TheWashBays_StandRightNextToTheBayBlock(string path)
        {
            var g = Geometry(LotAt(path));
            foreach (var washBay in g.WashBays)
            {
                var gap = washBay.xMin - g.Spots[3].xMax;
                Assert.That(gap, Is.InRange(0.01f, MaxWashBayGap), "gap to the bay block");
                Assert.Less(washBay.xMax, g.Strip.xMin);
            }
        }

        [Test]
        public void TheTrainingLot_HasNoMudPatches_TheCrazyLotHasFive_BetweenTwoAndAHalfAndFourUnits()
        {
            Assert.AreEqual(0, LotAt(TrainingPath).BuildWorldSpec().MudPatches.Count);
            Assert.AreEqual(0, LotAt(TrainingPath).Modifiers.Count);
            var patches = LotAt(CrazyPath).BuildWorldSpec().MudPatches;
            Assert.AreEqual(5, patches.Count);
            foreach (var patch in patches) Assert.That(patch.Size, Is.InRange(2.5f, 4f));
        }

        [Test]
        public void TheMudPatches_SitOnTheWayToTheWashBays_OffTheBays_AndInsideTheLot()
        {
            var lot = LotAt(CrazyPath);
            var g = Geometry(lot);
            foreach (var patch in lot.BuildWorldSpec().MudPatches)
            {
                var box = LotGeometry.Box(patch.Center, new Vector2(patch.Size, patch.Size));
                Assert.IsTrue(g.Bounds.Contains(box.min) && g.Bounds.Contains(box.max), patch.Center + " is inside the lot");
                Assert.Less(patch.Center.x + patch.Size * 0.5f, g.Strip.xMin, "west of the right strip");
                foreach (var spot in g.Spots) Assert.IsFalse(box.Overlaps(spot));
                foreach (var washBay in g.WashBays) Assert.IsFalse(box.Overlaps(washBay), "a puddle on a wash bay");
            }
        }

        private sealed class FakePicker : ILotPicker
        {
            public FakePicker(IReadOnlyList<LotChoice> choices) { Choices = choices; }
            public IReadOnlyList<LotChoice> Choices { get; }
            public bool TryChoose(int index) => true;
        }
    }
}
