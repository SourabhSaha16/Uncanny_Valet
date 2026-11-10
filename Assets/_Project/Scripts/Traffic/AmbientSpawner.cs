using System;

namespace UncannyValet.Traffic
{
    /// <summary>
    /// Pure rule: decides when and what ambient car appears. Intervals are random around the mean implied by the rate,
    /// the lane and look are random, all from one seeded generator. A rate of 0 switches it off.
    /// </summary>
    public sealed class AmbientSpawner
    {
        private const float SecondsPerMinute = 60f;

        private readonly Random _random;
        private readonly int _lookCount;
        private readonly float _intervalSpread;
        private readonly float _eastShare;
        private readonly float _minSpeedScale;
        private readonly float _maxSpeedScale;
        private float _timer;
        private bool _armed;

        /// <param name="seed">Random seed.</param>
        /// <param name="lookCount">How many car looks exist (at least 1).</param>
        /// <param name="intervalSpread">0..1: how far an interval may stray from the mean (0 = exactly the mean).</param>
        /// <param name="eastShare">0..1: chance that a car takes the east lane.</param>
        /// <param name="minSpeedScale">Lowest speed multiplier.</param>
        /// <param name="maxSpeedScale">Highest speed multiplier.</param>
        public AmbientSpawner(int seed, int lookCount, float intervalSpread, float eastShare,
            float minSpeedScale, float maxSpeedScale)
        {
            _random = new Random(seed);
            _lookCount = Math.Max(1, lookCount);
            _intervalSpread = intervalSpread;
            _eastShare = eastShare;
            _minSpeedScale = minSpeedScale;
            _maxSpeedScale = maxSpeedScale;
        }

        /// <summary>
        /// Advances the timer. Returns true and fills <paramref name="request"/> when a car is due.
        /// <paramref name="carsPerMinute"/> at or below 0 disables spawning and resets the wait.
        /// </summary>
        public bool Tick(float dt, float carsPerMinute, out SpawnRequest request)
        {
            request = default;
            if (carsPerMinute <= 0f)
            {
                _armed = false;
                return false;
            }
            if (!_armed)
            {
                _timer = NextInterval(carsPerMinute);
                _armed = true;
            }
            _timer -= dt;
            if (_timer > 0f) return false;
            _timer = NextInterval(carsPerMinute);
            request = Roll();
            return true;
        }

        private float NextInterval(float carsPerMinute)
        {
            float mean = SecondsPerMinute / carsPerMinute;
            float offset = ((float)_random.NextDouble() * 2f - 1f) * _intervalSpread;
            return mean * (1f + offset);
        }

        private SpawnRequest Roll()
        {
            TrafficLane lane = _random.NextDouble() < _eastShare ? TrafficLane.East : TrafficLane.West;
            int look = _random.Next(_lookCount);
            float scale = _minSpeedScale + (float)_random.NextDouble() * (_maxSpeedScale - _minSpeedScale);
            return new SpawnRequest(lane, look, scale);
        }
    }
}
