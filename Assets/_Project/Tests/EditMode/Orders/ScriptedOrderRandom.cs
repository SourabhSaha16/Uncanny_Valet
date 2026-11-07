using System.Collections.Generic;
using UncannyValet.Orders;

namespace UncannyValet.Tests.Orders
{
    /// <summary>Plays back the given picks and ranges in order; when they run out, picks index 0 and the range minimum.</summary>
    public sealed class ScriptedOrderRandom : IOrderRandom
    {
        private readonly Queue<int> _picks = new Queue<int>();
        private readonly Queue<float> _ranges = new Queue<float>();

        public ScriptedOrderRandom Picks(params int[] values)
        {
            foreach (var v in values) _picks.Enqueue(v);
            return this;
        }

        public ScriptedOrderRandom Gaps(params float[] values)
        {
            foreach (var v in values) _ranges.Enqueue(v);
            return this;
        }

        public int Next(int maxExclusive) => _picks.Count > 0 ? _picks.Dequeue() % maxExclusive : 0;

        public float Range(float min, float max) => _ranges.Count > 0 ? _ranges.Dequeue() : min;
    }
}
