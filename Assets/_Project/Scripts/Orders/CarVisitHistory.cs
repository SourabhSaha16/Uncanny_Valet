using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Orders
{
    /// <summary>Every spot each car was parked in, with the game minute, until its ticket closes.</summary>
    public sealed class CarVisitHistory
    {
        private readonly struct Visit
        {
            public readonly int Spot;
            public readonly float Minute;
            public Visit(int spot, float minute) { Spot = spot; Minute = minute; }
        }

        private readonly Dictionary<CarId, List<Visit>> _visits = new Dictionary<CarId, List<Visit>>();

        public void RecordParked(CarId car, int spotId, float minute)
        {
            if (!_visits.TryGetValue(car, out var list))
            {
                list = new List<Visit>();
                _visits[car] = list;
            }
            list.Add(new Visit(spotId, minute));
        }

        /// <summary>True if the car was parked in the spot at or after <paramref name="sinceMinute"/>.</summary>
        public bool HasVisited(CarId car, int spotId, float sinceMinute)
        {
            if (!_visits.TryGetValue(car, out var list)) return false;
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].Spot == spotId && list[i].Minute >= sinceMinute) return true;
            }
            return false;
        }

        public void Clear(CarId car) => _visits.Remove(car);

        public void ClearAll() => _visits.Clear();
    }
}
