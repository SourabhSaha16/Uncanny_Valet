using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Pedestrians
{
    /// <summary>Cars whose patron could not get a person yet (every one was out). They are served oldest first as people come back.</summary>
    public sealed class PendingStepOuts
    {
        private readonly List<CarId> _cars = new List<CarId>(8);
        private readonly List<bool> _summoned = new List<bool>(8);

        /// <summary>How many cars wait.</summary>
        public int Count => _cars.Count;

        /// <summary>The waiting car at <paramref name="index"/> (oldest first).</summary>
        public CarId CarAt(int index) => _cars[index];

        /// <summary>True when the car at <paramref name="index"/> was summoned while it waited.</summary>
        public bool SummonedAt(int index) => _summoned[index];

        /// <summary>True when <paramref name="car"/> already waits.</summary>
        public bool Contains(CarId car) => _cars.Contains(car);

        /// <summary>Queues a car; false (and no change) when it already waits.</summary>
        public bool Add(CarId car)
        {
            if (_cars.Contains(car)) return false;
            _cars.Add(car);
            _summoned.Add(false);
            return true;
        }

        /// <summary>Notes that the waiting car's patron was summoned meanwhile.</summary>
        public void MarkSummoned(CarId car)
        {
            var index = _cars.IndexOf(car);
            if (index >= 0) _summoned[index] = true;
        }

        /// <summary>Drops every waiting car.</summary>
        public void Clear()
        {
            _cars.Clear();
            _summoned.Clear();
        }

        /// <summary>Drops a car (served, or its ticket ended).</summary>
        public void Remove(CarId car)
        {
            var index = _cars.IndexOf(car);
            if (index < 0) return;
            _cars.RemoveAt(index);
            _summoned.RemoveAt(index);
        }
    }
}
