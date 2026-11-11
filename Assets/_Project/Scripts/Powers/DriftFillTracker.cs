using System;
using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Powers
{
    /// <summary>Turns time spent sliding into meter: a rate per second, capped per slide. No allocation after construction.</summary>
    public sealed class DriftFillTracker
    {
        private readonly PowerSettings _settings;
        private readonly List<CarId> _cars = new List<CarId>(8);
        private readonly List<float> _earned = new List<float>(8);

        public DriftFillTracker(PowerSettings settings) { _settings = settings; }

        /// <summary>Slides in progress.</summary>
        public int Active => _cars.Count;

        public void Begin(CarId car)
        {
            if (_cars.Contains(car)) return;
            _cars.Add(car);
            _earned.Add(0f);
        }

        public void End(CarId car)
        {
            var i = _cars.IndexOf(car);
            if (i < 0) return;
            _cars.RemoveAt(i);
            _earned.RemoveAt(i);
        }

        public void Clear()
        {
            _cars.Clear();
            _earned.Clear();
        }

        /// <summary>Meter earned by every slide during this step.</summary>
        public float Tick(float deltaSeconds)
        {
            var total = 0f;
            for (var i = 0; i < _cars.Count; i++)
            {
                var gain = Math.Min(_settings.FillDriftPerSecond * deltaSeconds, _settings.FillDriftCap - _earned[i]);
                if (gain <= 0f) continue;
                _earned[i] += gain;
                total += gain;
            }
            return total;
        }
    }
}
