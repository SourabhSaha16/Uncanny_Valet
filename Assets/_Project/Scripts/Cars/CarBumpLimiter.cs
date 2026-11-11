using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Cars
{
    /// <summary>Lets one car pair report a bump at most once per interval, so a car rattling against another cannot spam events.</summary>
    public sealed class CarBumpLimiter
    {
        private const int PruneAbove = 64;

        private readonly float _interval;
        private readonly Dictionary<long, float> _last = new Dictionary<long, float>();
        private readonly List<long> _expired = new List<long>();

        public CarBumpLimiter(float interval) => _interval = interval;

        /// <summary>True (and remembers the time) when the pair a, b has not bumped within the interval; the order of a and b does not matter.</summary>
        public bool TryPass(CarId a, CarId b, float now)
        {
            var key = Key(a, b);
            if (_last.TryGetValue(key, out var last) && now - last < _interval) return false;
            _last[key] = now;
            if (_last.Count > PruneAbove) Prune(now);
            return true;
        }

        private static long Key(CarId a, CarId b)
        {
            var low = a.Value < b.Value ? a.Value : b.Value;
            var high = a.Value < b.Value ? b.Value : a.Value;
            return ((long)low << 32) | (uint)high;
        }

        private void Prune(float now)
        {
            _expired.Clear();
            foreach (var pair in _last)
                if (now - pair.Value >= _interval) _expired.Add(pair.Key);
            for (var i = 0; i < _expired.Count; i++) _last.Remove(_expired[i]);
        }
    }
}
