using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>
    /// The director's published numbers and its last few decisions. The director brain writes it; the debug overlay
    /// reads it through <see cref="IDirectorView"/>.
    /// </summary>
    public sealed class DirectorState : IDirectorView
    {
        /// <summary>How many decisions the log keeps.</summary>
        public const int LogSize = 5;

        private readonly DirectorDecision[] _log = new DirectorDecision[LogSize];
        private int _next;
        private float _resolveSeconds;

        public bool Active { get; set; }
        public float Workload { get; set; }
        public float Pressure { get; set; }
        public float Target { get; set; }
        public float Form { get; set; }
        public DirectorPhase Phase { get; set; }
        public bool InRelief { get; set; }
        public float SecondsToNextArrival { get; set; }
        public int ArrivalsUsed { get; set; }
        public int ArrivalBudget { get; set; }
        public int ArrivalBudgetMax { get; set; }
        public bool HasOpenDilemma { get; set; }
        public DilemmaKind OpenDilemma { get; set; }
        public float DilemmaCooldownLeft { get; set; }
        public int DilemmasMade { get; private set; }
        public int DilemmasResolved { get; private set; }
        public bool ReturnForwardWanted { get; set; }
        public float AverageResolveSeconds => DilemmasResolved > 0 ? _resolveSeconds / DilemmasResolved : 0f;
        public int DecisionVersion { get; private set; }
        public int DecisionCount { get; private set; }

        public DirectorDecision GetDecision(int newestFirst)
            => _log[(_next - 1 - newestFirst + LogSize * 2) % LogSize];

        /// <summary>Adds a decision to the log, dropping the oldest when full.</summary>
        public void Log(in DirectorDecision decision)
        {
            _log[_next] = decision;
            _next = (_next + 1) % LogSize;
            if (DecisionCount < LogSize) DecisionCount++;
            DecisionVersion++;
        }

        /// <summary>Counts a dilemma that opened.</summary>
        public void CountMade() => DilemmasMade++;

        /// <summary>Counts a dilemma that resolved after this many seconds.</summary>
        public void CountResolved(float seconds)
        {
            DilemmasResolved++;
            _resolveSeconds += seconds;
        }

        /// <summary>Clears the counters and the log for a new shift.</summary>
        public void Reset()
        {
            _next = 0;
            DecisionCount = 0;
            DecisionVersion++;
            Workload = 0f; Pressure = 0f; Target = 0f; Form = 0f;
            InRelief = false;
            SecondsToNextArrival = 0f;
            ArrivalsUsed = 0;
            HasOpenDilemma = false;
            DilemmaCooldownLeft = 0f;
            DilemmasMade = 0;
            DilemmasResolved = 0;
            _resolveSeconds = 0f;
            ReturnForwardWanted = false;
        }
    }
}
