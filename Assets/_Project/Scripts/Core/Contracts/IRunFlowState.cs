using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// Read-only view of where the run goes next, updated before <c>NextLotChosen</c> and <c>RunCompleted</c> are published.
    /// The between-lot flow reads it instead of keeping its own copy.
    /// </summary>
    public interface IRunFlowState
    {
        /// <summary>True once the run is complete (finale cleared or the last lot of the definition cleared).</summary>
        bool IsRunComplete { get; }

        /// <summary>How the run ended; <see cref="RunEnd.None"/> until complete.</summary>
        RunEnd EndReason { get; }

        /// <summary>True when a next lot was chosen and the run is not complete.</summary>
        bool HasNextLot { get; }

        /// <summary>The lot chosen to play next; meaningful only when <see cref="HasNextLot"/>.</summary>
        LotId NextLot { get; }

        /// <summary>Why <see cref="NextLot"/> was chosen; meaningful once a decision was made.</summary>
        RunOutcome LastOutcome { get; }
    }
}
