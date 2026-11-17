using System;
using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Powers
{
    /// <summary>
    /// Turns time spent sliding into meter: a rate per second, capped per slide. Also totals what the bar really took
    /// per slide. No allocation after construction.
    /// </summary>
    public sealed class DriftFillTracker
    {
        private readonly PowerSettings _settings;
        private readonly List<CarId> _cars = new List<CarId>(8);
        private readonly List<float> _earned = new List<float>(8);
        private readonly List<float> _lastGain = new List<float>(8);
        private readonly List<float> _taken = new List<float>(8);

        public DriftFillTracker(PowerSettings settings) { _settings = settings; }

        /// <summary>Slides in progress.</summary>
        public int Active => _cars.Count;

        public void Begin(CarId car)
        {
            if (_cars.Contains(car)) return;
            _cars.Add(car);
            _earned.Add(0f);
            _lastGain.Add(0f);
            _taken.Add(0f);
        }

        /// <summary>Ends a slide and returns the meter the bar really took from it (0 when the car was not sliding).</summary>
        public float End(CarId car)
        {
            var i = _cars.IndexOf(car);
            if (i < 0) return 0f;
            var taken = _taken[i];
            RemoveAt(i);
            return taken;
        }

        public void Clear()
        {
            _cars.Clear();
            _earned.Clear();
            _lastGain.Clear();
            _taken.Clear();
        }

        /// <summary>Meter earned by every slide during this step.</summary>
        public float Tick(float deltaSeconds)
        {
            var total = 0f;
            for (var i = 0; i < _cars.Count; i++)
            {
                var gain = Math.Min(_settings.FillDriftPerSecond * deltaSeconds, _settings.FillDriftCap - _earned[i]);
                _lastGain[i] = Math.Max(gain, 0f);
                if (gain <= 0f) continue;
                _earned[i] += gain;
                total += gain;
            }
            return total;
        }

        /// <summary>Tells the tracker how much of the last Tick's <paramref name="offered"/> meter the bar took, shared out per slide.</summary>
        public void Credit(float taken, float offered)
        {
            if (offered <= 0f) return;
            var share = Math.Min(taken / offered, 1f);
            for (var i = 0; i < _cars.Count; i++) _taken[i] += _lastGain[i] * share;
        }

        private void RemoveAt(int i)
        {
            _cars.RemoveAt(i);
            _earned.RemoveAt(i);
            _lastGain.RemoveAt(i);
            _taken.RemoveAt(i);
        }
    }
}
