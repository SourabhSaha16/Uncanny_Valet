using System;

namespace UncannyValet.Orders
{
    /// <summary>Decides when the next order attempt is due: a random gap of game minutes after the last one.</summary>
    public sealed class OrderScheduler
    {
        /// <summary>Ticks come once per game minute, so a shorter gap would mean nothing.</summary>
        private const float MinGapMinutes = 1f;

        private readonly IOrderRandom _random;
        private float _nextAt = float.MaxValue;

        public OrderScheduler(IOrderRandom random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public bool IsArmed => _nextAt != float.MaxValue;

        /// <summary>Starts the clock: the first attempt is due one random gap after <paramref name="now"/>.</summary>
        public void Arm(float now, float gapMin, float gapMax) => _nextAt = now + RollGap(gapMin, gapMax);

        /// <summary>Stops attempts (orders are off tonight, or the night ended).</summary>
        public void Disarm() => _nextAt = float.MaxValue;

        /// <summary>True once per gap: when an attempt is due it also arms the next one.</summary>
        public bool TryConsume(float now, float gapMin, float gapMax)
        {
            if (now < _nextAt) return false;
            Arm(now, gapMin, gapMax);
            return true;
        }

        private float RollGap(float gapMin, float gapMax)
        {
            var low = Math.Min(gapMin, gapMax);
            var high = Math.Max(gapMin, gapMax);
            return Math.Max(MinGapMinutes, _random.Range(low, high));
        }
    }
}
