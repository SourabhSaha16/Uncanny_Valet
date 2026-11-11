namespace UncannyValet.Powers
{
    /// <summary>Countdown shared by the timed powers. Starting it again refreshes it to the full length.</summary>
    public sealed class PowerTimer
    {
        /// <summary>Seconds left; 0 when stopped.</summary>
        public float Remaining { get; private set; }

        public bool IsRunning => Remaining > 0f;

        public void Start(float seconds) { Remaining = seconds > 0f ? seconds : 0f; }

        public void Stop() { Remaining = 0f; }

        /// <summary>Advances the timer; returns true on the step it ran out.</summary>
        public bool Tick(float deltaSeconds)
        {
            if (Remaining <= 0f) return false;
            Remaining -= deltaSeconds;
            if (Remaining > 0f) return false;
            Remaining = 0f;
            return true;
        }
    }
}
