using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Run
{
    /// <summary>What the run rules decided after a lot ended: the next lot and why, or that the run is complete and how.</summary>
    public readonly struct LotDecision
    {
        public readonly LotId Next;
        public readonly RunOutcome Reason;
        public readonly RunEnd End;

        private LotDecision(LotId next, RunOutcome reason, RunEnd end)
        {
            Next = next;
            Reason = reason;
            End = end;
        }

        /// <summary>True when the run is over and there is no next lot.</summary>
        public bool IsRunComplete => Reason == RunOutcome.RunComplete;

        public static LotDecision Play(LotId next, RunOutcome reason) => new LotDecision(next, reason, RunEnd.None);

        public static LotDecision Complete(RunEnd end) => new LotDecision(default, RunOutcome.RunComplete, end);
    }
}
