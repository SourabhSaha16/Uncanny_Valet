using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>
    /// Pure rule (issue 70, like Overcooked there is always an open ticket): when the arrival queue has been empty for
    /// the configured limit, a car must be sent now. It only measures an empty queue, so a queue with cars in it never
    /// triggers it and every normal pacing, relief and queue-full rule still applies. It stands down when the shift's
    /// arrival budget is spent or too little of the shift is left for a new car to be useful.
    /// </summary>
    public sealed class IdleGuard
    {
        /// <summary>Returned by <see cref="SecondsUntilForced"/> when the guard cannot force a car right now.</summary>
        public const float NotArmed = -1f;

        private readonly PacingSettings _settings;
        private float _emptySeconds;
        private bool _armed;

        public IdleGuard(PacingSettings settings) { _settings = settings; }

        /// <summary>Seconds the queue may stay empty at most; the pacer also uses it to shorten a lost-send wait.</summary>
        public float Limit => _settings.MaxEmptyQueueSeconds;

        /// <summary>True while the queue is empty and the guard may send, so the limit is counting down.</summary>
        public bool Armed => _armed;

        /// <summary>Seconds until a car is forced, or <see cref="NotArmed"/>.</summary>
        public float SecondsUntilForced => _armed ? Mathf.Max(0f, Limit - _emptySeconds) : NotArmed;

        /// <summary>Starts a new shift: the first car is due one limit after the shift starts.</summary>
        public void Reset()
        {
            _emptySeconds = 0f;
            _armed = false;
        }

        /// <summary>Counts this tick. Returns true when a car must be sent now.</summary>
        public bool Update(in PacerInput input, float dt)
        {
            if (input.Queue > 0)
            {
                _emptySeconds = 0f;
                _armed = false;
                return false;
            }
            _emptySeconds += dt;
            _armed = input.ArrivalsUsed < _settings.MaxArrivals && input.ShiftSecondsLeft >= _settings.IdleCutoffSeconds;
            return _armed && _emptySeconds >= Limit;
        }
    }
}
