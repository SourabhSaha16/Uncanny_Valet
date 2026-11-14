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
    /// <summary>The shipped Vanaheim lots (task 107): where they sit in the run, the bays, the troughs, the widened lot and the mud patches.</summary>
    public sealed class VanaheimLayoutTests
    {
        private const string TrainingScene = "Assets/_Project/Scenes/Lot_VanaheimTraining.unity";
        private const string CrazyScene = "Assets/_Project/Scenes/Lot_VanaheimCrazy.unity";
        private const float BayToTrough = 14f;

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
        public void BothLots_AreValid_WithEightBays_NoEv_AndTheirTroughs(string path, int troughs)
        {
            var lot = LotAt(path);
            Assert.IsTrue(lot.TryValidate(out var problem), problem);
            var g = Geometry(lot);
            Assert.AreEqual(8, g.Spots.Length);
            Assert.AreEqual(0, g.ChargingSpots.Length, "no EV bay");
            Assert.IsFalse(g.HasWash, "no wash slot in the bay grid");
            Assert.AreEqual(troughs, g.Troughs.Length);
            Assert.AreEqual(troughs, g.WashStations.Length);
            Assert.AreEqual(troughs, lot.LayoutOptions.WashTroughs);
        }

        [Test]
        public void TheTrainingLot_IsTodaysWidth_TheCrazyLotIsWiderOnTheFarSide()
        {
            var plain = new LotGeometry(Config(), 4, new LotLayoutOptions(true, 0f));
            Assert.AreEqual(0f, LotAt(TrainingPath).LayoutOptions.ExtensionWidth);
            var crazy = Geometry(LotAt(CrazyPath));
            Assert.AreEqual(LotAt(CrazyPath).LayoutOptions.ExtensionWidth, crazy.Bounds.xMax - plain.Bounds.xMax, 0.001f);
            Assert.That(LotAt(CrazyPath).LayoutOptions.ExtensionWidth, Is.InRange(11f, 15f), "about twelve units wider");
        }

        [Test]
        public void TheCrazyTroughs_AreAtLeastFourteenUnitsFromEveryBay_AndTheTrainingTroughIsBesideTheBlock()
        {
            var crazy = Geometry(LotAt(CrazyPath));
            foreach (var trough in crazy.Troughs)
                foreach (var bay in crazy.Spots)
                    Assert.GreaterOrEqual(trough.xMin - bay.xMax, BayToTrough - 0.001f);
            var training = Geometry(LotAt(TrainingPath));
            Assert.Less(training.Troughs[0].xMin - training.Spots[3].xMax, BayToTrough, "the compact lot's trough is close to the bays");
            Assert.Greater(training.Troughs[0].xMin, training.Spots[3].xMax);
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
        public void TheMudPatches_SitBetweenTheBaysAndTheTroughs_OffTheBays_AndInsideTheLot()
        {
            var lot = LotAt(CrazyPath);
            var g = Geometry(lot);
            foreach (var patch in lot.BuildWorldSpec().MudPatches)
            {
                var half = patch.Size * 0.5f;
                var box = LotGeometry.Box(patch.Center, new Vector2(patch.Size, patch.Size));
                Assert.IsTrue(g.Bounds.Contains(box.min) && g.Bounds.Contains(box.max), patch.Center + " is inside the lot");
                Assert.Greater(patch.Center.x - half, g.Spots[3].xMax, "east of the bays");
                Assert.Less(patch.Center.x, g.Troughs[0].xMin, "west of the troughs");
                foreach (var spot in g.Spots) Assert.IsFalse(box.Overlaps(spot));
                foreach (var trough in g.Troughs) Assert.IsFalse(box.Overlaps(trough), "a puddle on a trough");
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
