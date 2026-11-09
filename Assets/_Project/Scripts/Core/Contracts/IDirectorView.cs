using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Read-only live numbers of the Patron Director, for the debug overlay.</summary>
    public interface IDirectorView
    {
        /// <summary>True while the director (not the old fixed timer) paces arrivals.</summary>
        bool Active { get; }
        float Workload { get; }
        float Pressure { get; }
        float Target { get; }
        float Form { get; }
        DirectorPhase Phase { get; }
        bool InRelief { get; }
        float SecondsToNextArrival { get; }
        int ArrivalsUsed { get; }
        int ArrivalBudget { get; }
        int ArrivalBudgetMax { get; }

        /// <summary>Increases whenever a decision is logged; lets a view cache its text.</summary>
        int DecisionVersion { get; }

        /// <summary>Entries in the log (at most five).</summary>
        int DecisionCount { get; }

        /// <summary>The nth newest decision; 0 is the latest.</summary>
        DirectorDecision GetDecision(int newestFirst);
    }
}
