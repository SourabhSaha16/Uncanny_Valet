using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Story;
using UncannyValet.Powers;
using UncannyValet.Run;
using UncannyValet.UI.Flow;
using UnityEditor;
using System.Collections.Generic;

namespace UncannyValet.Tests.Flow
{
    /// <summary>Every card the shipped story text, run and power definitions can produce fits the panel (issue 84).</summary>
    public sealed class ShippedCardsTests
    {
        private const string TextPath = "Assets/_Project/Data/UI/StoryText.asset";
        private const string RunPath = "Assets/_Project/Data/Run/RunDefinition.asset";
        private static readonly StoryPower[] Powers = { StoryPower.ThunderStride, StoryPower.StormWind, StoryPower.FreezeTime, StoryPower.MjolnirRecall };

        private StoryTextAsset _text;
        private RunDefinition _run;
        private RunStoryFacts _facts;
        private RunProgress _progress;
        private FlowMachine _m;

        [SetUp]
        public void SetUp()
        {
            _text = AssetDatabase.LoadAssetAtPath<StoryTextAsset>(TextPath);
            _run = AssetDatabase.LoadAssetAtPath<RunDefinition>(RunPath);
            var definitions = new List<PowerDefinition>();
            foreach (var guid in AssetDatabase.FindAssets("t:PowerDefinition"))
                definitions.Add(AssetDatabase.LoadAssetAtPath<PowerDefinition>(AssetDatabase.GUIDToAssetPath(guid)));
            _facts = new RunStoryFacts(_run, definitions);
            _progress = new RunProgress(_run);
            _m = new FlowMachine(() => _run.Lots[0].Id);
        }

        private StoryCard Build() => StoryCardBuilder.Build(_m, _text, _facts, _progress);

        [Test]
        public void EveryLotsIntroCard_Fits()
        {
            for (var i = 0; i < _run.Lots.Count; i++)
            {
                _m.SceneLoaded(_run.Lots[i].Id);
                _m.DevStart(_run.Lots[i].Id);
                var card = Build();
                Assert.Greater(card.Lines.Count, 3, _run.Lots[i].Id.ToString());
                StoryCardTests.AssertFits(card);
            }
        }

        [Test]
        public void EveryPowersRewardCard_Fits_AtBothLevels()
        {
            foreach (var power in Powers)
                for (var level = 1; level <= 2; level++)
                {
                    _progress.GrantPower(power, level);
                    _m.SceneLoaded(_run.Lots[1].Id);
                    _m.SkipToPlaying();
                    _m.ShiftEnded();
                    _m.PowerGranted(power, level);
                    _m.NextLotChosen(_run.Lots[2].Id, RunOutcome.Advanced);
                    _m.Advance();
                    Assert.AreEqual(FlowState.Reward, _m.State);
                    StoryCardTests.AssertFits(Build());
                    _m.Advance();
                    _m.SceneLoaded(_run.Lots[1].Id);
                }
        }

        [Test]
        public void TheSentBackAndEndCards_Fit_WithAllFourPowersEarnedAndUpgraded()
        {
            foreach (var power in Powers) _progress.GrantPower(power, 2);
            _m.SceneLoaded(_run.Lots[1].Id);
            _m.SkipToPlaying();
            _m.ShiftEnded();
            _m.NextLotChosen(_run.Lots[1].Id, RunOutcome.SentBack);
            _m.Advance();
            Assert.AreEqual(FlowState.SentBack, _m.State);
            StoryCardTests.AssertFits(Build());
            foreach (var end in new[] { RunEnd.EndOfContent, RunEnd.Finale })
            {
                var m = new FlowMachine(() => _run.Lots[0].Id);
                m.SceneLoaded(_run.Lots[1].Id);
                m.SkipToPlaying();
                m.ShiftEnded();
                m.NextLotChosen(default, RunOutcome.RunComplete);
                m.RunCompleted(end);
                m.Advance();
                StoryCardTests.AssertFits(StoryCardBuilder.Build(m, _text, _facts, _progress));
            }
        }
    }
}
