using System.Collections.Generic;
using UncannyValet.Core.Watchers;

namespace UncannyValet.Watchers
{
    /// <summary>
    /// Pure rules of the speech bubbles: one at a time, each for a fixed time, a lockout between the starts of two bubbles, and when
    /// several requests coincide only the highest priority shows (the earliest of equals). Requests that arrive while a bubble is showing
    /// or locked out are dropped, never queued.
    /// </summary>
    public sealed class SpeechScheduler
    {
        private readonly List<SpeechRequest> _pending = new List<SpeechRequest>(8);
        private readonly float _bubbleSeconds;
        private readonly float _minGapSeconds;
        private float _shownUntil = float.NegativeInfinity;
        private float _lockedUntil = float.NegativeInfinity;

        public SpeechScheduler(float bubbleSeconds, float minGapSeconds)
        {
            _bubbleSeconds = bubbleSeconds;
            _minGapSeconds = minGapSeconds;
        }

        /// <summary>Seconds a bubble stays.</summary>
        public float BubbleSeconds => _bubbleSeconds;

        /// <summary>Remembers a request until the next <see cref="Tick"/>.</summary>
        public void Submit(in SpeechRequest request) => _pending.Add(request);

        /// <summary>True while a bubble is on screen.</summary>
        public bool IsShowing(float now) => now < _shownUntil;

        /// <summary>
        /// Decides what happens to the requests submitted since the last tick: shows the best one when nothing is showing and the lockout
        /// is over, and drops all the others.
        /// </summary>
        public bool Tick(float now, out SpeechRequest shown)
        {
            shown = default;
            if (_pending.Count == 0) return false;
            var canShow = now >= _shownUntil && now >= _lockedUntil;
            if (canShow)
            {
                shown = Best();
                _shownUntil = now + _bubbleSeconds;
                _lockedUntil = now + _minGapSeconds;
            }
            _pending.Clear();
            return canShow;
        }

        /// <summary>Forgets the requests and the lockouts (a new shift).</summary>
        public void Reset()
        {
            _pending.Clear();
            _shownUntil = float.NegativeInfinity;
            _lockedUntil = float.NegativeInfinity;
        }

        private SpeechRequest Best()
        {
            var best = _pending[0];
            for (var i = 1; i < _pending.Count; i++)
                if (_pending[i].Priority > best.Priority) best = _pending[i];
            return best;
        }
    }
}
