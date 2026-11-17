using System.Collections.Generic;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Deals the gag lines in a shuffled order, every line once before any repeats and never the same line twice in a row across a reshuffle.
    /// Seeded, so a run of the same seed hears the same jokes.
    /// </summary>
    public sealed class GagLineDeck
    {
        private readonly IReadOnlyList<string> _lines;
        private readonly int[] _order;
        private readonly System.Random _random;
        private int _next;
        private int _last = -1;

        public GagLineDeck(IReadOnlyList<string> lines, int seed)
        {
            _lines = lines;
            _order = new int[lines.Count];
            _random = new System.Random(seed);
            for (var i = 0; i < _order.Length; i++) _order[i] = i;
            Shuffle();
        }

        /// <summary>The next line, or null when there are no lines.</summary>
        public string Next()
        {
            if (_order.Length == 0) return null;
            if (_next >= _order.Length) Shuffle();
            var line = _order[_next++];
            _last = line;
            return _lines[line];
        }

        private void Shuffle()
        {
            for (var i = _order.Length - 1; i > 0; i--)
            {
                var j = _random.Next(i + 1);
                var swap = _order[i];
                _order[i] = _order[j];
                _order[j] = swap;
            }
            if (_order.Length > 1 && _order[0] == _last)
            {
                var swap = _order[0];
                _order[0] = _order[1];
                _order[1] = swap;
            }
            _next = 0;
        }
    }
}
