namespace UncannyValet.Director
{
    /// <summary>
    /// Collects world time for the director and says when its next tick is due. While paused (freeze time) nothing
    /// is collected, so no timer inside the director moves and no car can be sent. Pure.
    /// </summary>
    public sealed class DirectorTicker
    {
        private float _since;

        /// <summary>Forgets the time collected so far.</summary>
        public void Reset() => _since = 0f;

        /// <summary>
        /// Adds <paramref name="worldDelta"/> unless <paramref name="paused"/>. Returns true, with the world seconds since
        /// the last tick, once <paramref name="interval"/> has been collected.
        /// </summary>
        public bool TryTick(float worldDelta, bool paused, float interval, out float elapsed)
        {
            elapsed = 0f;
            if (paused) return false;
            _since += worldDelta;
            if (_since < interval) return false;
            elapsed = _since;
            _since = 0f;
            return true;
        }
    }
}
