using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>
    /// Live speed scales of the patron trips, set from the debug panel: entering cars (lane cruise and the turn into the
    /// queue) and leaving cars (curb route and the lane after merging). Each is the tuned speed divided by the config
    /// speed it replaces, so the config values give a scale of 1.
    /// </summary>
    public sealed class TripSpeeds
    {
        /// <summary>The lowest scale a trip may run at, so a car never stops dead on the road.</summary>
        public const float MinScale = 0.05f;
        private const float MinReference = 0.01f;

        private readonly float _entryReference;
        private readonly float _exitReference;

        /// <param name="entryReference">The config entry speed (lane cruise) that gives scale 1.</param>
        /// <param name="exitReference">The config exit speed (curb route) that gives scale 1.</param>
        public TripSpeeds(float entryReference, float exitReference)
        {
            _entryReference = Mathf.Max(MinReference, entryReference);
            _exitReference = Mathf.Max(MinReference, exitReference);
        }

        /// <summary>Multiplier on the cruise and turn speeds of entering cars.</summary>
        public float EntryScale { get; private set; } = 1f;

        /// <summary>Multiplier on the route and cruise speeds of leaving cars.</summary>
        public float ExitScale { get; private set; } = 1f;

        /// <summary>Sets the entry speed in units per second.</summary>
        public void SetEntrySpeed(float speed) => EntryScale = Mathf.Max(MinScale, speed / _entryReference);

        /// <summary>Sets the exit speed in units per second.</summary>
        public void SetExitSpeed(float speed) => ExitScale = Mathf.Max(MinScale, speed / _exitReference);
    }
}
