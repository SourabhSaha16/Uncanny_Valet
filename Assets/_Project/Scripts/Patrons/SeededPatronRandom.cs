namespace UncannyValet.Patrons
{
    /// <summary>A small deterministic random source (xorshift32): the same seed always gives the same sequence.</summary>
    public sealed class SeededPatronRandom : IPatronRandom
    {
        private const uint ZeroSeedFallback = 0x9E3779B9u;
        private const float UnitScale = 1f / 16777216f;
        private const int ValueShift = 8;

        private uint _state;

        public SeededPatronRandom(int seed)
        {
            _state = (uint)seed;
            if (_state == 0u) _state = ZeroSeedFallback;
        }

        /// <summary>A value in [0, 1).</summary>
        public float Value()
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return (_state >> ValueShift) * UnitScale;
        }

        public float Range(float min, float max) => min + (max - min) * Value();
    }
}
