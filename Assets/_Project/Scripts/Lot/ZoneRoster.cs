using System;
using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Lot
{
    /// <summary>What a stop zone sees of a car this frame.</summary>
    public enum CarReading { Gone, Active, Stopped }

    /// <summary>
    /// Pure bookkeeping for the cars inside a stop zone: one <see cref="StopTimer"/> per car, plus
    /// release of cars that vanish (destroyed) while inside.
    /// </summary>
    public sealed class ZoneRoster
    {
        private readonly float _settleSeconds;
        private readonly Dictionary<CarId, StopTimer> _timers = new Dictionary<CarId, StopTimer>();
        private readonly List<CarId> _scratch = new List<CarId>();

        public ZoneRoster(float settleSeconds) { _settleSeconds = settleSeconds; }

        public int Count => _timers.Count;

        public void Enter(CarId car)
        {
            if (!_timers.ContainsKey(car)) _timers[car] = new StopTimer(_settleSeconds);
        }

        /// <summary>Removes the car. Returns true if it had settled (so a leave event is due).</summary>
        public bool Exit(CarId car)
        {
            if (!_timers.TryGetValue(car, out var timer)) return false;
            _timers.Remove(car);
            return timer.IsDone;
        }

        /// <summary>Advances every car. Fills <paramref name="settled"/> and <paramref name="gone"/> (id, had settled).</summary>
        public void Tick(float dt, Func<CarId, CarReading> read, List<CarId> settled, List<(CarId id, bool wasSettled)> gone)
        {
            _scratch.AddRange(_timers.Keys);
            foreach (var id in _scratch)
            {
                var reading = read(id);
                if (reading == CarReading.Gone) gone.Add((id, Exit(id)));
                else if (_timers[id].Tick(dt, reading == CarReading.Stopped)) settled.Add(id);
            }
            _scratch.Clear();
        }
    }
}
