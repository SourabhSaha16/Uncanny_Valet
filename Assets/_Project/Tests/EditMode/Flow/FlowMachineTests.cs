using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Flow;

namespace UncannyValet.Tests.Flow
{
    /// <summary>Every path of the story flow state machine (issue 84): first start, rewards, failures, the end cards, retry and the scene loads.</summary>
    public sealed class FlowMachineTests
    {
        private static readonly LotId MT = new LotId(World.Midgard, LotKind.Training);
        private static readonly LotId MC = new LotId(World.Midgard, LotKind.Crazy);
        private static readonly LotId VT = new LotId(World.Vanaheim, LotKind.Training);
        private static readonly LotId VC = new LotId(World.Vanaheim, LotKind.Crazy);
        private static readonly LotId Asgard = new LotId(World.Asgard, LotKind.Finale);

        private FlowMachine _m;

        [SetUp]
        public void SetUp()
        {
            _m = new FlowMachine(() => MT);
            _m.SceneLoaded(MT);
        }

        // Plays through the first press chain up to a running shift of the loaded lot.
        private void StartShift(LotId lot)
        {
            Assert.AreEqual(FlowState.LotIntro, _m.State);
            Assert.AreEqual(lot, _m.IntroLot);
            Assert.AreEqual(FlowStep.StartShift, _m.Advance());
            _m.ShiftStarted();
            Assert.AreEqual(FlowState.Playing, _m.State);
        }

        private void EndShift(LotId lot, RunOutcome outcome, LotId next)
        {
            _m.ShiftEnded();
            Assert.AreEqual(FlowState.Results, _m.State);
            _m.NextLotChosen(next, outcome);
        }

        private void LoadAndStart(LotId lot)
        {
            _m.SceneLoaded(lot);
            StartShift(lot);
        }

        [Test]
        public void FirstStart_ShowsHowToPlayThenTheFirstLotsIntroThenTheShift()
        {
            Assert.AreEqual(FlowState.Title, _m.State);
            Assert.AreEqual(FlowStep.None, _m.Advance());
            Assert.AreEqual(FlowState.HowToPlay, _m.State);
            Assert.AreEqual(FlowStep.None, _m.Advance(), "the loaded scene is already the first lot: no load");
            StartShift(MT);
        }

        [Test]
        public void SecondTitle_SkipsHowToPlay()
        {
            _m.Advance();
            _m.Advance();
            StartShift(MT);
            _m.ShiftEnded();
            _m.RunCompleted(RunEnd.EndOfContent);
            _m.NextLotChosen(default, RunOutcome.RunComplete);
            _m.Advance();
            Assert.AreEqual(FlowState.EndCard, _m.State);
            Assert.AreEqual(FlowStep.StartNewRun, _m.Advance());
            Assert.AreEqual(FlowState.Title, _m.State);
            Assert.AreEqual(FlowStep.LoadLot, _m.Advance(), "the dirty scene is replaced by a fresh first lot");
            Assert.AreEqual(FlowState.LotIntro, _m.State, "no how-to-play the second time");
            Assert.AreEqual(MT, _m.IntroLot);
        }

        [Test]
        public void ClearWithoutReward_GoesToTheNextLotsIntro()
        {
            _m.Advance(); _m.Advance(); StartShift(MT);
            EndShift(MT, RunOutcome.Advanced, MC);
            Assert.AreEqual(FlowStep.LoadLot, _m.Advance());
            Assert.AreEqual(FlowState.LotIntro, _m.State);
            Assert.AreEqual(MC, _m.IntroLot);
            Assert.AreEqual(FlowStep.None, _m.Advance(), "presses wait for the scene to load");
            LoadAndStart(MC);
        }

        [Test]
        public void ClearWithReward_ShowsTheRewardCardBetweenResultsAndTheNextIntro()
        {
            _m.Advance(); _m.Advance(); StartShift(MT);
            _m.ShiftEnded();
            _m.PowerGranted(StoryPower.ThunderStride, 1);
            _m.NextLotChosen(VT, RunOutcome.Advanced);
            Assert.AreEqual(FlowStep.None, _m.Advance());
            Assert.AreEqual(FlowState.Reward, _m.State);
            Assert.AreEqual(StoryPower.ThunderStride, _m.Reward.Power);
            Assert.AreEqual(FlowStep.LoadLot, _m.Advance());
            Assert.AreEqual(FlowState.LotIntro, _m.State);
            Assert.AreEqual(VT, _m.IntroLot);
            Assert.AreEqual(0, _m.RewardCount);
        }

        [Test]
        public void TwoRewards_ShowOneCardEach()
        {
            _m.Advance(); _m.Advance(); StartShift(MT);
            _m.ShiftEnded();
            _m.PowerGranted(StoryPower.ThunderStride, 2);
            _m.PowerGranted(StoryPower.StormWind, 1);
            _m.NextLotChosen(VT, RunOutcome.Advanced);
            _m.Advance();
            Assert.AreEqual(StoryPower.ThunderStride, _m.Reward.Power);
            Assert.AreEqual(2, _m.Reward.Level);
            _m.Advance();
            Assert.AreEqual(FlowState.Reward, _m.State);
            Assert.AreEqual(StoryPower.StormWind, _m.Reward.Power);
            _m.Advance();
            Assert.AreEqual(FlowState.LotIntro, _m.State);
        }

        [Test]
        public void FailTraining_RestartsTheSameLotThroughItsIntro()
        {
            _m.Advance(); _m.Advance(); StartShift(MT);
            EndShift(MT, RunOutcome.RestartedTraining, MT);
            Assert.AreEqual(FlowStep.LoadLot, _m.Advance(), "a fresh scene, even of the same lot");
            Assert.AreEqual(FlowState.LotIntro, _m.State);
            Assert.AreEqual(MT, _m.IntroLot);
        }

        [Test]
        public void FailCrazy_ShowsTheSentBackCardThenMidgardCrazyIntro()
        {
            _m.Advance(); _m.Advance(); StartShift(MT);
            EndShift(MT, RunOutcome.Advanced, VC);
            _m.Advance();
            LoadAndStart(VC);
            EndShift(VC, RunOutcome.SentBack, MC);
            Assert.AreEqual(FlowStep.None, _m.Advance());
            Assert.AreEqual(FlowState.SentBack, _m.State);
            Assert.AreEqual(FlowStep.LoadLot, _m.Advance());
            Assert.AreEqual(FlowState.LotIntro, _m.State);
            Assert.AreEqual(MC, _m.IntroLot);
        }

        [Test]
        public void FailFinale_IsSentBackToo()
        {
            _m.Advance(); _m.Advance(); StartShift(MT);
            EndShift(MT, RunOutcome.Advanced, Asgard);
            _m.Advance();
            LoadAndStart(Asgard);
            EndShift(Asgard, RunOutcome.SentBack, MC);
            _m.Advance();
            Assert.AreEqual(FlowState.SentBack, _m.State);
        }

        [Test]
        public void SentBack_ShowsAfterTheRewardWhenThereIsOne()
        {
            _m.Advance(); _m.Advance(); StartShift(MT);
            _m.ShiftEnded();
            _m.PowerGranted(StoryPower.StormWind, 1);
            _m.NextLotChosen(MC, RunOutcome.SentBack);
            _m.Advance();
            Assert.AreEqual(FlowState.Reward, _m.State);
            _m.Advance();
            Assert.AreEqual(FlowState.SentBack, _m.State);
        }

        [Test]
        public void RunCompletedWithEndOfContent_ShowsTheEndCardThenStartsAFreshRun()
        {
            _m.Advance(); _m.Advance(); StartShift(MT);
            _m.ShiftEnded();
            _m.NextLotChosen(default, RunOutcome.RunComplete);
            _m.RunCompleted(RunEnd.EndOfContent);
            _m.Advance();
            Assert.AreEqual(FlowState.EndCard, _m.State);
            Assert.AreEqual(RunEnd.EndOfContent, _m.End);
            Assert.AreEqual(FlowStep.StartNewRun, _m.Advance());
            Assert.AreEqual(FlowState.Title, _m.State);
            Assert.IsFalse(_m.IsRunComplete);
        }

        [Test]
        public void ClearFinale_ShowsTheFinaleEndCardAfterTheReward()
        {
            _m.Advance(); _m.Advance(); StartShift(MT);
            _m.ShiftEnded();
            _m.PowerGranted(StoryPower.MjolnirRecall, 1);
            _m.NextLotChosen(default, RunOutcome.RunComplete);
            _m.RunCompleted(RunEnd.Finale);
            _m.Advance();
            Assert.AreEqual(FlowState.Reward, _m.State);
            _m.Advance();
            Assert.AreEqual(FlowState.EndCard, _m.State);
            Assert.AreEqual(RunEnd.Finale, _m.End);
        }

        [Test]
        public void SkippedTrainingLots_AreSilent_TheNextLotIsWhateverTheRunChose()
        {
            _m.Advance(); _m.Advance(); StartShift(MT);
            EndShift(MT, RunOutcome.Advanced, VC);
            _m.Advance();
            Assert.AreEqual(FlowState.LotIntro, _m.State);
            Assert.AreEqual(VC, _m.IntroLot);
        }

        [Test]
        public void Retry_GoesStraightBackToTheShiftWithNoCards()
        {
            _m.Advance(); _m.Advance(); StartShift(MT);
            Assert.IsFalse(_m.Retry(), "only the results screen retries");
            EndShift(MT, RunOutcome.RestartedTraining, MT);
            Assert.IsTrue(_m.Retry());
            Assert.AreEqual(FlowState.Playing, _m.State);
            Assert.IsFalse(_m.HasNextLot, "the old decision is forgotten");
        }

        [Test]
        public void PowerGrantedOutsideAShift_GetsNoRewardCard()
        {
            _m.PowerGranted(StoryPower.ThunderStride, 1);
            Assert.AreEqual(0, _m.RewardCount);
        }

        [Test]
        public void Advance_DoesNothingWhileAShiftRuns()
        {
            _m.Advance(); _m.Advance(); StartShift(MT);
            Assert.AreEqual(FlowStep.None, _m.Advance());
            Assert.AreEqual(FlowState.Playing, _m.State);
        }

        [Test]
        public void DevStart_ShowsTheChosenLotsIntroInAFreshScene()
        {
            Assert.AreEqual(FlowStep.LoadLot, _m.DevStart(VC));
            Assert.AreEqual(FlowState.LotIntro, _m.State);
            Assert.AreEqual(VC, _m.IntroLot);
            _m.SceneLoaded(VC);
            Assert.AreEqual(FlowState.LotIntro, _m.State);
            Assert.AreEqual(FlowStep.StartShift, _m.Advance());
        }

        [Test]
        public void DevStart_OfTheLoadedFreshLot_ShowsTheIntroWithoutLoading()
        {
            Assert.AreEqual(FlowStep.None, _m.DevStart(MT));
            Assert.AreEqual(FlowState.LotIntro, _m.State);
        }

        [Test]
        public void AScene_LoadedOutsideTheFlow_GetsItsIntroCard()
        {
            _m.Advance(); _m.Advance(); StartShift(MT);
            _m.SceneLoaded(VT);
            Assert.AreEqual(FlowState.LotIntro, _m.State);
            Assert.AreEqual(VT, _m.IntroLot);
        }

        [Test]
        public void SkipToPlaying_NeedsNoCards()
        {
            _m.SkipToPlaying();
            Assert.AreEqual(FlowState.Playing, _m.State);
        }

        [Test]
        public void StartLot_IsAskedWhenTheTitleIsDismissed()
        {
            var asked = 0;
            var m = new FlowMachine(() => { asked++; return VT; });
            m.SceneLoaded(VT);
            m.Advance();
            Assert.AreEqual(0, asked);
            m.Advance();
            Assert.AreEqual(1, asked);
            Assert.AreEqual(VT, m.IntroLot);
        }

        [Test]
        public void Version_ChangesWithEveryScreen()
        {
            var v = _m.Version;
            _m.Advance();
            Assert.AreNotEqual(v, _m.Version);
        }
    }
}
