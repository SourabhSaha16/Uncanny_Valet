using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Run
{
    /// <summary>
    /// The roguelite rules that turn a lot outcome into the next lot. Cleared: the next lot in the run's order, skipping
    /// training lots already cleared; the finale, or the last lot of the definition, completes the run. Failed training:
    /// the same lot. Failed crazy lot or finale: Midgard's crazy lot (found by id). Pure: reads the progress, changes nothing.
    /// </summary>
    public sealed class NextLotRules
    {
        private static readonly LotId SendBackTarget = new LotId(World.Midgard, LotKind.Crazy);
        private readonly RunDefinition _run;

        public NextLotRules(RunDefinition run)
        {
            _run = run ?? throw new ArgumentNullException(nameof(run));
        }

        /// <summary>Decides what follows the lot. False when the run does not list the lot (or Midgard's crazy lot for a send-back).</summary>
        public bool TryDecide(IRunProgress progress, LotId lot, bool cleared, out LotDecision decision)
        {
            decision = default;
            if (_run.IndexOf(lot) < 0) return false;
            return cleared ? TryDecideCleared(progress, lot, out decision) : TryDecideFailed(lot, out decision);
        }

        private bool TryDecideCleared(IRunProgress progress, LotId lot, out LotDecision decision)
        {
            decision = LotDecision.Complete(RunEnd.EndOfContent);
            if (lot.Kind == LotKind.Finale)
            {
                decision = LotDecision.Complete(RunEnd.Finale);
                return true;
            }
            var lots = _run.Lots;
            for (var i = _run.IndexOf(lot) + 1; i < lots.Count; i++)
            {
                var candidate = lots[i].Id;
                if (candidate.Kind == LotKind.Training && progress.IsCleared(candidate)) continue;
                decision = LotDecision.Play(candidate, RunOutcome.Advanced);
                return true;
            }
            return true;
        }

        private bool TryDecideFailed(LotId lot, out LotDecision decision)
        {
            if (lot.Kind == LotKind.Training)
            {
                decision = LotDecision.Play(lot, RunOutcome.RestartedTraining);
                return true;
            }
            decision = LotDecision.Play(SendBackTarget, RunOutcome.SentBack);
            return _run.IndexOf(SendBackTarget) >= 0;
        }
    }
}
