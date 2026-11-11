namespace UncannyValet.Valet
{
    /// <summary>
    /// Decides whether the valet barges a pedestrian this step: only while boosted, and never the same person again
    /// within a cooldown (the valet stays overlapped with them for several physics steps). Fixed-size, allocation-free.
    /// </summary>
    public sealed class BargeRule
    {
        private readonly int[] _ids;
        private readonly float[] _until;
        private readonly float _cooldown;
        private int _next;

        /// <param name="capacity">How many recently barged people are remembered.</param>
        /// <param name="cooldownSeconds">Seconds before the same person can be barged again.</param>
        public BargeRule(int capacity, float cooldownSeconds)
        {
            _ids = new int[capacity < 1 ? 1 : capacity];
            _until = new float[_ids.Length];
            _cooldown = cooldownSeconds;
        }

        /// <summary>True (and remembers the person) when a boosted valet overlapping <paramref name="id"/> at time <paramref name="now"/> barges them.</summary>
        public bool TryBarge(bool boosted, int id, float now)
        {
            if (!boosted || id == 0) return false;
            for (var i = 0; i < _ids.Length; i++)
                if (_ids[i] == id && _until[i] > now) return false;
            _ids[_next] = id;
            _until[_next] = now + _cooldown;
            _next = (_next + 1) % _ids.Length;
            return true;
        }

        /// <summary>Forgets everyone (a new shift).</summary>
        public void Clear()
        {
            for (var i = 0; i < _ids.Length; i++) { _ids[i] = 0; _until[i] = 0f; }
            _next = 0;
        }
    }
}
