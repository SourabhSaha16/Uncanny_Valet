using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Story;
using UncannyValet.UI.Flow;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.Tests.Flow
{
    /// <summary>The cards built for each flow state from sample lots (issue 84): their content, their fit on the 480x270 panel and the world's accents.</summary>
    public sealed class StoryCardTests
    {
        private static readonly LotId MT = new LotId(World.Midgard, LotKind.Training);
        private static readonly LotId MC = new LotId(World.Midgard, LotKind.Crazy);
        private static readonly LotId VC = new LotId(World.Vanaheim, LotKind.Crazy);
        private static readonly Color Tan = new Color(0.9f, 0.7f, 0.4f);
        private static readonly Color Green = new Color(0.4f, 0.7f, 0.4f);
        private static readonly Color VGold = new Color(1f, 0.8f, 0.2f);
        private const int PromptTop = 234;

        private sealed class Facts : IStoryFacts
        {
            public bool TryGetLot(LotId id, out StoryLotFacts facts)
            {
                facts = default;
                if (id == MT) facts = new StoryLotFacts(id, "Grandma's Diner", "Grandma", 150);
                else if (id == MC) facts = new StoryLotFacts(id, "Fair Night at Grandma's", "Mrs. Okafor", 300);
                else if (id == VC) facts = new StoryLotFacts(id, "Storm-Soaked Harvest Feast", "Freyja", 300);
                else return false;
                return true;
            }

            public bool TryGetPower(StoryPower power, out StoryPowerFacts facts)
            {
                facts = new StoryPowerFacts(power, power == StoryPower.StormWind ? "Storm wind" : "Thunder stride", "Fallback line");
                return power != StoryPower.None;
            }
        }

        private sealed class Progress : IRunProgress
        {
            public readonly List<StoryPower> Earned = new List<StoryPower>();
            public readonly Dictionary<StoryPower, int> Levels = new Dictionary<StoryPower, int>();
            public bool HasCurrentLot => true;
            public LotId CurrentLot => MT;
            public IReadOnlyList<StoryPower> EarnedPowers => Earned;
            public int PowerLevel(StoryPower power) => Levels.TryGetValue(power, out var l) ? l : 0;
            public int FlashedLevel(StoryPower power) => 0;
            public bool IsCleared(LotId lot) => false;
            public bool IsWorldCleared(World world) => false;
            public int BestBlessings(LotId lot) => 321;
        }

        private StoryTextAsset _text;
        private Facts _facts;
        private Progress _progress;
        private FlowMachine _m;

        [SetUp]
        public void SetUp()
        {
            _text = StoryTextAsset.Create("Test",
                new[] { new WorldStory(World.Midgard, "MIDGARD", Tan, Green), new WorldStory(World.Vanaheim, "VANAHEIM", VGold, Green) },
                new[]
                {
                    new LotStory(MT, new List<string> { "SUNNY DAY", "PARK THE CARS" }),
                    new LotStory(MC, new List<string> { "FAIR NIGHT" }),
                    new LotStory(VC, new List<string> { "FEAST" }),
                },
                new[]
                {
                    new PowerStory(StoryPower.ThunderStride, "DASH IN A FLASH.", "WELL PARKED, DEAR."),
                    new PowerStory(StoryPower.StormWind, string.Empty, "TAKE THE STORM'S WIND."),
                });
            _facts = new Facts();
            _progress = new Progress();
            _m = new FlowMachine(() => MT);
            _m.SceneLoaded(MT);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_text);

        private static string AllText(StoryCard card)
        {
            var all = string.Empty;
            for (var i = 0; i < card.Lines.Count; i++) all += card.Lines[i].Text + "\n";
            return all;
        }

        private StoryCard Build() => StoryCardBuilder.Build(_m, _text, _facts, _progress);

        private void ToState(FlowState target, LotId from, RunOutcome outcome, LotId next)
        {
            _m.SceneLoaded(from);
            _m.SkipToPlaying();
            _m.ShiftEnded();
            _m.NextLotChosen(next, outcome);
            if (outcome == RunOutcome.RunComplete) _m.RunCompleted(RunEnd.EndOfContent);
            while (_m.State != target) _m.Advance();
        }

        private static void AssertFits(StoryCard card)
        {
            for (var i = 0; i < card.Lines.Count; i++)
            {
                var line = card.Lines[i];
                Assert.GreaterOrEqual(line.X, 10, line.Text);
                Assert.LessOrEqual(line.X + PixelTextLayout.Width(line.Text.Length, line.Scale), PixelGrid.Width - 10, line.Text);
                Assert.LessOrEqual(line.Y + PixelTextLayout.Height(line.Scale), PromptTop, "overlaps the prompt: " + line.Text);
            }
        }

        [Test]
        public void TrainingIntro_ShowsWorldTagLotTrainerFlavourAndGoal()
        {
            _m.Advance(); _m.Advance();
            var card = Build();
            var text = AllText(card);
            StringAssert.Contains("MIDGARD", text);
            StringAssert.Contains("TRAINING", text);
            StringAssert.Contains("GRANDMA'S DINER", text);
            StringAssert.Contains("TRAINER: GRANDMA", text);
            StringAssert.Contains("PARK THE CARS", text);
            StringAssert.Contains("EARN 150 BLESSINGS TO CLEAR THE LOT", text);
            Assert.AreEqual(Tan, card.Accent);
            Assert.AreEqual(Green, card.AccentSecond);
            Assert.AreEqual("PRESS ANY KEY", card.Prompt);
            AssertFits(card);
        }

        [Test]
        public void CrazyIntro_FramesTheLocalGodsTest_WithTheLotsThreshold()
        {
            _m.DevStart(MC);
            var text = AllText(Build());
            StringAssert.Contains("CRAZY", text);
            StringAssert.Contains("THE TEST OF MRS. OKAFOR", text);
            StringAssert.Contains("EARN 300 BLESSINGS", text);
        }

        [Test]
        public void LongLotNames_StillFit_AndVanaheimUsesItsOwnAccent()
        {
            _m.DevStart(VC);
            var card = Build();
            StringAssert.Contains("STORM-SOAKED HARVEST FEAST", AllText(card));
            Assert.AreEqual(VGold, card.Accent);
            AssertFits(card);
        }

        [Test]
        public void Reward_ShowsNameIconDescriptionKeyAndTheGodsFarewell()
        {
            _progress.Earned.Add(StoryPower.ThunderStride);
            _progress.Levels[StoryPower.ThunderStride] = 1;
            _m.SkipToPlaying();
            _m.ShiftEnded();
            _m.PowerGranted(StoryPower.ThunderStride, 1);
            _m.NextLotChosen(MC, RunOutcome.Advanced);
            _m.Advance();
            var card = Build();
            var text = AllText(card);
            StringAssert.Contains("NEW POWER!", text);
            StringAssert.Contains("THUNDER STRIDE", text);
            StringAssert.Contains("DASH IN A FLASH.", text);
            StringAssert.Contains("PRESS 1 TO USE IT", text);
            StringAssert.Contains("WELL PARKED, DEAR.", text);
            StringAssert.Contains("- GRANDMA", text, "the farewell is attributed to the watcher of the lot that granted it");
            Assert.IsTrue(card.HasIcon);
            Assert.AreEqual(StoryPower.ThunderStride, card.IconPower);
            AssertFits(card);
        }

        [Test]
        public void Reward_KeyNumberFollowsTheEarnOrder_AndAnEmptyDescriptionFallsBackToTheFacts()
        {
            _progress.Earned.Add(StoryPower.ThunderStride);
            _progress.Earned.Add(StoryPower.StormWind);
            _m.SkipToPlaying();
            _m.ShiftEnded();
            _m.PowerGranted(StoryPower.StormWind, 1);
            _m.NextLotChosen(MC, RunOutcome.Advanced);
            _m.Advance();
            var text = AllText(Build());
            StringAssert.Contains("PRESS 2 TO USE IT", text);
            StringAssert.Contains("FALLBACK LINE", text.ToUpperInvariant());
        }

        [Test]
        public void UpgradeReward_SaysUpgraded()
        {
            _progress.Earned.Add(StoryPower.ThunderStride);
            _m.SkipToPlaying();
            _m.ShiftEnded();
            _m.PowerGranted(StoryPower.ThunderStride, 2);
            _m.NextLotChosen(MC, RunOutcome.Advanced);
            _m.Advance();
            StringAssert.Contains("POWER UPGRADED!", AllText(Build()));
        }

        [Test]
        public void SentBack_ListsThePowersKept_OrNone()
        {
            ToState(FlowState.SentBack, VC, RunOutcome.SentBack, MC);
            var none = AllText(Build());
            StringAssert.Contains("THROWN BACK!", none);
            StringAssert.Contains("POWERS KEPT:", none);
            StringAssert.Contains("NONE YET", none);
            _progress.Earned.Add(StoryPower.ThunderStride);
            _progress.Earned.Add(StoryPower.StormWind);
            _progress.Levels[StoryPower.StormWind] = 2;
            var card = Build();
            var text = AllText(card);
            StringAssert.Contains("THUNDER STRIDE", text);
            StringAssert.Contains("STORM WIND +", text);
            Assert.AreEqual(VGold, card.Accent, "the failed lot's world");
            AssertFits(card);
        }

        [Test]
        public void EndOfContent_SaysToBeContinued_WithThePowersEarned()
        {
            _progress.Earned.Add(StoryPower.ThunderStride);
            ToState(FlowState.EndCard, VC, RunOutcome.RunComplete, default);
            var card = Build();
            var text = AllText(card);
            StringAssert.Contains("TO BE CONTINUED", text);
            StringAssert.Contains("POWERS EARNED:", text);
            StringAssert.Contains("THUNDER STRIDE", text);
            StringAssert.DoesNotContain("THOR IS HOME", text);
            Assert.AreEqual("PRESS ANY KEY", card.Prompt);
            AssertFits(card);
        }

        [Test]
        public void FinaleEnd_SaysThorIsHome_WithTheFinalesBlessings()
        {
            _m.SceneLoaded(MC);
            _m.SkipToPlaying();
            _m.ShiftEnded();
            _m.NextLotChosen(default, RunOutcome.RunComplete);
            _m.RunCompleted(RunEnd.Finale);
            _m.Advance();
            var text = AllText(Build());
            StringAssert.Contains("THOR IS HOME", text);
            StringAssert.Contains("FINALE BLESSINGS: 321", text);
        }

        [Test]
        public void ACardWithoutAWorldEntry_StillBuilds_AndReportsTheGap()
        {
            var empty = StoryTextAsset.Create("Empty", null, null, null);
            _m.Advance(); _m.Advance();
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("no entry for world"));
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("no intro lines"));
            var card = StoryCardBuilder.Build(_m, empty, _facts, _progress);
            Assert.Greater(card.Lines.Count, 0);
            Object.DestroyImmediate(empty);
        }
    }
}
