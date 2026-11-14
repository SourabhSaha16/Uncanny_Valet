using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Run
{
    /// <summary>
    /// Applies the run rules when a lot ends: LotCleared marks the lot cleared and grants its power once, LotFailed
    /// changes nothing; both then ask <see cref="NextLotRules"/>, store the answer on the <see cref="RunProgress"/> and
    /// publish NextLotChosen (and RunCompleted when the run is over). Loads nothing. Created per scene; dispose with the scene.
    /// </summary>
    public sealed class RunFlow : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly RunProgress _progress;
        private readonly RunDefinition _run;
        private readonly NextLotRules _rules;

        public RunFlow(IEventBus bus, RunProgress progress, RunDefinition run)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _run = run ?? throw new ArgumentNullException(nameof(run));
            _rules = new NextLotRules(run);
            _bus.Subscribe<LotCleared>(OnCleared);
            _bus.Subscribe<LotFailed>(OnFailed);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<LotCleared>(OnCleared);
            _bus.Unsubscribe<LotFailed>(OnFailed);
        }

        private void OnCleared(LotCleared e)
        {
            if (!_run.TryGet(e.Lot, out var lot))
            {
                Debug.LogError("RunFlow: the run definition does not list the cleared lot " + e.Lot + ".");
                return;
            }
            _progress.MarkCleared(e.Lot);
            GrantReward(lot.Grant);
            Decide(e.Lot, true);
        }

        private void OnFailed(LotFailed e) => Decide(e.Lot, false);

        private void GrantReward(PowerGrant grant)
        {
            if (grant.IsNone) return;
            var level = Math.Min(grant.Level, PowerGrant.MaxLevel);
            if (_progress.PowerLevel(grant.Power) >= level) return;
            _progress.GrantPower(grant.Power, level);
            _bus.Publish(new PowerGranted(grant.Power, level));
        }

        private void Decide(UncannyValet.Core.Ids.LotId lot, bool cleared)
        {
            if (!_rules.TryDecide(_progress, lot, cleared, out var decision))
            {
                Debug.LogError("RunFlow: no next lot after " + lot + " (cleared " + cleared + "); the run definition lacks it or Midgard's crazy lot.");
                return;
            }
            _progress.SetDecision(decision);
            _bus.Publish(new NextLotChosen(decision.Next, decision.Reason));
            if (decision.IsRunComplete) _bus.Publish(new RunCompleted(decision.End));
        }
    }
}
