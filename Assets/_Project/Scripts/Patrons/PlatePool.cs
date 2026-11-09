using System.Collections.Generic;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Hands out licence plates that are unique among the plates currently held. A plate is drawn at a random
    /// index from the injected source; if it is taken, the next free index is used. Release a plate when its ticket closes.
    /// </summary>
    public sealed class PlatePool
    {
        private readonly IPatronRandom _random;
        private readonly HashSet<string> _held = new HashSet<string>();

        public PlatePool(IPatronRandom random)
        {
            _random = random;
        }

        /// <summary>Number of plates currently held.</summary>
        public int Count => _held.Count;

        /// <summary>Takes a plate no other holder has. Deterministic for a given random source.</summary>
        public string Acquire()
        {
            var start = System.Math.Min(PlateFormat.Total - 1, (int)(_random.Value() * PlateFormat.Total));
            for (var i = 0; i < PlateFormat.Total; i++)
            {
                var plate = PlateFormat.FromIndex(start + i);
                if (_held.Add(plate)) return plate;
            }
            return PlateFormat.FromIndex(start); // every plate is taken; far beyond any real number of open tickets
        }

        /// <summary>Frees a plate for reuse. Unknown plates are ignored.</summary>
        public void Release(string plate)
        {
            if (plate != null) _held.Remove(plate);
        }

        /// <summary>Frees every plate.</summary>
        public void Reset() => _held.Clear();
    }
}
