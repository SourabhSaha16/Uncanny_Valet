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

        /// <summary>A director-made dilemma is open (its car has not been parked and its trigger still holds).</summary>
        bool HasOpenDilemma { get; }

        /// <summary>The open dilemma's rule (meaningful when <see cref="HasOpenDilemma"/>).</summary>
        DilemmaKind OpenDilemma { get; }

        /// <summary>Seconds until another dilemma may be made (0 when none is blocked by the cooldown).</summary>
        float DilemmaCooldownLeft { get; }

        /// <summary>Dilemmas made this shift.</summary>
        int DilemmasMade { get; }

        /// <summary>Dilemmas resolved this shift.</summary>
        int DilemmasResolved { get; }

        /// <summary>Mean seconds a resolved dilemma stayed open; 0 before the first.</summary>
        float AverageResolveSeconds { get; }

        /// <summary>The last pick asked for a parked patron's return to come forward (lot tight).</summary>
        bool ReturnForwardWanted { get; }

        /// <summary>Increases whenever a decision is logged; lets a view cache its text.</summary>
        int DecisionVersion { get; }

        /// <summary>Entries in the log (at most five).</summary>
        int DecisionCount { get; }

        /// <summary>The nth newest decision; 0 is the latest.</summary>
        DirectorDecision GetDecision(int newestFirst);
    }
}
