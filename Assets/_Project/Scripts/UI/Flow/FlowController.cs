using System;

namespace UncannyValet.UI.Flow
{
    /// <summary>
    /// Turns presses into flow steps and carries them out: waits the key delay after every new screen (so a held key cannot skip it), consumes
    /// the press that changed the screen (it never also dismisses the next one) and hands each step to the <see cref="IFlowActions"/>.
    /// </summary>
    public sealed class FlowController
    {
        private readonly FlowMachine _machine;
        private readonly IFlowActions _actions;
        private readonly float _cardDelay;
        private readonly float _resultsDelay;
        private int _version = -1;
        private float _enteredAt;
        private int _enteredFrame;

        public FlowController(FlowMachine machine, IFlowActions actions, float cardDelaySeconds, float resultsDelaySeconds)
        {
            _machine = machine ?? throw new ArgumentNullException(nameof(machine));
            _actions = actions ?? throw new ArgumentNullException(nameof(actions));
            _cardDelay = cardDelaySeconds;
            _resultsDelay = resultsDelaySeconds;
        }

        public FlowMachine Machine => _machine;

        /// <summary>Seconds since the current screen appeared.</summary>
        public float SecondsOnScreen(float now) => now - _enteredAt;

        /// <summary>
        /// Call once per frame. <paramref name="anyPressed"/>: any key or the mouse went down this frame; <paramref name="keyPressed"/>: a key (not the
        /// mouse) did, which is all the results screen reacts to (its buttons call <see cref="Retry"/> and <see cref="Continue"/>);
        /// <paramref name="retryPressed"/>: Enter or Space did.
        /// </summary>
        public void Update(float now, int frame, bool anyPressed, bool keyPressed, bool retryPressed)
        {
            Sync(now, frame);
            if (frame == _enteredFrame || _machine.State == FlowState.Playing) return;
            if (_machine.State == FlowState.Results)
            {
                if (!keyPressed || now - _enteredAt < _resultsDelay) return;
                if (retryPressed) Retry(); else Continue();
                return;
            }
            if (anyPressed && now - _enteredAt >= _cardDelay) Perform(_machine.Advance());
        }

        /// <summary>Retry button or key on the results screen.</summary>
        public void Retry()
        {
            if (_machine.Retry()) _actions.RetryShift();
        }

        /// <summary>Next lot button or key on the results screen: on through the cards.</summary>
        public void Continue()
        {
            if (_machine.State == FlowState.Results) Perform(_machine.Advance());
        }

        /// <summary>The developer picker chose a lot.</summary>
        public void DevStart(UncannyValet.Core.Ids.LotId lot) => Perform(_machine.DevStart(lot));

        /// <summary>Test and developer hook: straight to the shift, no cards.</summary>
        public void SkipToPlaying()
        {
            _machine.SkipToPlaying();
            _actions.StartShift();
        }

        private void Sync(float now, int frame)
        {
            if (_machine.Version == _version) return;
            _version = _machine.Version;
            _enteredAt = now;
            _enteredFrame = frame;
        }

        private void Perform(FlowStep step)
        {
            switch (step)
            {
                case FlowStep.StartShift: _actions.StartShift(); break;
                case FlowStep.LoadLot: _actions.LoadLot(_machine.IntroLot); break;
                case FlowStep.StartNewRun: _actions.StartNewRun(); break;
            }
        }
    }
}
