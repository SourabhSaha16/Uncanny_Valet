using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Story;
using UncannyValet.Run;
using UncannyValet.UI.Kit;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.Tests.Flow
{
    /// <summary>The story text asset covers every lot and power of the run, uses only the pixel font's characters and keeps its lines short (issue 84).</summary>
    public sealed class StoryTextTests
    {
        private const string TextPath = "Assets/_Project/Data/UI/StoryText.asset";
        private const string RunPath = "Assets/_Project/Data/Run/RunDefinition.asset";
        private static readonly LotId MT = new LotId(World.Midgard, LotKind.Training);

        private static List<LotId> RunLots()
        {
            var run = AssetDatabase.LoadAssetAtPath<RunDefinition>(RunPath);
            var ids = new List<LotId>();
            for (var i = 0; i < run.Lots.Count; i++) ids.Add(run.Lots[i].Id);
            return ids;
        }

        [Test]
        public void TheShippedText_CoversEveryLotOfTheRun_AndEveryPower()
        {
            var text = AssetDatabase.LoadAssetAtPath<StoryTextAsset>(TextPath);
            Assert.IsNotNull(text, "run Uncanny Valet/Setup/Build Everything");
            Assert.IsNull(StoryTextRules.Check(text, RunLots(), UncannyValet.Core.Contracts.PowerLevelRows.All));
        }

        [Test]
        public void TheShippedRun_ListsTheFourStoryLots()
        {
            Assert.AreEqual(4, RunLots().Count);
        }

        [Test]
        public void Check_ReportsAMissingLotWorldOrPower()
        {
            var text = StoryTextAsset.Create("T", new[] { new WorldStory(World.Midgard, "MIDGARD", Color.white, Color.white) }, null, null);
            Assert.That(StoryTextRules.Check(text, new[] { MT }, new StoryPower[0]), Does.Contain("no intro lines"));
            Assert.That(StoryTextRules.Check(text, new[] { new LotId(World.Vanaheim, LotKind.Training) }, new StoryPower[0]), Does.Contain("no world name"));
            Assert.That(StoryTextRules.Check(text, new LotId[0], new[] { StoryPower.StormWind }), Does.Contain("no reward lines"));
            Object.DestroyImmediate(text);
        }

        [Test]
        public void Check_RejectsALongLine_AndACharacterTheFontLacks()
        {
            var longLine = new string('A', StoryTextAsset.MaxLineLength + 1);
            var world = new[] { new WorldStory(World.Midgard, "MIDGARD", Color.white, Color.white) };
            var tooLong = StoryTextAsset.Create("T", world, new[] { new LotStory(MT, new List<string> { longLine }) }, null);
            Assert.That(StoryTextRules.Check(tooLong, new[] { MT }, new StoryPower[0]), Does.Contain("over"));
            var odd = StoryTextAsset.Create("T", world, new[] { new LotStory(MT, new List<string> { "NO @ HERE" }) }, null);
            Assert.That(StoryTextRules.Check(odd, new[] { MT }, new StoryPower[0]), Does.Contain("no glyph"));
            Object.DestroyImmediate(tooLong);
            Object.DestroyImmediate(odd);
        }

        [Test]
        public void TheDefaultGenericText_PassesTheCheck()
        {
            var text = StoryTextAsset.Create("T", null, null, null);
            Assert.IsNull(StoryTextRules.Check(text, new LotId[0], new StoryPower[0]));
            Object.DestroyImmediate(text);
        }

        [Test]
        public void TheDraftTexts_UseOnlyGlyphsTheFontHas()
        {
            Assert.AreEqual(PixelGlyphs.Chars, UncannyValet.Core.Watchers.GagLineAsset.AllowedCharacters);
        }

        [Test]
        public void StartLot_IsTheFirstLotNotYetCleared()
        {
            var run = AssetDatabase.LoadAssetAtPath<RunDefinition>(RunPath);
            var progress = new RunProgress(run);
            Assert.AreEqual(MT, StartLotRule.First(run, progress), "a fresh page load starts at Midgard training");
            progress.MarkCleared(MT);
            Assert.AreEqual(new LotId(World.Midgard, LotKind.Crazy), StartLotRule.First(run, progress));
            foreach (var lot in RunLots()) progress.MarkCleared(lot);
            Assert.AreEqual(MT, StartLotRule.First(run, progress), "everything cleared: the first lot again");
            progress.StartNewRun();
            Assert.AreEqual(MT, StartLotRule.First(run, progress));
        }
    }
}
