using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Run;
using UnityEditor;

namespace UncannyValet.Tests.Run
{
    /// <summary>The roguelite run rules over the real 4-lot definition (the demo end of content).</summary>
    public sealed class RunFlowTests : RunFlowTestBase
    {
        [Test]
        public void Real_ClearingInOrderAdvances_ThenTheLastLotEndsTheRunWithEndOfContent()
        {
            StartReal();
            AssertPlay(Clear(MT), MC, RunOutcome.Advanced);
            AssertPlay(Clear(MC), VT, RunOutcome.Advanced);
            AssertPlay(Clear(VT), VC, RunOutcome.Advanced);
            var end = Clear(VC);
            Assert.IsFalse(end.HasNext);
            Assert.AreEqual(RunOutcome.RunComplete, end.Reason);
            Assert.AreEqual(1, _completed.Count);
            Assert.AreEqual(RunEnd.EndOfContent, _completed[0].Reason);
            Assert.IsTrue(_progress.IsRunComplete);
            Assert.AreEqual(RunEnd.EndOfContent, _progress.EndReason);
            Assert.IsFalse(_progress.HasNextLot);
        }
        [Test]
        public void Real_FlowStateMirrorsTheLastDecision()
        {
            StartReal();
            Clear(MT);
            Assert.IsTrue(_progress.HasNextLot);
            Assert.AreEqual(MC, _progress.NextLot);
            Assert.AreEqual(RunOutcome.Advanced, _progress.LastOutcome);
            Assert.IsFalse(_progress.IsRunComplete);
            Assert.IsTrue(_progress.IsCleared(MT));
        }
        [Test]
        public void FailedTraining_RestartsTheSameLot_ChangingNothing()
        {
            StartReal();
            AssertPlay(Fail(MT), MT, RunOutcome.RestartedTraining);
            Assert.IsFalse(_progress.IsCleared(MT));
            Assert.AreEqual(0, _granted.Count);
            AssertPlay(Clear(MT), MC, RunOutcome.Advanced);
            AssertPlay(Clear(MC), VT, RunOutcome.Advanced);
            AssertPlay(Fail(VT), VT, RunOutcome.RestartedTraining);
        }
        [Test]
        public void FailedCrazy_GoesBackToMidgardCrazy_KeepingPowersAndClearedFlags()
        {
            StartReal();
            Clear(MT);
            Clear(MC);
            Clear(VT);
            Assert.AreEqual(1, _progress.PowerLevel(StoryPower.ThunderStride));
            AssertPlay(Fail(VC), MC, RunOutcome.SentBack);
            Assert.IsTrue(_progress.IsCleared(MT));
            Assert.IsTrue(_progress.IsCleared(MC));
            Assert.IsTrue(_progress.IsCleared(VT));
            Assert.IsFalse(_progress.IsCleared(VC));
            Assert.AreEqual(1, _progress.PowerLevel(StoryPower.ThunderStride));
            Assert.IsFalse(_progress.IsRunComplete);
        }
        [Test]
        public void SentBackClimb_SkipsClearedTrainingLots_AndFindsTheSameCrazyLotAgain()
        {
            StartReal();
            Clear(MT);
            Clear(MC);
            Clear(VT);
            Fail(VC);
            AssertPlay(Clear(MC), VC, RunOutcome.Advanced);
        }
        [Test]
        public void TrainingNotYetCleared_IsPlayedEvenOnAClimb()
        {
            StartReal();
            Clear(MT);
            Clear(MC);
            Fail(MC);
            AssertPlay(Clear(MC), VT, RunOutcome.Advanced);
        }
        [Test]
        public void CrazyLotGrantsItsPowerOnce_AndAReplayGrantsNothingMore()
        {
            StartReal();
            Clear(MT);
            Clear(MC);
            Assert.AreEqual(1, _granted.Count);
            Assert.AreEqual(StoryPower.ThunderStride, _granted[0].Power);
            Assert.AreEqual(1, _granted[0].Level);
            Fail(MC);
            Clear(MC);
            Assert.AreEqual(1, _granted.Count);
            Assert.AreEqual(1, _progress.PowerLevel(StoryPower.ThunderStride));
            Assert.AreEqual(1, _progress.EarnedPowers.Count);
        }
        [Test]
        public void TrainingLotsGrantNothing()
        {
            StartReal();
            Clear(MT);
            Assert.AreEqual(0, _granted.Count);
            Assert.AreEqual(0, _progress.EarnedPowers.Count);
        }
        [Test]
        public void BestResultKeepsTheMaximum()
        {
            StartReal();
            _progress.SetCurrentLot(MT);
            _progress.RecordBest(MT, 7);
            _progress.RecordBest(MT, 3);
            Clear(MT);
            Assert.AreEqual(7, _progress.BestBlessings(MT));
        }
        [Test]
        public void StartNewRun_ResetsEverything()
        {
            StartReal();
            Clear(MT);
            Clear(MC);
            _progress.SetCurrentLot(VC);
            _progress.RecordBest(MC, 9);
            Clear(VT);
            Clear(VC);
            Assert.IsTrue(_progress.IsRunComplete);
            _progress.StartNewRun();
            Assert.IsFalse(_progress.IsRunComplete);
            Assert.AreEqual(RunEnd.None, _progress.EndReason);
            Assert.IsFalse(_progress.HasNextLot);
            Assert.IsFalse(_progress.HasCurrentLot);
            Assert.AreEqual(0, _progress.EarnedPowers.Count);
            Assert.AreEqual(0, _progress.PowerLevel(StoryPower.ThunderStride));
            Assert.IsFalse(_progress.IsCleared(MT));
            Assert.IsFalse(_progress.IsCleared(VC));
            Assert.AreEqual(0, _progress.BestBlessings(MC));
            AssertPlay(Clear(MT), MC, RunOutcome.Advanced);
        }
        [Test]
        public void Dispose_StopsReacting()
        {
            StartReal();
            _flow.Dispose();
            _bus.Publish(new LotCleared(MT));
            Assert.AreEqual(0, _chosen.Count);
            _flow = null;
        }
    }
}
