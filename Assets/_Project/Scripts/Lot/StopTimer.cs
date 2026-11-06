namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure "stopped and empty for N seconds" rule. Fires once, then stays done until <see cref="Reset"/>.
    /// </summary>
    public sealed class StopTimer
    {
        private readonly float _required;
        private float _elapsed;

        public StopTimer(float requiredSeconds) { _required = requiredSeconds; }

        public bool IsDone { get; private set; }

        /// <summary>Advance time. Returns true only on the tick the timer completes.</summary>
        public bool Tick(float deltaSeconds, bool stoppedAndEmpty)
        {
            if (IsDone) return false;
            if (!stoppedAndEmpty) { _elapsed = 0f; return false; }
            _elapsed += deltaSeconds;
            if (_elapsed < _required) return false;
            IsDone = true;
            return true;
        }

        public void Reset() { _elapsed = 0f; IsDone = false; }
    }
}
