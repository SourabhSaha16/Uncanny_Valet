namespace UncannyValet.Audio
{
    /// <summary>Deterministic white noise (xorshift32), so rendered audio is identical every run.</summary>
    public struct NoiseSource
    {
        private const float HalfRange = 2147483648f;

        private uint _state;

        public NoiseSource(uint seed) { _state = seed == 0 ? 1u : seed; }

        /// <summary>Next sample in [-1, 1].</summary>
        public float Next()
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return _state / HalfRange - 1f;
        }
    }
}
