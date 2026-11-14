using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Story;
using UncannyValet.EditorTools;
using UncannyValet.Run;
using UncannyValet.UI.Screens;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.Tests.Flow
{
    /// <summary>The god's verdict on the results screen (issue 85): tier selection, picking the line, and the shipped text.</summary>
    public sealed class VerdictTests
    {
        private const string TextPath = "Assets/_Project/Data/UI/StoryText.asset";
        private const string RunPath = "Assets/_Project/Data/Run/RunDefinition.asset";
        private static readonly LotId Training = new LotId(World.Midgard, LotKind.Training);
        private static readonly LotId Crazy = new LotId(World.Vanaheim, LotKind.Crazy);
        private static readonly LotId Finale = new LotId(World.Asgard, LotKind.Finale);

        private static ShiftResult Result(int blessings, int threshold) => new ShiftResult(blessings, threshold, blessings >= threshold, 0, 1);

        private static StoryTextAsset Text() => StoryTextAsset.Create("T", null, null, null, VerdictTextDrafts.Verdicts(), VerdictTextDrafts.Trainers());

        [TestCase(LotKind.Crazy, 29, 30, VerdictTier.Failed)]
        [TestCase(LotKind.Crazy, 30, 30, VerdictTier.Cleared)]
        [TestCase(LotKind.Crazy, 44, 30, VerdictTier.Cleared)]
        [TestCase(LotKind.Crazy, 45, 30, VerdictTier.ClearedWell)]
        [TestCase(LotKind.Crazy, 90, 30, VerdictTier.ClearedWell)]
        [TestCase(LotKind.Finale, 44, 30, VerdictTier.Cleared)]
        [TestCase(LotKind.Finale, 45, 30, VerdictTier.ClearedWell)]
        [TestCase(LotKind.Finale, 0, 30, VerdictTier.Failed)]
        public void Tier_FollowsTheThresholdAndTheClearedWellMultiple(LotKind kind, int blessings, int threshold, VerdictTier expected)
            => Assert.AreEqual(expected, VerdictRule.Tier(kind, blessings, threshold, 1.5f));

        [TestCase(29, VerdictTier.Failed)]
        [TestCase(30, VerdictTier.Cleared)]
        [TestCase(300, VerdictTier.Cleared)]
        public void Tier_ATrainingLot_IsNeverClearedWell(int blessings, VerdictTier expected)
            => Assert.AreEqual(expected, VerdictRule.Tier(LotKind.Training, blessings, 30, 1.5f));

        [Test]
        public void Tier_AMultipleUnderOne_CountsAsOne()
        {
            Assert.AreEqual(VerdictTier.ClearedWell, VerdictRule.Tier(LotKind.Crazy, 30, 30, 0.2f));
            Assert.AreEqual(VerdictTier.Failed, VerdictRule.Tier(LotKind.Crazy, 29, 30, 0.2f));
        }

        [Test]
        public void Pick_ACrazyLot_GivesTheLocalGodsNameAndTheTierLine()
        {
            var text = Text();
            Assert.IsTrue(text.TryGetVerdict(Crazy, out var story));
            var failed = VerdictPicker.Pick(text, Crazy, Result(10, 30));
            var cleared = VerdictPicker.Pick(text, Crazy, Result(30, 30));
            var well = VerdictPicker.Pick(text, Crazy, Result(45, 30));
            Assert.AreEqual("FREYJA", failed.Speaker);
            Assert.AreEqual(story.Line(VerdictTier.Failed), failed.Line);
            Assert.AreEqual(story.Line(VerdictTier.Cleared), cleared.Line);
            Assert.AreEqual(story.Line(VerdictTier.ClearedWell), well.Line);
            Assert.IsTrue(well.HasGod);
            Object.DestroyImmediate(text);
        }

        [Test]
        public void Pick_MidgardCrazy_IsMrsOkaforAndTheFinaleIsOdin()
        {
            var text = Text();
            Assert.AreEqual("MRS. OKAFOR", VerdictPicker.Pick(text, new LotId(World.Midgard, LotKind.Crazy), Result(30, 30)).Speaker);
            Assert.AreEqual("ODIN", VerdictPicker.Pick(text, Finale, Result(30, 30)).Speaker);
            Object.DestroyImmediate(text);
        }

        [Test]
        public void Pick_ATrainingLot_GivesATrainerLineAndNoGod()
        {
            var text = Text();
            var cleared = VerdictPicker.Pick(text, Training, Result(30, 30));
            var failed = VerdictPicker.Pick(text, Training, Result(3, 30));
            Assert.IsFalse(cleared.HasGod);
            Assert.AreEqual(string.Empty, cleared.Speaker);
            Assert.That(cleared.Line, Is.Not.Empty);
            Assert.AreNotEqual(cleared.Line, failed.Line);
            Assert.AreEqual(cleared.Line, VerdictPicker.Pick(text, Training, Result(300, 30)).Line);
            Object.DestroyImmediate(text);
        }

        [Test]
        public void Pick_WithoutAnEntryOrAText_GivesNothing()
        {
            var empty = StoryTextAsset.Create("E", null, null, null);
            Assert.AreEqual(string.Empty, VerdictPicker.Pick(empty, Crazy, Result(30, 30)).Line);
            Assert.AreEqual(string.Empty, VerdictPicker.Pick(empty, Training, Result(30, 30)).Line);
            Assert.IsFalse(VerdictPicker.Pick(null, Crazy, Result(30, 30)).HasGod);
            Object.DestroyImmediate(empty);
        }

        [Test]
        public void Build_WrapsTheVerdictLineToThePanel_AndNoGodMeansNoSpeaker()
        {
            var content = ResultsFormatter.Build(Result(30, 30), 3, 1, new VerdictSpeech("FREYJA", "THE HARVEST IS YOURS, VALET."));
            Assert.AreEqual("FREYJA", content.Speaker);
            Assert.AreEqual(new[] { "THE HARVEST IS YOURS, VALET." }, content.SpeechLines);
            var plain = ResultsFormatter.Build(Result(30, 30), 3);
            Assert.AreEqual(string.Empty, plain.Speaker);
            Assert.IsEmpty(plain.SpeechLines);
        }

        [Test]
        public void TheDrafts_CoverEveryGodAndEveryTrainingWorld_AndEveryLineFitsThePanel()
        {
            var lots = new List<LotId> { Finale };
            for (var world = World.Midgard; world <= World.Helheim; world++)
            {
                lots.Add(new LotId(world, LotKind.Crazy));
                lots.Add(new LotId(world, LotKind.Training));
            }
            var text = Text();
            Assert.IsNull(VerdictTextRules.Check(text, lots));
            foreach (var verdict in text.Verdicts)
                foreach (VerdictTier tier in System.Enum.GetValues(typeof(VerdictTier)))
                    AssertFits(verdict.Line(tier), verdict.Lot.ToString());
            foreach (var trainer in text.Trainers)
                foreach (VerdictTier tier in System.Enum.GetValues(typeof(VerdictTier)))
                    AssertFits(trainer.Line(tier), trainer.World.ToString());
            Object.DestroyImmediate(text);
        }

        private static void AssertFits(string line, string where)
        {
            var wrapped = ResultsFormatter.Wrap(line);
            Assert.LessOrEqual(wrapped.Length, ResultsPainter.MaxSpeechLines, where + ": " + line);
            foreach (var part in wrapped) Assert.LessOrEqual(part.Length, ResultsPainter.SpeechChars, where);
        }

        [Test]
        public void Check_ReportsAMissingGodOrTrainerAndABadLine()
        {
            var empty = StoryTextAsset.Create("E", null, null, null);
            Assert.That(VerdictTextRules.Check(empty, new[] { Crazy }), Does.Contain("no verdict lines"));
            Assert.That(VerdictTextRules.Check(empty, new[] { Training }), Does.Contain("no trainer lines"));
            var bad = StoryTextAsset.Create("B", null, null, null, new[] { new VerdictStory(Crazy, "FREYJA", "OK.", "OK.", "café") }, null);
            Assert.That(VerdictTextRules.Check(bad, new[] { Crazy }), Does.Contain("cleared-well verdict"));
            Object.DestroyImmediate(empty);
            Object.DestroyImmediate(bad);
        }

        [Test]
        public void AddMissingVerdicts_KeepsOwnerEditsAndAddsTheRest()
        {
            var edited = new VerdictStory(Crazy, "FREYJA", "MINE.", "MINE.", "MINE.");
            var text = StoryTextAsset.Create("T", null, null, null, new[] { edited }, null);
            Assert.IsTrue(text.AddMissingVerdicts(VerdictTextDrafts.Verdicts(), VerdictTextDrafts.Trainers()));
            Assert.IsTrue(text.TryGetVerdict(Crazy, out var kept));
            Assert.AreEqual("MINE.", kept.Line(VerdictTier.Failed));
            Assert.IsTrue(text.TryGetTrainer(World.Helheim, out _));
            Assert.IsFalse(text.AddMissingVerdicts(VerdictTextDrafts.Verdicts(), VerdictTextDrafts.Trainers()));
            Object.DestroyImmediate(text);
        }

        [Test]
        public void TheShippedText_HasVerdictLinesForEveryLotOfTheRun()
        {
            var text = AssetDatabase.LoadAssetAtPath<StoryTextAsset>(TextPath);
            var run = AssetDatabase.LoadAssetAtPath<RunDefinition>(RunPath);
            var lots = new List<LotId>();
            for (var i = 0; i < run.Lots.Count; i++) lots.Add(run.Lots[i].Id);
            Assert.IsNull(VerdictTextRules.Check(text, lots), "run Uncanny Valet/Setup/Build Everything");
        }
    }
}
