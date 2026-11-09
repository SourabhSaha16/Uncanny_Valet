using System;

namespace UncannyValet.Night
{
    /// <summary>The shift's countdown in real seconds. Pure: it reports the final stretch and the end once each.</summary>
    public sealed class ShiftTimer
    {
        private readonly float _length;
        private readonly float _finalStretch;
        private bool _finalReported;

        public ShiftTimer(float lengthSeconds, float finalStretchSeconds)
        {
            _length = Math.Max(0f, lengthSeconds);
            _finalStretch = Math.Max(0f, finalStretchSeconds);
            SecondsRemaining = _length;
        }

        public float LengthSeconds => _length;
        public float SecondsRemaining { get; private set; }
        public bool IsEnded => SecondsRemaining <= 0f;

        /// <summary>Counts down by real seconds. Each flag in the result appears on exactly one call.</summary>
        public ShiftTimerStep Advance(float seconds)
        {
            if (IsEnded) return ShiftTimerStep.None;
            SecondsRemaining = Math.Max(0f, SecondsRemaining - Math.Max(0f, seconds));
            var step = ShiftTimerStep.None;
            if (!_finalReported && _finalStretch < _length && SecondsRemaining <= _finalStretch)
            {
                _finalReported = true;
                step |= ShiftTimerStep.EnteredFinalStretch;
            }
            if (IsEnded) step |= ShiftTimerStep.Ended;
            return step;
        }
    }
}
