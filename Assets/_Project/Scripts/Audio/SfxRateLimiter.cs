namespace UncannyValet.Audio
{
    /// <summary>Lets a sound through at most once per interval, so a pile-up of identical events cannot spam.</summary>
    public sealed class SfxRateLimiter
    {
        private readonly float _minInterval;
        private float _last = float.NegativeInfinity;

        public SfxRateLimiter(float minInterval) => _minInterval = minInterval;

        /// <summary>True (and remembers the time) when enough time passed since the last allowed call.</summary>
        public bool TryPass(float now)
        {
            if (now - _last < _minInterval) return false;
            _last = now;
            return true;
        }
    }
}
