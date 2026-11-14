using System;
using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.UI.Flow
{
    /// <summary>
    /// The pure state machine of the story flow: title, how-to-play (once per page load), the intro card of each lot, the shift, the results,
    /// reward cards, the sent-back card and the end card. It holds no scene objects, so one instance outlives every lot scene. Inputs are plain
    /// calls (a key press, the shift ending, the run's decisions, a scene loading); <see cref="Advance"/> says what the host must do next.
    /// The run's rules live in the Run module: this only reacts to what they decided.
    /// </summary>
    public sealed class FlowMachine
    {
        private readonly Func<LotId> _startLot;
        private readonly List<PendingReward> _rewards = new List<PendingReward>();
        private bool _howToPlayShown;
        private bool _sceneDirty;
        private bool _hasLoaded;

        /// <param name="startLot">The lot the story starts at: the first lot of the run that is not cleared yet. Asked when the title is dismissed.</param>
        public FlowMachine(Func<LotId> startLot)
        {
            _startLot = startLot ?? throw new ArgumentNullException(nameof(startLot));
        }

        public FlowState State { get; private set; } = FlowState.Title;

        /// <summary>Counts every screen change (also a second reward card after a first), so a view can tell when a new card started.</summary>
        public int Version { get; private set; }

        /// <summary>The lot whose intro card shows or showed last.</summary>
        public LotId IntroLot { get; private set; }

        /// <summary>The lot of the scene that is loaded now; meaningful once a scene reported itself.</summary>
        public LotId LoadedLot { get; private set; }

        /// <summary>True from a load request until the scene reports itself: further presses do nothing.</summary>
        public bool LoadPending { get; private set; }

        public bool HasNextLot { get; private set; }
        public LotId NextLot { get; private set; }
        public RunOutcome Outcome { get; private set; }
        public bool IsRunComplete { get; private set; }
        public RunEnd End { get; private set; }

        /// <summary>The reward the Reward card shows; meaningful in <see cref="FlowState.Reward"/>.</summary>
        public PendingReward Reward => _rewards.Count > 0 ? _rewards[0] : default;

        public int RewardCount => _rewards.Count;

        /// <summary>A press on any key (or the mouse): the next screen, or the shift. Does nothing while a shift runs or a scene loads.</summary>
        public FlowStep Advance()
        {
            if (LoadPending) return FlowStep.None;
            switch (State)
            {
                case FlowState.Title: return AdvanceTitle();
                case FlowState.HowToPlay: return EnterIntro(_startLot());
                case FlowState.LotIntro: return StartShift();
                case FlowState.Results: return AfterResults();
                case FlowState.Reward: return AfterReward();
                case FlowState.SentBack: return EnterIntro(HasNextLot ? NextLot : LoadedLot);
                case FlowState.EndCard: return StartNewRun();
                default: return FlowStep.None;
            }
        }

        /// <summary>The developer picker chose a lot: its intro card, in a fresh scene of it.</summary>
        public FlowStep DevStart(LotId lot) => EnterIntro(lot);

        /// <summary>The player retries from the results screen: back to a shift of the same lot, no cards.</summary>
        public bool Retry()
        {
            if (State != FlowState.Results) return false;
            ShiftStarted();
            return true;
        }

        /// <summary>A shift began (the first, a retry): the screen is the HUD and the previous decision is forgotten.</summary>
        public void ShiftStarted()
        {
            _sceneDirty = true;
            HasNextLot = false;
            IsRunComplete = false;
            End = RunEnd.None;
            if (State != FlowState.Playing) Enter(FlowState.Playing);
        }

        /// <summary>The shift ended and the results are ready.</summary>
        public void ShiftEnded()
        {
            if (State == FlowState.Playing) Enter(FlowState.Results);
        }

        /// <summary>The run chose the next lot (<see cref="IRunFlowState"/> already says so).</summary>
        public void NextLotChosen(LotId next, RunOutcome reason)
        {
            NextLot = next;
            Outcome = reason;
            HasNextLot = reason != RunOutcome.RunComplete;
        }

        /// <summary>The run is over.</summary>
        public void RunCompleted(RunEnd reason)
        {
            IsRunComplete = true;
            End = reason;
        }

        /// <summary>A power or upgrade was earned: a reward card is due after the results. Ignored outside a shift or its results (the developer panel).</summary>
        public void PowerGranted(StoryPower power, int level)
        {
            if (State == FlowState.Playing || State == FlowState.Results) _rewards.Add(new PendingReward(power, level));
        }

        /// <summary>A lot scene finished building. A scene loaded outside the flow (the developer path) gets its intro card.</summary>
        public void SceneLoaded(LotId lot)
        {
            LoadedLot = lot;
            _hasLoaded = true;
            _sceneDirty = false;
            LoadPending = false;
            if (State != FlowState.Title && State != FlowState.HowToPlay && State != FlowState.LotIntro) EnterIntro(lot);
        }

        /// <summary>Jumps straight to the running shift (developer and test hook): no cards.</summary>
        public void SkipToPlaying() => ShiftStarted();

        private FlowStep AdvanceTitle()
        {
            if (_howToPlayShown) return EnterIntro(_startLot());
            _howToPlayShown = true;
            Enter(FlowState.HowToPlay);
            return FlowStep.None;
        }

        private FlowStep StartShift()
        {
            Enter(FlowState.Playing);
            return FlowStep.StartShift;
        }

        private FlowStep EnterIntro(LotId lot)
        {
            IntroLot = lot;
            Enter(FlowState.LotIntro);
            if (_hasLoaded && LoadedLot == lot && !_sceneDirty) return FlowStep.None;
            LoadPending = true;
            return FlowStep.LoadLot;
        }

        private FlowStep AfterResults()
        {
            if (_rewards.Count > 0) { Enter(FlowState.Reward); return FlowStep.None; }
            return AfterRewards();
        }

        private FlowStep AfterReward()
        {
            _rewards.RemoveAt(0);
            if (_rewards.Count > 0) { Enter(FlowState.Reward); return FlowStep.None; }
            return AfterRewards();
        }

        private FlowStep AfterRewards()
        {
            if (IsRunComplete) { Enter(FlowState.EndCard); return FlowStep.None; }
            if (HasNextLot && Outcome == RunOutcome.SentBack) { Enter(FlowState.SentBack); return FlowStep.None; }
            return EnterIntro(HasNextLot ? NextLot : LoadedLot);
        }

        private FlowStep StartNewRun()
        {
            HasNextLot = false;
            IsRunComplete = false;
            End = RunEnd.None;
            _rewards.Clear();
            Enter(FlowState.Title);
            return FlowStep.StartNewRun;
        }

        private void Enter(FlowState state)
        {
            State = state;
            Version++;
        }
    }
}
