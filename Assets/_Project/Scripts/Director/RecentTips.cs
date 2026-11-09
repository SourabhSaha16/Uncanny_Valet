using System.Collections.Generic;

namespace UncannyValet.Director
{
    /// <summary>Tips earned over the last while, kept in a reusable queue so counting allocates nothing.</summary>
    public sealed class RecentTips
    {
        private const float RetentionSeconds = 180f;

        private readonly Queue<TipSample> _samples = new Queue<TipSample>();

        public void Clear() => _samples.Clear();

        public void Add(float atSeconds, int amount)
        {
            while (_samples.Count > 0 && atSeconds - _samples.Peek().At > RetentionSeconds) _samples.Dequeue();
            _samples.Enqueue(new TipSample(atSeconds, amount));
        }

        /// <summary>Dollars earned in the last <paramref name="windowSeconds"/> seconds.</summary>
        public int SumWithin(float nowSeconds, float windowSeconds)
        {
            var sum = 0;
            foreach (var s in _samples)
                if (nowSeconds - s.At <= windowSeconds) sum += s.Amount;
            return sum;
        }

        private readonly struct TipSample
        {
            public readonly float At;
            public readonly int Amount;
            public TipSample(float at, int amount) { At = at; Amount = amount; }
        }
    }
}
