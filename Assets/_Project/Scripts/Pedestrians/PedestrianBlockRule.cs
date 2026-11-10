using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Pure rule for a person whose path may be blocked by a car, the valet, a wall or another person. While blocked
    /// they wait up to the wait time; if the blocker does not clear they pass through for the pass time (a fallback,
    /// so a route can never freeze), then the rule starts over.
    /// </summary>
    public sealed class PedestrianBlockRule
    {
        private float _waitSeconds = 2f;
        private float _passSeconds = 1.5f;
        private float _waited;
        private float _passLeft;

        /// <summary>The state decided by the last <see cref="Evaluate"/>.</summary>
        public PedestrianBlockState State { get; private set; }

        /// <summary>Sets how long to wait and how long to pass through, and starts over.</summary>
        public void Configure(float waitSeconds, float passSeconds)
        {
            _waitSeconds = Mathf.Max(0f, waitSeconds);
            _passSeconds = Mathf.Max(0f, passSeconds);
            Reset();
        }

        /// <summary>Forgets any waiting (a new walk begins).</summary>
        public void Reset()
        {
            _waited = 0f;
            _passLeft = 0f;
            State = PedestrianBlockState.Moving;
        }

        /// <summary>Advances the rule by one step. <paramref name="blockedAhead"/> is true when something solid is in the way.</summary>
        public PedestrianBlockState Evaluate(float deltaSeconds, bool blockedAhead)
        {
            if (State == PedestrianBlockState.Passing && (_passLeft -= deltaSeconds) > 0f) return State;
            if (!blockedAhead)
            {
                Reset();
                return State;
            }
            _waited += deltaSeconds;
            if (_waited < _waitSeconds) return State = PedestrianBlockState.Waiting;
            _waited = 0f;
            _passLeft = _passSeconds;
            return State = _passSeconds > 0f ? PedestrianBlockState.Passing : PedestrianBlockState.Moving;
        }
    }
}
