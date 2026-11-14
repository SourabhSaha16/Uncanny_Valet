using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Flow;

namespace UncannyValet.Tests.Flow
{
    /// <summary>The key delay, the consumed press and the actions the flow asks for.</summary>
    public sealed class FlowControllerTests
    {
        private const float CardDelay = 0.3f;
        private const float ResultsDelay = 0.8f;
        private static readonly LotId MT = new LotId(World.Midgard, LotKind.Training);
        private static readonly LotId MC = new LotId(World.Midgard, LotKind.Crazy);

        private sealed class Actions : IFlowActions
        {
            public readonly List<string> Calls = new List<string>();
            public void StartShift() => Calls.Add("start");
            public void RetryShift() => Calls.Add("retry");
            public void LoadLot(LotId lot) => Calls.Add("load " + lot);
            public void StartNewRun() => Calls.Add("new run");
        }

        private FlowMachine _machine;
        private FlowController _flow;
        private Actions _actions;
        private int _frame;
        private float _now;

        [SetUp]
        public void SetUp()
        {
            _machine = new FlowMachine(() => MT);
            _machine.SceneLoaded(MT);
            _actions = new Actions();
            _flow = new FlowController(_machine, _actions, CardDelay, ResultsDelay);
            _frame = 0;
            _now = 0f;
            Tick(false);
        }

        private void Tick(bool press, float seconds = 0.016f, bool keyboard = true, bool retry = false)
        {
            _now += seconds;
            _frame++;
            _flow.Update(_now, _frame, press, press && keyboard, press && retry);
        }

        // One frame for the new screen to register, then a press after its delay.
        private void Press()
        {
            Tick(false);
            Tick(true, CardDelay + 0.1f);
        }

        [Test]
        public void AKeyBeforeTheDelay_DoesNothing_AfterItAdvances()
        {
            Tick(true, 0.1f);
            Assert.AreEqual(FlowState.Title, _machine.State);
            Tick(true, CardDelay);
            Assert.AreEqual(FlowState.HowToPlay, _machine.State);
        }

        [Test]
        public void ThePressThatDismissedACard_DoesNotAlsoDismissTheNext()
        {
            Tick(true, CardDelay + 0.1f);
            Assert.AreEqual(FlowState.HowToPlay, _machine.State);
            Tick(true, 0.016f);
            Assert.AreEqual(FlowState.HowToPlay, _machine.State, "the next card needs a fresh press after its own delay");
            Tick(true, CardDelay + 0.1f);
            Assert.AreEqual(FlowState.LotIntro, _machine.State);
        }

        [Test]
        public void TheIntroKey_StartsTheShift()
        {
            Press();
            Press();
            Press();
            Assert.AreEqual(FlowState.Playing, _machine.State);
            CollectionAssert.AreEqual(new[] { "start" }, _actions.Calls);
        }

        [Test]
        public void LoadingALot_IsAskedOnceEvenWhenKeysKeepComing()
        {
            _machine.ShiftStarted();
            _machine.ShiftEnded();
            _machine.NextLotChosen(MC, RunOutcome.Advanced);
            Tick(false);
            Tick(true, ResultsDelay + 0.1f);
            Press();
            Press();
            CollectionAssert.AreEqual(new[] { "load " + MC }, _actions.Calls);
        }

        [Test]
        public void Results_WaitTheirOwnLongerDelay()
        {
            _machine.ShiftStarted();
            _machine.ShiftEnded();
            _machine.NextLotChosen(MC, RunOutcome.Advanced);
            Tick(false);
            Tick(true, CardDelay + 0.1f);
            Assert.AreEqual(FlowState.Results, _machine.State);
            Tick(true, ResultsDelay);
            Assert.AreEqual(FlowState.LotIntro, _machine.State);
        }

        [Test]
        public void Results_EnterRetries_AndTheMouseAloneDoesNothing()
        {
            _machine.ShiftStarted();
            _machine.ShiftEnded();
            Tick(false);
            Tick(true, ResultsDelay + 0.1f, keyboard: false);
            Assert.AreEqual(FlowState.Results, _machine.State);
            Tick(true, 0.1f, retry: true);
            Assert.AreEqual(FlowState.Playing, _machine.State);
            CollectionAssert.AreEqual(new[] { "retry" }, _actions.Calls);
        }

        [Test]
        public void TheNextLotButton_ContinuesTheFlowAndRetryTheShift()
        {
            _machine.ShiftStarted();
            _machine.ShiftEnded();
            _machine.NextLotChosen(MT, RunOutcome.RestartedTraining);
            _flow.Continue();
            Assert.AreEqual(FlowState.LotIntro, _machine.State);
            CollectionAssert.AreEqual(new[] { "load " + MT }, _actions.Calls);
        }

        [Test]
        public void DevStart_LoadsTheChosenLot()
        {
            _flow.DevStart(MC);
            CollectionAssert.AreEqual(new[] { "load " + MC }, _actions.Calls);
            Assert.AreEqual(FlowState.LotIntro, _machine.State);
        }

        [Test]
        public void SkipToPlaying_StartsTheShiftWithoutCards()
        {
            _flow.SkipToPlaying();
            Assert.AreEqual(FlowState.Playing, _machine.State);
            CollectionAssert.AreEqual(new[] { "start" }, _actions.Calls);
        }

        [Test]
        public void KeysDoNothing_WhileAShiftRuns()
        {
            _flow.SkipToPlaying();
            Tick(true, 5f);
            Assert.AreEqual(FlowState.Playing, _machine.State);
        }
    }
}
