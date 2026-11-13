using System.Collections.Generic;

namespace UncannyValet.Director
{
    /// <summary>Blessings earned over the last while, kept in a reusable queue so counting allocates nothing.</summary>
    public sealed class RecentBlessings
    {
        private const float RetentionSeconds = 180f;

        private readonly Queue<BlessingSample> _samples = new Queue<BlessingSample>();

        public void Clear() => _samples.Clear();

        public void Add(float atSeconds, int amount)
        {
            while (_samples.Count > 0 && atSeconds - _samples.Peek().At > RetentionSeconds) _samples.Dequeue();
            _samples.Enqueue(new BlessingSample(atSeconds, amount));
        }

        /// <summary>Blessings earned in the last <paramref name="windowSeconds"/> seconds.</summary>
        public int SumWithin(float nowSeconds, float windowSeconds)
        {
            var sum = 0;
            foreach (var s in _samples)
                if (nowSeconds - s.At <= windowSeconds) sum += s.Amount;
            return sum;
        }

        private readonly struct BlessingSample
        {
            public readonly float At;
            public readonly int Amount;
            public BlessingSample(float at, int amount) { At = at; Amount = amount; }
        }
    }
}
